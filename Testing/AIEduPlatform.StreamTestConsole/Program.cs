using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// ── Configuration ────────────────────────────────────────────────────
const string baseUrl = "https://localhost:7205";
const string email = "ahmed2@gmail.com";
const string password = "AhmedAshraf123#";

// ── HTTP Client Setup ─────────────────────────────────────────────────
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
using var http = new HttpClient(handler)
{
    BaseAddress = new Uri(baseUrl),
    Timeout = TimeSpan.FromMinutes(10)
};

// ── Banner ────────────────────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine("  Stream Chat Test Console");
Console.WriteLine("═══════════════════════════════════════════════");
Console.ResetColor();

// ── 1. Login ──────────────────────────────────────────────────────────
Console.WriteLine($"\n[*] Logging in as {email} ...");

var loginResponse = await http.PostAsJsonAsync("/api/auth/login", new { email, password });

if (!loginResponse.IsSuccessStatusCode)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[!] Login failed ({loginResponse.StatusCode}): {await loginResponse.Content.ReadAsStringAsync()}");
    Console.ResetColor();
    return;
}

var loginBody = await loginResponse.Content.ReadAsStringAsync();

var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthData>>(loginBody, options);

var accessToken = apiResponse?.Data?.AccessToken;

if (string.IsNullOrEmpty(accessToken))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[!] Could not extract access token. Response: {loginBody}");
    Console.ResetColor();
    return;
}

http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"[+] Logged in successfully. Token expires: {apiResponse!.Data!.AccessTokenExpiration}");
Console.ResetColor();

// ── 2. Ask for Session ID ─────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Cyan;
Console.Write("Enter Session ID: ");
Console.ResetColor();
var sessionId = Console.ReadLine()?.Trim();

if (string.IsNullOrEmpty(sessionId))
{
    sessionId = "019c8209-8a20-7c6b-9003-c78438e75181";
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"[+] Using session: {sessionId}");
Console.ResetColor();

// ── 3. Chat Loop ──────────────────────────────────────────────────────
Console.WriteLine("\nType your message and press Enter to send. Type 'exit' to quit.\n");

while (true)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("You: ");
    Console.ResetColor();

    var userMessage = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(userMessage)) continue;
    if (userMessage.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    var chatRequest = new { sessionId, message = userMessage };

    var request = new HttpRequestMessage(HttpMethod.Post, $"/api/study-sessions/{sessionId}/chat")
    {
        Content = JsonContent.Create(chatRequest)
    };

    HttpResponseMessage streamResponse;
    try
    {
        streamResponse = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }
    catch (HttpRequestException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[!] Connection failed: {ex.Message}");
        Console.ResetColor();
        continue;
    }

    if (!streamResponse.IsSuccessStatusCode)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[!] Request failed ({streamResponse.StatusCode}): {await streamResponse.Content.ReadAsStringAsync()}");
        Console.ResetColor();
        continue;
    }

    // ── Stream the Response ───────────────────────────────────────────
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("\nAI: ");
    Console.ResetColor();

    var fullResponse = new StringBuilder();

    await using var stream = await streamResponse.Content.ReadAsStreamAsync();
    using var reader = new StreamReader(stream);

    while (await reader.ReadLineAsync() is { } line)
    {
        if (!line.StartsWith("data: ")) continue;

        SseChunk? chunk;
        try { chunk = JsonSerializer.Deserialize<SseChunk>(line["data: ".Length..]); }
        catch (JsonException) { continue; }

        if (chunk is null) continue;

        if (!string.IsNullOrEmpty(chunk.Content))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(chunk.Content);
            Console.Out.Flush();
            fullResponse.Append(chunk.Content);
        }

        if (chunk.Done)
        {
            Console.WriteLine();
            if (chunk.Sources is { Count: > 0 })
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[Sources: {string.Join(", ", chunk.Sources)}]");
                Console.ResetColor();
            }
            break;
        }

        if (!string.IsNullOrEmpty(chunk.Error))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[!] Stream error: {chunk.Error}");
            Console.ResetColor();
            break;
        }
    }

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"({fullResponse.Length} chars)\n");
    Console.ResetColor();
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Goodbye!");
Console.ResetColor();

// ── Models ────────────────────────────────────────────────────────────
// ── Models ────────────────────────────────────────────────────────────
record ApiResponse<T>
{
    [JsonPropertyName("success")] public bool Success { get; init; }
    [JsonPropertyName("data")] public T? Data { get; init; }
    [JsonPropertyName("message")] public string? Message { get; init; }
}

record AuthData
{
    [JsonPropertyName("accessToken")] public string AccessToken { get; init; } = "";
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; init; } = "";
    [JsonPropertyName("accessTokenExpiration")] public string AccessTokenExpiration { get; init; } = "";
    [JsonPropertyName("refreshTokenExpiration")] public string RefreshTokenExpiration { get; init; } = "";
}

record SseChunk
{
    [JsonPropertyName("content")] public string? Content { get; init; }
    [JsonPropertyName("done")] public bool Done { get; init; }
    [JsonPropertyName("sources")] public List<string>? Sources { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
}
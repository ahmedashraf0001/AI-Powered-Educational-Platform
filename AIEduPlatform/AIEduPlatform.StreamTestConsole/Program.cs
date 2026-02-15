using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// ── Configuration ────────────────────────────────────────────────────
const string baseUrl = "https://localhost:7205";

// Login credentials
const string email = "ahmed2@gmail.com";
const string password = "AhmedAshraf123#";

// The exact request payload
var chatRequest = new
{
    sessionId = "019c5269-e4e9-7653-8c74-7349051a0ddd",
    message = "can you explain what is this video is talking about?",
    lectureId = "019c582d-78c3-770e-8647-5f50e2de964d",
    materialIds = new[] { "019c58d1-c7a8-78d4-bcd3-5f7cdd2b5726" }
};

// ── Main ─────────────────────────────────────────────────────────────
var handler = new HttpClientHandler
{
    // Trust the ASP.NET Core dev certificate for localhost
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
using var http = new HttpClient(handler)
{
    BaseAddress = new Uri(baseUrl),
    Timeout = TimeSpan.FromMinutes(10) // Streaming responses can take a while
};

// 1. Authenticate to get a JWT token
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine("  Stream Chat Test Console");
Console.WriteLine("═══════════════════════════════════════════════");
Console.ResetColor();

Console.WriteLine($"\n[*] Logging in as {email} ...");

var loginResponse = await http.PostAsJsonAsync("/api/auth/login", new { email, password });

if (!loginResponse.IsSuccessStatusCode)
{
    Console.ForegroundColor = ConsoleColor.Red;
    var errorBody = await loginResponse.Content.ReadAsStringAsync();
    Console.WriteLine($"[!] Login failed ({loginResponse.StatusCode}): {errorBody}");
    Console.ResetColor();
    Console.WriteLine("\n    Update the email/password constants in Program.cs to a valid student account.");
    return;
}

var loginBody = await loginResponse.Content.ReadAsStringAsync();

Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine($"[DEBUG] Raw login response: {loginBody[..Math.Min(200, loginBody.Length)]}...");
Console.ResetColor();

var authResult = JsonSerializer.Deserialize<AuthResponse>(loginBody, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
});

if (string.IsNullOrEmpty(authResult?.AccessToken))
{
    // Fallback: try to extract token from raw JSON in case of casing mismatch
    using var doc = JsonDocument.Parse(loginBody);
    var root = doc.RootElement;
    string? token = null;

    foreach (var prop in root.EnumerateObject())
    {
        if (prop.Name.Equals("accessToken", StringComparison.OrdinalIgnoreCase) ||
            prop.Name.Equals("token", StringComparison.OrdinalIgnoreCase))
        {
            token = prop.Value.GetString();
            break;
        }
    }

    if (string.IsNullOrEmpty(token))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[!] Could not find access token in login response.");
        Console.WriteLine($"    Available properties: {string.Join(", ", root.EnumerateObject().Select(p => p.Name))}");
        Console.ResetColor();
        return;
    }

    authResult = new AuthResponse { AccessToken = token };
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("[+] Logged in successfully.");
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine($"[DEBUG] Token: {authResult.AccessToken[..Math.Min(50, authResult.AccessToken.Length)]}...");
Console.ResetColor();

// 2. Set the Bearer token for subsequent requests
http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

// 3. Send the chat message and stream the SSE response
Console.WriteLine($"\n[*] Sending chat message to session {chatRequest.sessionId} ...");
Console.WriteLine($"    Message : \"{chatRequest.message}\"");
Console.WriteLine($"    Lecture : {chatRequest.lectureId}");
Console.WriteLine($"    Material: {chatRequest.materialIds[0]}\n");

var request = new HttpRequestMessage(HttpMethod.Post,
    $"/api/study-sessions/{chatRequest.sessionId}/chat")
{
    Content = JsonContent.Create(chatRequest)
};
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

HttpResponseMessage streamResponse;
try
{
    streamResponse = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
}
catch (HttpRequestException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[!] Connection failed: {ex.Message}");
    Console.WriteLine("    Make sure the API is running on " + baseUrl);
    Console.ResetColor();
    return;
}

if (!streamResponse.IsSuccessStatusCode)
{
    Console.ForegroundColor = ConsoleColor.Red;
    var errorBody = await streamResponse.Content.ReadAsStringAsync();
    Console.WriteLine($"[!] Chat request failed ({streamResponse.StatusCode}): {errorBody}");

    // Show WWW-Authenticate header for Unauthorized responses
    if (streamResponse.Headers.WwwAuthenticate.Any())
    {
        Console.WriteLine($"    WWW-Authenticate: {string.Join(", ", streamResponse.Headers.WwwAuthenticate)}");
    }

    Console.ResetColor();
    return;
}

// 4. Read SSE stream and append tokens in-order
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("--- AI Response (streamed) -----------------------");
Console.ResetColor();

var fullResponse = new StringBuilder();

await using var stream = await streamResponse.Content.ReadAsStreamAsync();
using var reader = new StreamReader(stream);

while (await reader.ReadLineAsync() is { } line)
{
    if (!line.StartsWith("data: "))
        continue;

    var json = line["data: ".Length..];

    SseChunk? chunk;
    try
    {
        chunk = JsonSerializer.Deserialize<SseChunk>(json);
    }
    catch (JsonException)
    {
        continue;
    }

    if (chunk is null)
        continue;

    // Append the token content as-is — subword pieces concatenate into full words
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
            Console.WriteLine($"\n[Sources: {string.Join(", ", chunk.Sources)}]");
            Console.ResetColor();
        }

        break;
    }
}

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("--------------------------------------------------");
Console.ResetColor();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"\n[+] Done — received {fullResponse.Length} characters total.");
Console.ResetColor();

// ── Models ───────────────────────────────────────────────────────────
record AuthResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; } = "";

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; init; } = "";
}

record SseChunk
{
    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("done")]
    public bool Done { get; init; }

    [JsonPropertyName("sources")]
    public List<string>? Sources { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}

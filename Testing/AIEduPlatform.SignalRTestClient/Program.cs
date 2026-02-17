using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIEduPlatform.SignalRTestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║   AI Educational Platform - SignalR Test Client       ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Get configuration from user
            Console.Write("Enter API Base URL (default: https://localhost:7205): ");
            var baseUrl = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "https://localhost:7205";
            }

            Console.Write("Enter your JWT token: ");
            var jwtToken = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ JWT token is required!");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                return;
            }

            // Build the SignalR connection
            var hubUrl = $"{baseUrl}/hubs/material-indexing";
            Console.WriteLine($"\n🔗 Connecting to: {hubUrl}");
            Console.WriteLine();

            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(jwtToken)!;
                    
                    // For development: skip SSL certificate validation
                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback =
                                (sender, certificate, chain, sslPolicyErrors) => true;
                        }
                        return handler;
                    };
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                })
                .Build();

            // Set up event handlers
            connection.Closed += async (error) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Connection closed: {error?.Message}");
                Console.ResetColor();
                await Task.CompletedTask;
            };

            connection.Reconnecting += async (error) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠️  Reconnecting: {error?.Message}");
                Console.ResetColor();
                await Task.CompletedTask;
            };

            connection.Reconnected += async (connectionId) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ Reconnected! ConnectionId: {connectionId}");
                Console.ResetColor();
                await Task.CompletedTask;
            };

            // Subscribe to notifications
            connection.On<object>("ReceiveIndexingNotification", (notification) =>
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n" + new string('═', 60));
                Console.WriteLine("📬 INDEXING NOTIFICATION RECEIVED");
                Console.WriteLine(new string('═', 60));
                Console.ResetColor();

                var json = JsonSerializer.Serialize(notification, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                Console.WriteLine(json);
                Console.WriteLine();
            });

            // Connect to the hub
            try
            {
                await connection.StartAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Connected successfully!");
                Console.WriteLine($"📍 ConnectionId: {connection.ConnectionId}");
                Console.WriteLine($"🔄 State: {connection.State}");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine(new string('─', 60));
                Console.WriteLine("👂 Listening for notifications...");
                Console.WriteLine("Press 'Q' to quit, 'S' to show status");
                Console.WriteLine(new string('─', 60));
                Console.WriteLine();

                // Keep the application running
                bool running = true;
                while (running)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        switch (key.Key)
                        {
                            case ConsoleKey.Q:
                                running = false;
                                break;
                            case ConsoleKey.S:
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"\n📊 Status: {connection.State}");
                                Console.WriteLine($"📍 ConnectionId: {connection.ConnectionId}");
                                Console.ResetColor();
                                Console.WriteLine();
                                break;
                        }
                    }
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Connection failed: {ex.Message}");
                Console.WriteLine($"   {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                }
                Console.ResetColor();
            }
            finally
            {
                // Cleanup
                if (connection.State == HubConnectionState.Connected)
                {
                    Console.WriteLine("\n🔌 Disconnecting...");
                    await connection.StopAsync();
                    await connection.DisposeAsync();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("✅ Disconnected gracefully");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}

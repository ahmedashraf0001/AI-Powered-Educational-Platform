using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIEduPlatform.SignalRTestClient
{
    class Program
    {
        // Track joined course groups for the student hub
        static readonly HashSet<string> _joinedCourseGroups = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      AI Educational Platform - SignalR Test Client           ║");
            Console.WriteLine("║  Teacher Hub: /hubs/material-indexing                        ║");
            Console.WriteLine("║  Student Hub: /hubs/student-notifications                    ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Get configuration from user
            Console.Write("Enter API Base URL (default: https://localhost:7205): ");
            var baseUrl = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "https://localhost:7205";

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

            // ── Build both hub connections ──────────────────────────────────────
            var teacherConnection = BuildConnection(baseUrl, "/hubs/material-indexing", jwtToken);
            var studentConnection = BuildConnection(baseUrl, "/hubs/student-notifications", jwtToken);

            // ── Teacher Hub events ──────────────────────────────────────────────
            WireConnectionLifecycle(teacherConnection, "Teacher");

            teacherConnection.On<object>("ReceiveIndexingNotification", (data) =>
                PrintNotification("📦 INDEXING COMPLETE", ConsoleColor.Cyan, data));

            teacherConnection.On<object>("ExamSubmitted", (data) =>
                PrintNotification("📝 EXAM SUBMITTED (teacher)", ConsoleColor.Yellow, data));

            teacherConnection.On<object>("NewEnrollment", (data) =>
                PrintNotification("🎓 NEW ENROLLMENT (teacher)", ConsoleColor.Green, data));

            teacherConnection.On<object>("NewReview", (data) =>
                PrintNotification("⭐ NEW REVIEW (teacher)", ConsoleColor.Magenta, data));

            teacherConnection.On<object>("EnrollmentCompleted", (data) =>
                PrintNotification("🏆 COURSE COMPLETED (teacher)", ConsoleColor.Green, data));

            teacherConnection.On<object>("StudentUnenrolled", (data) =>
                PrintNotification("🚪 STUDENT UNENROLLED (teacher)", ConsoleColor.DarkYellow, data));

            // ── Student Hub events ──────────────────────────────────────────────
            WireConnectionLifecycle(studentConnection, "Student");

            studentConnection.On<object>("NewExamPosted", (data) =>
                PrintNotification("📋 NEW EXAM POSTED (student)", ConsoleColor.Yellow, data));

            studentConnection.On<object>("NewMaterialUploaded", (data) =>
                PrintNotification("📎 NEW MATERIAL UPLOADED (student)", ConsoleColor.Cyan, data));

            studentConnection.On<object>("NewLectureAdded", (data) =>
                PrintNotification("📖 NEW LECTURE ADDED (student)", ConsoleColor.Cyan, data));

            studentConnection.On<object>("CourseUpdated", (data) =>
                PrintNotification("✏️  COURSE UPDATED (student)", ConsoleColor.Blue, data));

            studentConnection.On<object>("CoursePublished", (data) =>
                PrintNotification("🚀 COURSE PUBLISHED (student)", ConsoleColor.Green, data));

            studentConnection.On<object>("CourseUnpublished", (data) =>
                PrintNotification("🔒 COURSE UNPUBLISHED (student)", ConsoleColor.DarkRed, data));

            studentConnection.On<object>("ExamUpdated", (data) =>
                PrintNotification("🔄 EXAM UPDATED (student)", ConsoleColor.Yellow, data));

            studentConnection.On<object>("ExamDeleted", (data) =>
                PrintNotification("🗑️  EXAM CANCELLED (student)", ConsoleColor.Red, data));

            studentConnection.On<object>("SubmissionGraded", (data) =>
                PrintNotification("✅ SUBMISSION GRADED (student)", ConsoleColor.Green, data));

            studentConnection.On<object>("GradeApproved", (data) =>
                PrintNotification("✅ GRADE APPROVED (student)", ConsoleColor.Green, data));

            studentConnection.On<object>("GradeUpdated", (data) =>
                PrintNotification("🔄 GRADE REVISED (student)", ConsoleColor.Magenta, data));

            studentConnection.On<object>("EngagementAlert", (data) =>
                PrintNotification("⚠️  ENGAGEMENT ALERT (student)", ConsoleColor.Red, data));

            // ── Connect both hubs ───────────────────────────────────────────────
            try
            {
                Console.WriteLine($"🔗 Connecting to Teacher Hub: {baseUrl}/hubs/material-indexing");
                await teacherConnection.StartAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   ✅ Teacher Hub connected  (id: {teacherConnection.ConnectionId})");
                Console.ResetColor();

                Console.WriteLine($"🔗 Connecting to Student Hub: {baseUrl}/hubs/student-notifications");
                await studentConnection.StartAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   ✅ Student Hub connected  (id: {studentConnection.ConnectionId})");
                Console.ResetColor();
                Console.WriteLine();

                PrintHelp();

                // ── Interactive loop ────────────────────────────────────────────
                bool running = true;
                while (running)
                {
                    if (!Console.KeyAvailable)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    var key = Console.ReadKey(true);
                    switch (char.ToUpper(key.KeyChar))
                    {
                        case 'Q':
                            running = false;
                            break;

                        case 'S':
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"\n📊 Teacher Hub: {teacherConnection.State}  (id: {teacherConnection.ConnectionId})");
                            Console.WriteLine($"   Student Hub: {studentConnection.State}  (id: {studentConnection.ConnectionId})");
                            if (_joinedCourseGroups.Count > 0)
                                Console.WriteLine($"   Joined course groups: {string.Join(", ", _joinedCourseGroups)}");
                            else
                                Console.WriteLine("   No course groups joined yet.");
                            Console.ResetColor();
                            Console.WriteLine();
                            break;

                        case 'J':
                            Console.Write("\nEnter Course ID to join group: ");
                            var joinId = Console.ReadLine()?.Trim();
                            if (!string.IsNullOrWhiteSpace(joinId))
                            {
                                try
                                {
                                    await studentConnection.InvokeAsync("JoinCourseGroup", joinId);
                                    _joinedCourseGroups.Add(joinId);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"✅ Joined course group: {joinId}");
                                    Console.ResetColor();
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"❌ Failed to join: {ex.Message}");
                                    Console.ResetColor();
                                }
                            }
                            Console.WriteLine();
                            break;

                        case 'L':
                            Console.Write("\nEnter Course ID to leave group: ");
                            var leaveId = Console.ReadLine()?.Trim();
                            if (!string.IsNullOrWhiteSpace(leaveId))
                            {
                                try
                                {
                                    await studentConnection.InvokeAsync("LeaveCourseGroup", leaveId);
                                    _joinedCourseGroups.Remove(leaveId);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"✅ Left course group: {leaveId}");
                                    Console.ResetColor();
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"❌ Failed to leave: {ex.Message}");
                                    Console.ResetColor();
                                }
                            }
                            Console.WriteLine();
                            break;

                        case 'H':
                            PrintHelp();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Connection failed: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("\n🔌 Disconnecting...");
                if (teacherConnection.State != HubConnectionState.Disconnected)
                    await teacherConnection.StopAsync();
                if (studentConnection.State != HubConnectionState.Disconnected)
                    await studentConnection.StopAsync();
                await teacherConnection.DisposeAsync();
                await studentConnection.DisposeAsync();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("✅ Disconnected gracefully");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        static HubConnection BuildConnection(string baseUrl, string hubPath, string jwtToken)
        {
            return new HubConnectionBuilder()
                .WithUrl($"{baseUrl}{hubPath}", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken);
                    options.HttpMessageHandlerFactory = handler =>
                    {
                        if (handler is HttpClientHandler h)
                            h.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
                        return handler;
                    };
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                .ConfigureLogging(l => l.SetMinimumLevel(LogLevel.Warning))
                .Build();
        }

        static void WireConnectionLifecycle(HubConnection conn, string label)
        {
            conn.Closed += async error =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ [{label} Hub] Connection closed: {error?.Message}");
                Console.ResetColor();
                await Task.CompletedTask;
            };
            conn.Reconnecting += async error =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠️  [{label} Hub] Reconnecting: {error?.Message}");
                Console.ResetColor();
                await Task.CompletedTask;
            };
            conn.Reconnected += async id =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ [{label} Hub] Reconnected (id: {id})");
                Console.ResetColor();
                await Task.CompletedTask;
            };
        }

        static void PrintNotification(string title, ConsoleColor color, object data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            Console.ForegroundColor = color;
            Console.WriteLine($"\n[{timestamp}] ── {title} ──");
            Console.ResetColor();
            Console.WriteLine(json);
            Console.WriteLine();
        }

        static void PrintHelp()
        {
            Console.WriteLine(new string('─', 64));
            Console.WriteLine("👂 Listening on both hubs. All 17 notification events wired.");
            Console.WriteLine();
            Console.WriteLine("  Keys:");
            Console.WriteLine("    J  – Join a course group (receive course-broadcast events)");
            Console.WriteLine("    L  – Leave a course group");
            Console.WriteLine("    S  – Show connection status & joined groups");
            Console.WriteLine("    H  – Show this help");
            Console.WriteLine("    Q  – Quit");
            Console.WriteLine(new string('─', 64));
            Console.WriteLine();
        }
    }
}


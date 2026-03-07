using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Auth.Commands.RegisterStudent
{
    public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, Unit>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMailService _mailService;
        private readonly ILogger<RegisterStudentCommandHandler> _logger;

        public RegisterStudentCommandHandler(
            UserManager<UserEntity> userManager,
            IMailService mailService,
            ILogger<RegisterStudentCommandHandler> logger)
        {
            _userManager = userManager;
            _mailService = mailService;
            _logger = logger;
        }

        public async Task<Unit> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new BadRequestException("Email is already registered.");

            var existingUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserName != null)
                throw new BadRequestException("Username is already taken.");

            var nameParts = request.FullName.Trim().Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var verificationToken = Guid.NewGuid().ToString("N");

            var user = new UserEntity
            {
                Email = request.Email,
                UserName = request.UserName,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = false,
                IsEmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Registration failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "Student");

            _logger.LogInformation("Student registered: {Email}. Sending verification email.", request.Email);

            await SendVerificationEmail(user, verificationToken);

            return Unit.Value;
        }

        private async Task SendVerificationEmail(UserEntity user, string token)
        {
            var verificationLink = $"https://localhost:7189/api/auth/verify-email?token={token}&email={Uri.EscapeDataString(user.Email!)}";

            var subject = "Verify your email — AI Edu Platform";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .btn {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Verify Your Email</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {user.FirstName},</h2>
                            <p>Thank you for registering with AI Edu Platform as a student!</p>
                            <p>Please click the link below to verify your email address:</p>
                            <p><a href='{verificationLink}' class='btn'>Verify Email</a></p>
                            <p>This link expires in 24 hours.</p>
                            <p>If you did not create an account, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.UtcNow.Year} AI Edu Platform. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            try
            {
                await _mailService.SendEmailAsync(user.Email!, subject, body);
            }
            catch (Exception ex)
            {
                // Don't fail registration if email fails — user can request resend
                _logger.LogWarning(ex, "Failed to send verification email to {Email}", user.Email);
            }
        }
    }
}

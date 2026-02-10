using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Unit>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMailService _mailService;

        public RegisterCommandHandler(
            UserManager<UserEntity> userManager,
            IMailService mailService)
        {
            _userManager = userManager;
            _mailService = mailService;
        }

        public async Task<Unit> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new BadRequestException("Email is already registered.");
            }

            var existingUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserName != null)
            {
                throw new BadRequestException("Username is already taken.");
            }

            var user = new UserEntity
            {
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                EmailConfirmed = false,
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

            await SendWelcomeEmail(user);

            return Unit.Value;
        }

        private async Task SendWelcomeEmail(UserEntity user)
        {
            var subject = "Welcome to AI Edu Platform!";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to AI Edu Platform!</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {user.UserName},</h2>
                            <p>Thank you for registering with AI Edu Platform.</p>
                            <p>We're excited to have you on board and look forward to helping you achieve your learning goals!</p>
                            <p>Get started by exploring our features:</p>
                            <ul>
                                <li>AI-powered study sessions</li>
                                <li>Interactive flashcards</li>
                                <li>Mind mapping tools</li>
                                <li>Personalized quizzes</li>
                            </ul>
                        </div>
                        <div class='footer'>
                            <p>&copy; {DateTime.UtcNow.Year} AI Edu Platform. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await _mailService.SendEmailAsync(user.Email!, subject, body);
        }
    }
}

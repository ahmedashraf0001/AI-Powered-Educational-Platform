using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Auth;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IAuthTokenService _authTokenService;

        public LoginCommandHandler(
            UserManager<UserEntity> userManager,
            IAuthTokenService authTokenService)
        {
            _userManager = userManager;
            _authTokenService = authTokenService;
        }

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                throw new BadRequestException("Invalid email or password.");
            }

            var result = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!result)
            {
                throw new BadRequestException("Invalid email or password.");
            }

            return await _authTokenService.GenerateAuthTokensAsync(user);
        }
    }
}

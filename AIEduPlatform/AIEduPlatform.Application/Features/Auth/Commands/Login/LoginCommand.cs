using AIEduPlatform.Core.DTOs.Auth;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AIEduPlatform.Application.Features.Auth.Commands.Login
{
    public record LoginCommand : IRequest<AuthResponseDto>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}

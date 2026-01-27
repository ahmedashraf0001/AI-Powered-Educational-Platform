using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Auth
{
    public record ForgotPasswordRequestDto
    {
        [EmailAddress]
        public string Email { get; init; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Auth
{
    public record LoginDto
    {
        public string Email { get; init; }
        public string Password { get; init; }
    }
}

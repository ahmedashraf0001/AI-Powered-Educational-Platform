using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Auth
{
    public record RegisterDto
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Username { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
    }
}

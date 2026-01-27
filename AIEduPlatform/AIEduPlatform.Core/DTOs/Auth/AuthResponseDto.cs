using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Auth
{
    public record AuthResponseDto
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public DateTime AccessTokenExpiration { get; init; }
        public DateTime RefreshTokenExpiration { get; init; }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Identity
{
    public class AuthResult
    {
        public bool Succeeded { get; init; }
        public string AccessToken { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
        public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();
    }
}


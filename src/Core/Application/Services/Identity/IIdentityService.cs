using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Identity
{
    public interface IIdentityService
    {
        Task<AuthResult> RegisterAsync(string email, string password, CancellationToken ct = default);
        Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default);
    }
}


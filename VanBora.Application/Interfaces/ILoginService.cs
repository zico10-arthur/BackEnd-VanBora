using VanBora.Application.DTOs.Auth;
using VanBora.Domain.Common;
using VanBora.Domain.Entities;

namespace VanBora.Application.Interfaces;

public interface ILoginService
{
    Task<Result<(Usuario usuario, List<string> tipos)>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}

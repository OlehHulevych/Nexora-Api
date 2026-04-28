using Nexora.Domain.Entities;

namespace Nexora.Application.Interfaces.JwtService;

public interface IJwtService
{
    public Task<string?> CreateToken(ApplicationUser user);
}
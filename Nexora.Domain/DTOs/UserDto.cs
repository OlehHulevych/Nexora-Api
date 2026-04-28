using System.Runtime.InteropServices;

namespace Nexora.Domain.DTOs;

public record UserDto(
    string Id,
    string Name,
    string Email,
    [Optional]
    string? Photo,
    [Optional]
    string? Address
    );
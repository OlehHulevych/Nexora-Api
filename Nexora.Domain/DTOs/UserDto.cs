namespace Nexora.Domain.DTOs;

public record UserDto(
    string Id,
    string Name,
    string Email,
    string Photo,
    string Address
    );
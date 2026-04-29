namespace Nexora.Domain.DTOs;

public record ReviewDto(
    string Username,
    int? Rating,
    string Description
    );
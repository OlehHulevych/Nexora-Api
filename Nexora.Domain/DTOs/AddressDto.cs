namespace Nexora.Domain.DTOs;

public record AddressDto( string Line1,
    string? Line2,
    string City,
    string Country,
    string ZipCode);
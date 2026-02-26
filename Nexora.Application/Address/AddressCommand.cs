namespace Nexora.Application.Address;

public record AddressCommand(
    string Line1,
    string? Line2,

    string City,
    string Country,

    string? PostalCode 
    );
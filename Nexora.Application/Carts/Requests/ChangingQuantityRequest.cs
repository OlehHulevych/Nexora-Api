namespace Nexora.Application.Carts.Requests;

public record ChangingQuantityRequest(Guid cartItemId, string action);
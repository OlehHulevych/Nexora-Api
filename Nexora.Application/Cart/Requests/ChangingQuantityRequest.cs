namespace Nexora.Application.Cart.Requests;

public record ChangingQuantityRequest(Guid cartItemId, string action);
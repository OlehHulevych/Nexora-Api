namespace Nexora.Domain.Exceptions;

public class NotFoundException:Exception
{
    public NotFoundException(string name, object key):base($"Entity '{name}' with '{key}' was not found.") { }
}
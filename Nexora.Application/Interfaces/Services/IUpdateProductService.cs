using Nexora.Application.Product.Command;

namespace Nexora.Application.Interfaces.Services;

public interface IUpdateProductService
{
    public  Task<Guid> UpdateProductHandler(UpdateProductCommand request);
}
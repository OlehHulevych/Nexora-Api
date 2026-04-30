using Nexora.Application.Product.Command;

namespace Nexora.Application.Interfaces.Services;

public interface ICreateProduct
{
    public  Task<Listing> AddProduct(CreateProductCommand data, string id);

}
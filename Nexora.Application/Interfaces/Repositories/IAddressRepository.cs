

namespace Nexora.Application.Interfaces.Repositories;

public interface IAddressRepository
{
    public Task<bool> AddAsync(Address address);
    public Task<bool> UpdateAsync(Address address);
    public Task<Address?> GetAddressById(Guid id);
    public Task<Address?> GetAddressByUserId(string id);
    public Task<List<Address>> GetUserAddressesAsync(string userId);


}
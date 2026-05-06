namespace Nexora.Application.Interfaces.Repositories;

public interface IAddressRepository
{
    public Task<bool> AddAsync(Domain.Entities.Address address);
    public Task<bool> UpdateAsync(Domain.Entities.Address address);
    public Task<Domain.Entities.Address?> GetAddressById(Guid id);
    public Task<Domain.Entities.Address?> GetAddressByUserId(string id);
    public Task<List<Domain.Entities.Address>> GetUserAddressesAsync(string userId);


}
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class AddressRepository:IAddressRepository
{
    private readonly IApplicationDbContext _context;

    public AddressRepository(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> AddAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
        var result = await _context.SaveChangesAsync();
        return result > 0;

    }

    public async Task<bool> UpdateAsync(Address address)
    {
        _context.Addresses.Update(address);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Address?> GetAddressById(Guid id)
    {
        Address? address = await _context.Addresses.FirstOrDefaultAsync(a=>a.Id == id);
        if (address == null) throw new NotFoundException(nameof(Address), id);
        return address;
    }

    public async Task<Address?> GetAddressByUserId(string id)
    {
        Address? address = await _context.Addresses.FirstOrDefaultAsync(a=>a.UserId == id);
        if (address == null) throw new NotFoundException(nameof(Address), id);
        return address;
    }
    

    public async Task<List<Address>> GetUserAddressesAsync(string userId)
    {
        var addresses = await _context.Addresses.Where(a=>a.UserId==userId).ToListAsync();
        if (!addresses.Any()) throw new BadHttpRequestException("Failed to fetch user addresses");
        return addresses;
    }
}
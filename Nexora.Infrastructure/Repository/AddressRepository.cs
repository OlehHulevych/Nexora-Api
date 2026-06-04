using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Domain.Entities;
using Nexora.Domain.Exceptions;

namespace Nexora.Infrastructure.Repository;

public class AddressRepository : IAddressRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AddressRepository> _logger;

    public AddressRepository(IApplicationDbContext context, ILogger<AddressRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AddAsync(Address address)
    {
        _logger.LogInformation("Adding address for user {UserId}", address.UserId);
        try
        {
            await _context.Addresses.AddAsync(address);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Address {AddressId} added successfully for user {UserId}", address.Id, address.UserId);
            else
                _logger.LogWarning("Address for user {UserId} was not saved", address.UserId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add address for user {UserId}", address.UserId);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Address address)
    {
        _logger.LogInformation("Updating address {AddressId} for user {UserId}", address.Id, address.UserId);
        try
        {
            _context.Addresses.Update(address);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                _logger.LogInformation("Address {AddressId} updated successfully", address.Id);
            else
                _logger.LogWarning("Address {AddressId} was not updated", address.Id);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update address {AddressId}", address.Id);
            throw;
        }
    }

    public async Task<Address?> GetAddressById(Guid id)
    {
        _logger.LogInformation("Fetching address {AddressId}", id);
        Address? address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id);
        if (address == null)
        {
            _logger.LogWarning("Address {AddressId} not found", id);
            throw new NotFoundException(nameof(Address), id);
        }
        return address;
    }

    public async Task<Address?> GetAddressByUserId(string id)
    {
        _logger.LogInformation("Fetching address for user {UserId}", id);
        Address? address = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == id);
        if (address == null)
        {
            _logger.LogWarning("Address not found for user {UserId}", id);
            throw new NotFoundException(nameof(Address), id);
        }
        return address;
    }

    public async Task<List<Address>> GetUserAddressesAsync(string userId)
    {
        _logger.LogInformation("Fetching all addresses for user {UserId}", userId);
        try
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (!addresses.Any())
            {
                _logger.LogWarning("No addresses found for user {UserId}", userId);
                throw new BadHttpRequestException("Failed to fetch user addresses");
            }

            _logger.LogInformation("Fetched {Count} addresses for user {UserId}", addresses.Count, userId);
            return addresses;
        }
        catch (Exception ex) when (ex is not BadHttpRequestException)
        {
            _logger.LogError(ex, "Failed to fetch addresses for user {UserId}", userId);
            throw;
        }
    }
}
using System;
using System.Threading.Tasks;
using Nexora.Domain.Entities;

namespace Nexora.Application.Interfaces.Repositories;

public interface ICartRepository:IBaseRepository<Cart, Guid>
{
    public Task<Cart?> GetByUserId(string id);
   
}
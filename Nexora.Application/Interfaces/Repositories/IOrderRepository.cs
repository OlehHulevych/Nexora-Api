using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexora.Domain.Entities;

namespace Nexora.Application.Interfaces.Repositories;

public interface IOrderRepository:IBaseRepository<Order, Guid>
{
   
    public Task<List<Order>> GetByUser(string id);
   

}
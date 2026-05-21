using System;
using System.Threading.Tasks;
using Nexora.Domain.Entities;

namespace Nexora.Application.Interfaces.Repositories;

public interface IAvatarRepository
{
    public Task<bool> Add(Avatar avatar);
    public Task<bool> Update(Avatar avatar);
    public Task<Avatar?> GetById(Guid id);
    public Task<Avatar?> GetByUserId(string id);
    public Task<bool> Delete(Guid id);
}
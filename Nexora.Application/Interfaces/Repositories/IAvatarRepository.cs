namespace Nexora.Application.Interfaces.Repositories;

public interface IAvatarRepository
{
    public Task<bool> Add(Avatar avatar);
    public Task<bool> Update(Avatar avatar);
    public Task<Avatar?> GetById(Guid id);
}
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Domain.Entities;

namespace Nexora.Application.Users.Commands.UploadAvatar;

public class UploadAvatarHandler
{
    private readonly IBlobStorage _userBlobStorage;
    private readonly IApplicationDbContext _context;
    public UploadAvatarHandler(IBlobStorage userBlobStorage, IApplicationDbContext context)
    {
        _userBlobStorage = userBlobStorage;
        _context = context;
    }
    public async Task<UploadAvatarResponse> UploadAvatar(UploadAvatarCommand avatarCommand)
    {
        var avatarUri = await _userBlobStorage.UploadAsync(avatarCommand.File, "avatars");
        if (avatarUri.IsNullOrEmpty())
        {
            throw new Exception("Failed to upload avatar");
        }
        Avatar avatar = new Avatar
        {
            UserId = avatarCommand.UserId,
            User = avatarCommand.User,
            Uri = avatarUri
            
        };
        await _context.Avatars.AddAsync(avatar);
        await _context.SaveChangesAsync();
        return new UploadAvatarResponse(avatar, avatarUri);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Domain.Entities;

namespace Nexora.Application.Users.Commands.UploadAvatar;

public class AvatarService
{
    private readonly IUserBlobStorage _userUserBlobStorage;
    private readonly IApplicationDbContext _context;
    public AvatarService(IUserBlobStorage userUserBlobStorage, IApplicationDbContext context)
    {
        _userUserBlobStorage = userUserBlobStorage;
        _context = context;
    }
    public async Task<UploadAvatarResponse> UploadAvatar(UploadAvatarCommand avatarCommand, string username)
    {
        var (avatarUri,filePath) = await _userUserBlobStorage.UploadAsync(avatarCommand.File, $"avatars/{username}");
        if (avatarUri.IsNullOrEmpty())
        {
            throw new Exception("Failed to upload avatar");
        }
        Avatar avatar = new Avatar
        {
            UserId = avatarCommand.UserId,
            User = avatarCommand.User,
            Uri = avatarUri,
            FilePath = filePath
            
        };
        await _context.Avatars.AddAsync(avatar);
        await _context.SaveChangesAsync();
        return new UploadAvatarResponse(avatar, avatarUri);
    }
    public async Task<UploadAvatarResponse> UpdateAvatar(UploadAvatarCommand avatarCommand, ApplicationUser user , Avatar avatar)
    {
        var (avatarUri,filePath) = await _userUserBlobStorage.UploadAsync(avatarCommand.File, $"avatars/{user.FirstName + "_" + user.LastName}");
        if (avatarUri.IsNullOrEmpty())
        {
            throw new Exception("Failed to upload avatar");
        }
        avatar.Uri = avatarUri;
        avatar.FilePath = filePath;
        _context.Avatars.Update(avatar);
        await _context.SaveChangesAsync();
        return new UploadAvatarResponse(avatar, avatarUri);
    }
}
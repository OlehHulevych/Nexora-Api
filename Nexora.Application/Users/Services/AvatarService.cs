using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.UploadAvatar;

namespace Nexora.Application.Users.Services;

public class AvatarService:IAvatarService
{
    private readonly IUserBlobStorage _userUserBlobStorage;
    private readonly IAvatarRepository _avatarRepository;
    public AvatarService(IUserBlobStorage userUserBlobStorage, IAvatarRepository avatarRepository)
    {
        _userUserBlobStorage = userUserBlobStorage;
        _avatarRepository = avatarRepository;
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
        await _avatarRepository.Add(avatar);
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
        await _avatarRepository.Update(avatar);
        return new UploadAvatarResponse(avatar, avatarUri);
    }
}
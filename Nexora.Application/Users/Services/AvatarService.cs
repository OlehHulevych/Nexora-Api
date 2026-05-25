using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Users.Commands.UploadAvatar;

namespace Nexora.Application.Users.Services;

public class AvatarService : IAvatarService
{
    private readonly IUserBlobStorage _userUserBlobStorage;
    private readonly IAvatarRepository _avatarRepository;
    private readonly ILogger<AvatarService> _logger;

    public AvatarService(IUserBlobStorage userUserBlobStorage, IAvatarRepository avatarRepository, ILogger<AvatarService> logger)
    {
        _userUserBlobStorage = userUserBlobStorage;
        _avatarRepository = avatarRepository;
        _logger = logger;
    }

    public async Task<UploadAvatarResponse> UploadAvatar(UploadAvatarCommand avatarCommand, string username)
    {
        _logger.LogInformation("Uploading avatar for user {UserId} with username {Username}", avatarCommand.UserId, username);

        var (avatarUri, filePath) = await _userUserBlobStorage.UploadAsync(avatarCommand.File, $"avatars/{username}");
        if (avatarUri.IsNullOrEmpty())
        {
            _logger.LogError("Avatar upload failed for user {UserId} — blob storage returned empty URI", avatarCommand.UserId);
            throw new Exception("Failed to upload avatar");
        }

        _logger.LogInformation("Avatar uploaded to blob storage for user {UserId}: {AvatarUri}", avatarCommand.UserId, avatarUri);

        Avatar avatar = new Avatar
        {
            UserId = avatarCommand.UserId,
            User = avatarCommand.User,
            Uri = avatarUri,
            FilePath = filePath
        };

        await _avatarRepository.Add(avatar);
        _logger.LogInformation("Avatar saved to database for user {UserId}", avatarCommand.UserId);

        return new UploadAvatarResponse(avatar, avatarUri);
    }

    public async Task<UploadAvatarResponse> UpdateAvatar(UploadAvatarCommand avatarCommand, ApplicationUser user, Avatar avatar)
    {
        _logger.LogInformation("Updating avatar for user {UserId}", user.Id);

        var (avatarUri, filePath) = await _userUserBlobStorage.UploadAsync(avatarCommand.File, $"avatars/{user.FirstName + "_" + user.LastName}");
        if (avatarUri.IsNullOrEmpty())
        {
            _logger.LogError("Avatar update failed for user {UserId} — blob storage returned empty URI", user.Id);
            throw new Exception("Failed to upload avatar");
        }

        _logger.LogInformation("New avatar uploaded to blob storage for user {UserId}: {AvatarUri}", user.Id, avatarUri);

        avatar.Uri = avatarUri;
        avatar.FilePath = filePath;
        await _avatarRepository.Update(avatar);

        _logger.LogInformation("Avatar updated in database for user {UserId}", user.Id);

        return new UploadAvatarResponse(avatar, avatarUri);
    }
}
using Nexora.Application.Users.Commands.UploadAvatar;

namespace Nexora.Application.Interfaces.Services;

public interface IAvatarService
{
    public Task<UploadAvatarResponse> UploadAvatar(UploadAvatarCommand avatarCommand, string username);

    public Task<UploadAvatarResponse> UpdateAvatar(UploadAvatarCommand avatarCommand, ApplicationUser user,
        Avatar avatar);
}
using Nexora.Domain.Entities;

namespace Nexora.Application.Users.Commands.UploadAvatar;

public record UploadAvatarResponse
(
    Avatar Avatar,
    string uri
);
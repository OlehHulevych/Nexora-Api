using Microsoft.AspNetCore.Http;
using Nexora.Domain.Entities;

namespace Nexora.Application.Users.Commands.UploadAvatar;

public record UploadAvatarCommand
(
    string UserId,
    ApplicationUser User,
    IFormFile File
    
);
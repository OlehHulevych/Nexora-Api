using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nexora.Domain.Entities;

namespace Nexora.Application.Interfaces.IBlobStorage;


public interface IUserBlobStorage
{
   Task<(string,string)> UploadAsync(IFormFile file, string folder,  CancellationToken ct = default);
   Task<(string,string)> UpdateAsync(IFormFile file, ApplicationUser user, Avatar avatarForUpdate, CancellationToken ct = default);

}
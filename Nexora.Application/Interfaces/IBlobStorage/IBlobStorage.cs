using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nexora.Application.Interfaces.IBlobStorage;


public interface IBlobStorage
{
   Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
   
}
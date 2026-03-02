using Nexora.Domain.Common;

namespace Nexora.Domain.Entities;

public class Avatar:BaseEntity
{
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string Uri { get; set; }
}
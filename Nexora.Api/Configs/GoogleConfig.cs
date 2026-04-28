using System.ComponentModel.DataAnnotations;
using Nexora.Application.Interfaces.Config;

namespace Nexora.Api.Configs;

public class GoogleConfig:IGoogleConfig
{
    [Required]
    public string ClientId { get; set; }
    
    [Required]
    public string ClientSecret { get; set; }
}
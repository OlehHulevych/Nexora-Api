using Nexora.Application.Interfaces.Config;

namespace Nexora.Api.Configs;

public class GoogleConfig:IGoogleConfig
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
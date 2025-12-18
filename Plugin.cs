using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Extensions.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoiceAction.Actions;
using VoiceAction.Controls;

namespace VoiceAction;

public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        services.AddAction<AdjustVolumeAction, AdjustVolumeSettingsControl>();
        services.AddAction<MuteAction>();
        services.AddAction<UnmuteAction>();
    }
}
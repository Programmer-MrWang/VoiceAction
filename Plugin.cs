using AvaloniaEdit.Utils;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Core.Models.Automation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoiceAction.Actions;
using VoiceAction.Controls;

namespace VoiceAction;

public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {

        //行动树
        IActionService.ActionMenuTree.Add(new ActionMenuTreeGroup("调整音量", "\uF019"));
        IActionService.ActionMenuTree["调整音量"].AddRange([
            new ActionMenuTreeItem("VoiceAction.AdjustVolume", "调整音量", "\uF013"),
            new ActionMenuTreeItem("VoiceAction.Mute", "静音", "\uF015"),
            new ActionMenuTreeItem("VoiceAction.Unmute", "取消静音", "\uF00D")
        ]);


        services.AddAction<AdjustVolumeAction, AdjustVolumeSettingsControl>();
        services.AddAction<MuteAction>();
        services.AddAction<UnmuteAction>();
    }
}
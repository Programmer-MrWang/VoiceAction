using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Core.Models.Automation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoiceAction.UN.Actions;
using VoiceAction.SET.Actions;
using VoiceAction.M.Actions;
using AvaloniaEdit.Utils;

namespace VoiceAction;

public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // ========== 构建行动树 ==========
        BuildActionTree();

        // ========== 注册行动 ==========
        services.AddAction<SetVolumeAction, SetVolumeSettingsControl>();
        services.AddAction<MuteVolumeAction>();
        services.AddAction<UnmuteVolumeAction>();
    }

    private void BuildActionTree()
    {
        IActionService.ActionMenuTree.Add(
            new ActionMenuTreeGroup("系统音量控制…", "\uF013"));

        IActionService.ActionMenuTree["系统音量控制…"].AddRange([
            new ActionMenuTreeItem("VoiceAction.AdjustVolume", "设置系统音量", "\uF013"),
            new ActionMenuTreeItem("VoiceAction.Mute", "系统静音", "\uF015"),
            new ActionMenuTreeItem("VoiceAction.Unmute", "取消系统静音", "\uF00D")
        ]);
    }
}
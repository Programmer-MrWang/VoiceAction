using System;
using System.IO;
using System.Threading.Tasks;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace VoiceAction.Actions;

[ActionInfo("VoiceAction.Mute", "静音", "\uF015",false)]
public class MuteAction : ActionBase
{

    private readonly ILogger<MuteAction> _logger;

    public MuteAction(ILogger<MuteAction> logger)
    {
        _logger = logger;
    }

    protected override async Task OnInvoke()
    {
        string pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        string batPath = Path.Combine(pluginDir, "jingyin.bat");

        string batContent = $@"@echo off
setlocal
set ""CUSTOM_MODULE_PATH={pluginDir}\AudioDeviceCmdlets.psd1""

powershell -WindowStyle Hidden -NoProfile -Command ""Set-ExecutionPolicy Bypass -Scope CurrentUser -Force; Import-Module '%CUSTOM_MODULE_PATH%'; Set-AudioDevice -PlaybackMute $true""";

        await File.WriteAllTextAsync(batPath, batContent);

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = batPath,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        using var proc = System.Diagnostics.Process.Start(psi);
        if (proc != null) await proc.WaitForExitAsync();

        await base.OnInvoke();
    }
}
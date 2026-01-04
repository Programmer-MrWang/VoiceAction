using System;
using System.IO;
using System.Threading.Tasks;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;
using VoiceAction.Settings;

namespace VoiceAction.Actions;

[ActionInfo("VoiceAction.AdjustVolume", "调整音量", "\uF013", false)]
public class AdjustVolumeAction : ActionBase<AdjustVolumeSettings> 
{
    private readonly ILogger<AdjustVolumeAction> _logger;

    public AdjustVolumeAction(ILogger<AdjustVolumeAction> logger)
    {
        _logger = logger;
    }

    protected override async Task OnInvoke()
    {
        int volume = Math.Clamp(Settings.Volume, 0, 100);

        string pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        string batPath = Path.Combine(pluginDir, "tiaozheng.bat");

        string batContent = $@"@echo off
setlocal
set ""CUSTOM_MODULE_PATH={pluginDir}\AudioDeviceCmdlet.psd1""
powershell -WindowStyle Hidden -NoProfile -Command ""Set-ExecutionPolicy Bypass -Scope CurrentUser -Force; Import-Module '%CUSTOM_MODULE_PATH%'; Set-AudioDevice -PlaybackMute $false""

powershell -WindowStyle Hidden -NoProfile -Command ""Set-ExecutionPolicy Bypass -Scope CurrentUser -Force; Import-Module '%CUSTOM_MODULE_PATH%'; Set-AudioDevice -PlaybackVolume {volume}""";

        await File.WriteAllTextAsync(batPath, batContent);
        _logger.LogInformation("已生成脚本: {Path}, 音量: {Volume}", batPath, volume);

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
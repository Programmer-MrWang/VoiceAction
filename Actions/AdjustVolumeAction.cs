using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;
using VoiceAction.Settings;

namespace VoiceAction.Actions;

[ActionInfo("VoiceAction.AdjustVolume", "调整音量", "\uF013")]
public class AdjustVolumeAction : ActionBase
{
    private readonly ILogger<AdjustVolumeAction> _logger;

    public AdjustVolumeAction(ILogger<AdjustVolumeAction> logger)
    {
        _logger = logger;
    }

    protected override async Task OnInvoke()
    {
        // 优先从文件读取，实现用户输入立即生效
        int volume = await ReadVolumeFromFile();

        // 兼容回退：文件不存在或无效时使用 Settings
        if (volume == -1)
        {
            if (ActionItem.Settings is not AdjustVolumeSettings settings)
            {
                _logger.LogWarning("ActionItem.Settings 为空或类型不匹配");
                return;
            }
            volume = Math.Clamp(settings.Volume, 0, 100);
        }

        string pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        string batPath = Path.Combine(pluginDir, "tiaozheng.bat");

        string batContent = $@"@echo off
setlocal
set ""CUSTOM_MODULE_PATH={pluginDir}\AudioDeviceCmdlets.psd1""
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

    private async Task<int> ReadVolumeFromFile()
    {
        try
        {
            string filePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "int.json");

            if (!File.Exists(filePath))
                return -1;

            // 轻量解析，避免复杂类型开销
            string json = await File.ReadAllTextAsync(filePath);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("volume", out var element) &&
                element.TryGetInt32(out int volume))
            {
                return Math.Clamp(volume, 0, 100);
            }
            return -1;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "读取 int.json 失败，使用默认 Settings");
            return -1; // 兼容回退
        }
    }
}
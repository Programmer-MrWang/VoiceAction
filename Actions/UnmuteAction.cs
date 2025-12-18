using System;
using System.IO;
using System.Threading.Tasks;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace VoiceAction.Actions;

[ActionInfo("VoiceAction.Unmute", "取消静音", "\ue767")]
public class UnmuteAction : ActionBase
{

    private readonly ILogger<UnmuteAction> _logger;

    public UnmuteAction(ILogger<UnmuteAction> logger)
    {
        _logger = logger;
    }

    protected override async Task OnInvoke()
    {
        string pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        string batPath = Path.Combine(pluginDir, "quxiaojingyin.bat");

        string batContent = $@"@echo off
cd /d ""{pluginDir}""
powershell.exe -Command ""Set-AudioDevice -PlaybackMute $false""";

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
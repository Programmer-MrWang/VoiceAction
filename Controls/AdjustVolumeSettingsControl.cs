using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using ClassIsland.Core.Abstractions.Controls;
using VoiceAction.Settings;

namespace VoiceAction.Controls;

public class AdjustVolumeSettingsControl : ActionSettingsControlBase
{
    public AdjustVolumeSettings Settings => SettingsInternal;
    public AdjustVolumeSettings Control => SettingsInternal;
    public AdjustVolumeSettings SettingsInternal { get; private set; }

    public AdjustVolumeSettingsControl()
    {
        SettingsInternal = new AdjustVolumeSettings();

        var panel = new StackPanel { Spacing = 10, Margin = new(10) };

        panel.Children.Add(new TextBlock
        {
            Text = "音量 (0-100):",
            FontWeight = Avalonia.Media.FontWeight.Bold
        });

        var box = new TextBox
        {
            Width = 60,
            [!TextBox.TextProperty] = new Binding(nameof(Settings.Volume))
        };

        // 当 TextBox 失去焦点时验证并保存
        box.LostFocus += async (sender, e) => await OnVolumeInputChanged(box.Text);

        panel.Children.Add(box);

        Content = panel;
    }

    private async Task OnVolumeInputChanged(string? inputText)
    {
        // 验证输入是否为 0-100 的整数
        if (!int.TryParse(inputText, out int volume) || volume < 0 || volume > 100)
        {
            return; // 验证失败，静默返回以保证兼容性
        }

        await SaveVolumeToFile(volume);
    }

    private async Task SaveVolumeToFile(int volume)
    {
        try
        {
            string filePath = Path.Combine(GetPluginDirectory(), "int.json");
            // 极简 JSON，避免额外依赖
            string json = $"{{\"volume\":{volume}}}";
            await File.WriteAllTextAsync(filePath, json);
        }
        catch
        {
            // 保存失败不影响主功能，保证兼容性
        }
    }

    private string GetPluginDirectory()
    {
        return Path.GetDirectoryName(GetType().Assembly.Location)!;
    }
}
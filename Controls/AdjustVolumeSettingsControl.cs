using System;
using System.IO;
using System.Text.Json;
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

        LoadVolumeOnInitialize();

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

        box.LostFocus += async (sender, e) => await OnVolumeInputChanged(box.Text);

        panel.Children.Add(box);
        Content = panel;
    }

    private void LoadVolumeOnInitialize()
    {
        try
        {
            string filePath = Path.Combine(GetPluginDirectory(), "int.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("volume", out var element) &&
                    element.TryGetInt32(out int volume))
                {
                    SettingsInternal.Volume = Math.Clamp(volume, 0, 100);
                    return;
                }
            }

        }
        catch
        {

        }
    }

    private async Task OnVolumeInputChanged(string? inputText)
    {
        if (!int.TryParse(inputText, out int volume) || volume < 0 || volume > 100)
        {
            return;
        }

        SettingsInternal.Volume = volume; 
        await SaveVolumeToFile(volume);
    }

    private async Task SaveVolumeToFile(int volume)
    {
        try
        {
            string filePath = Path.Combine(GetPluginDirectory(), "int.json");
            string json = $"{{\"volume\":{volume}}}";
            await File.WriteAllTextAsync(filePath, json);
        }
        catch
        {

        }
    }

    private string GetPluginDirectory()
    {
        return Path.GetDirectoryName(GetType().Assembly.Location)!;
    }
}
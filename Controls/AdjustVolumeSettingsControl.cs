using Avalonia.Controls;
using Avalonia.Data;
using ClassIsland.Core.Abstractions.Controls;
using VoiceAction.Settings;

namespace VoiceAction.Controls;

public class AdjustVolumeSettingsControl : ActionSettingsControlBase<AdjustVolumeSettings> 
{
    public AdjustVolumeSettingsControl()
    {
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

        panel.Children.Add(box);
        Content = panel;
    }
}
using Avalonia.Controls;
using Avalonia.Data;
using ClassIsland.Core.Abstractions.Controls;
using VoiceAction.SET.Actions;

namespace VoiceAction.SET.Actions;

public class SetVolumeSettingsControl : ActionSettingsControlBase<SetVolumeSettings>
{
    private NumericUpDown _volumeInput;

    public SetVolumeSettingsControl()
    {
        var panel = new StackPanel { Spacing = 10, Margin = new(10) };

        panel.Children.Add(new TextBlock
        {
            Text = "设置音量百分比",
            Margin = new(0, 0, 0, 5)
        });

        _volumeInput = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 100,
            Increment = 1,
            FormatString = "0",
            Watermark = "输入 0-100 的整数"
        };
        panel.Children.Add(_volumeInput);

        panel.Children.Add(new TextBlock
        {
            Text = "0 = 静音, 100 = 最大音量",
            Foreground = Avalonia.Media.Brushes.Gray,
            FontSize = 12,
            Margin = new(0, 5, 0, 0)
        });

        Content = panel;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _volumeInput[!NumericUpDown.ValueProperty] = new Binding(nameof(Settings.VolumePercent))
        {
            Source = Settings,
            Mode = BindingMode.TwoWay
        };
    }
}
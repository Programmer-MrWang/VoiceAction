using System.Text.Json.Serialization;

namespace VoiceAction.Settings;

public class AdjustVolumeSettings
{
    [JsonPropertyName("volume")]
    public int Volume { get; set; } = 50; // 默认50
}
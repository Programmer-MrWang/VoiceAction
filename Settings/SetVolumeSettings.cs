using System.Text.Json.Serialization;

namespace VoiceAction.SET.Actions;

public class SetVolumeSettings
{
    [JsonPropertyName("volumePercent")]
    public float VolumePercent { get; set; } = 50f;
}
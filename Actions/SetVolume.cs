using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VoiceAction.SET.Actions;

[ActionInfo("VoiceAction.AdjustVolume", "设置系统音量", "\uF013", false)]
public class SetVolumeAction : ActionBase<SetVolumeSettings>
{
    private readonly ILogger<SetVolumeAction> _logger;

    public SetVolumeAction(ILogger<SetVolumeAction> logger)
    {
        _logger = logger;
    }

    protected override async Task OnInvoke()
    {
        IAudioEndpointVolume audioEndpointVolume = null;
        IMMDevice device = null;
        IMMDeviceEnumerator deviceEnumerator = null;

        try
        {
            var type = Type.GetTypeFromCLSID(new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"));
            deviceEnumerator = (IMMDeviceEnumerator)Activator.CreateInstance(type);

            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device);

            var iid = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
            device.Activate(iid, 0, IntPtr.Zero, out var obj);
            audioEndpointVolume = (IAudioEndpointVolume)obj;

            var setMute = (ISetMute)audioEndpointVolume;
            setMute.SetMute(false, Guid.Empty);
            _logger.LogInformation("已取消静音");

            float volume = Settings.VolumePercent / 100f;
            var setVolume = (ISetMasterVolumeLevelScalar)audioEndpointVolume;
            setVolume.SetMasterVolumeLevelScalar(volume, Guid.Empty);

            _logger.LogInformation($"音量设置为 {Settings.VolumePercent}%");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置音量失败");
            throw;
        }
        finally
        {
            if (audioEndpointVolume != null) Marshal.ReleaseComObject(audioEndpointVolume);
            if (device != null) Marshal.ReleaseComObject(device);
            if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
        }
    }
}

[ComImport]
[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
internal class MMDeviceEnumeratorCom { }

[ComImport]
[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceEnumerator
{
    [PreserveSig]
    int EnumAudioEndpoints(EDataFlow dataFlow, DeviceState stateMask, out IntPtr devices);

    [PreserveSig]
    int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice endpoint);
}

[ComImport]
[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDevice
{
    [PreserveSig]
    int Activate([MarshalAs(UnmanagedType.LPStruct)] Guid iid,
        int dwClsCtx, IntPtr pActivationParams,
        [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
}

/// <summary>
/// IAudioEndpointVolume 基础接口（空）
/// </summary>
[ComImport]
[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioEndpointVolume { }

/// <summary>
/// 用于设置静音的接口（跳过11个方法）
/// </summary>
[ComImport]
[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISetMute : IAudioEndpointVolume
{
    void _VtblGap1_11();

    [PreserveSig]
    int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute,
        [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);
}

/// <summary>
/// 用于设置主音量标量的接口（跳过4个方法）
/// </summary>
[ComImport]
[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISetMasterVolumeLevelScalar : IAudioEndpointVolume
{
    void _VtblGap1_4();

    [PreserveSig]
    int SetMasterVolumeLevelScalar(float fLevel,
        [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);
}

internal enum EDataFlow
{
    eRender,
    eCapture,
    eAll
}

internal enum ERole
{
    eConsole,
    eMultimedia,
    eCommunications
}

[Flags]
internal enum DeviceState
{
    ACTIVE = 0x00000001,
    DISABLED = 0x00000002,
    NOT_PRESENT = 0x00000004,
    UNPLUGGED = 0x00000008,
    MASK_ALL = 0x0000000F
}
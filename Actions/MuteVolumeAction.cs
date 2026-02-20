using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VoiceAction.M.Actions;

[ActionInfo("VoiceAction.Mute", "系统静音", "\uF015", false)]
public class MuteVolumeAction : ActionBase
{
    private readonly ILogger<MuteVolumeAction> _logger;

    public MuteVolumeAction(ILogger<MuteVolumeAction> logger)
    {
        _logger = logger;
    }

    protected override async Task OnInvoke()
    {
        try
        {
            var audioEndpointVolume = GetAudioEndpointVolume();

            if (audioEndpointVolume != null)
            {
                var setMute = (ISetMute)audioEndpointVolume;
                setMute.SetMute(true, Guid.Empty);

                _logger.LogInformation("系统已静音");

                Marshal.ReleaseComObject(audioEndpointVolume);
            }
            else
            {
                _logger.LogError("无法获取音频端点音量控制接口");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "静音失败");
            throw;
        }
    }

    private IAudioEndpointVolume GetAudioEndpointVolume()
    {
        IMMDeviceEnumerator deviceEnumerator = null;
        IMMDevice device = null;

        try
        {
            var type = Type.GetTypeFromCLSID(new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"));
            deviceEnumerator = (IMMDeviceEnumerator)Activator.CreateInstance(type);

            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device);

            var iid = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
            device.Activate(iid, 0, IntPtr.Zero, out var obj);

            return (IAudioEndpointVolume)obj;
        }
        finally
        {
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
/// 用于获取静音状态的接口（跳过12个方法）
/// </summary>
[ComImport]
[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGetMute : IAudioEndpointVolume
{
    void _VtblGap1_12(); // 跳过前面12个方法

    [PreserveSig]
    int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
}

/// <summary>
/// 用于设置主音量标量的接口（跳过4个方法）
/// </summary>
[ComImport]
[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISetMasterVolumeLevelScalar : IAudioEndpointVolume
{
    void _VtblGap1_4(); // 跳过: RegisterControlChangeNotify, Unregister..., GetChannelCount, SetMasterVolumeLevel

    [PreserveSig]
    int SetMasterVolumeLevelScalar(float fLevel,
        [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);
}

/// <summary>
/// 用于获取主音量标量的接口（跳过6个方法）
/// </summary>
[ComImport]
[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGetMasterVolumeLevelScalar : IAudioEndpointVolume
{
    void _VtblGap1_6();

    [PreserveSig]
    int GetMasterVolumeLevelScalar(out float pfLevel);
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
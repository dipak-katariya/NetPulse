using System.Runtime.InteropServices;

namespace NetPulse.Native;

internal static partial class WindowComposition
{
    private enum WindowCompositionAttribute
    {
        AccentPolicy = 19,
    }

    private enum AccentState
    {
        Disabled = 0,
        Gradient = 1,
        TransparentGradient = 2,
        BlurBehind = 3,
        AcrylicBlurBehind = 4,
        InvalidState = 5,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public AccentState AccentState;
        public uint AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    [LibraryImport("user32.dll")]
    private static partial int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    public static bool TryApplyAcrylic(IntPtr hwnd, uint argb)
    {
        // Win 10 1803 (build 17134) introduced AcrylicBlurBehind.
        if (Environment.OSVersion.Version.Build < 17134) return false;

        AccentPolicy policy = new()
        {
            AccentState = AccentState.AcrylicBlurBehind,
            AccentFlags = 2,
            GradientColor = argb,
            AnimationId = 0,
        };

        int size = Marshal.SizeOf<AccentPolicy>();
        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(policy, ptr, fDeleteOld: false);
            WindowCompositionAttributeData data = new()
            {
                Attribute = WindowCompositionAttribute.AccentPolicy,
                Data = ptr,
                SizeOfData = size,
            };
            return SetWindowCompositionAttribute(hwnd, ref data) == 0;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}

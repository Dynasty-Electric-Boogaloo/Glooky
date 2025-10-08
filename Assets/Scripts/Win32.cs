using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

public static class Win32
{
    public const ushort HID_USAGE_PAGE_GENERIC = 0x01;
    public const ushort HID_USAGE_GENERIC_MOUSE = 0x02;
    public const uint WM_INPUT = 0x00FF;
    public const uint WM_INPUT_DEVICE_CHANGE = 0x00FE;
    public const uint RID_INPUT = 0x10000003;
    public static readonly IntPtr HWND_MESSAGE = (IntPtr)(-3);
    public const int RIM_TYPEMOUSE = 0;
    public const uint RIDEV_NOLEGACY = 0x30;
    public const uint RIDEV_INPUTSINK = 0x100;
    public const uint RIDEV_DEVNOTIFY = 0x2000;
    public const uint MOUSE_MOVE_RELATIVE = 0x00;
    public const uint MOUSE_MOVE_ABSOLUTE = 0x01;
    public const ushort RI_MOUSE_LEFT_BUTTON_DOWN = 0x01;
    public const ushort RI_MOUSE_LEFT_BUTTON_UP = 0x02;

    public struct WNDCLASSEXW
    {
        public int cbSize;
        public int style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    public struct RAWINPUTDEVICE
    {
        public ushort usUsagePage;
        public ushort usUsage;
        public uint dwFlags;
        public IntPtr hwndTarget;
    }

    public struct RAWINPUTHEADER
    {
        public uint dwType;
        public int dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }
    
    public struct RAWMOUSE {
        public ushort usFlags;
        [StructLayout(LayoutKind.Explicit)]
        public struct Buttons {
            [FieldOffset(0)]
            public uint ulButtons;
            [FieldOffset(0)]
            public ushort usButtonFlags;
            [FieldOffset(2)]
            public ushort usButtonData;
        }

        public Buttons buttons;
        public uint ulRawButtons;
        public int lLastX;
        public int lLastY;
        public uint ulExtraInformation;
    };
    
    public struct RAWINPUT {
        public RAWINPUTHEADER header;
        public RAWMOUSE mouse;
    };
    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetModuleHandleW([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);

    [DllImport("user32.dll", SetLastError = true)] [return : MarshalAs(UnmanagedType.U2)]
    public static extern ushort RegisterClassExW([In] ref WNDCLASSEXW lpwcx);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterClassW(IntPtr windowClass, IntPtr hInstance);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CreateWindowExW(uint dwExStyle, IntPtr windowClass, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr pvParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern bool RegisterRawInputDevices([In][MarshalAs(UnmanagedType.LPArray)] RAWINPUTDEVICE[] pRawInputDevices, int uiNumDevices, int cbSize);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern int GetRegisteredRawInputDevices([In][Out][MarshalAs(UnmanagedType.LPArray)] RAWINPUTDEVICE[] pRawInputDevices, ref int puiNumDevices, int cbSize);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern int GetRawInputData(IntPtr hRawInput, uint uiCommand, [MarshalAs(UnmanagedType.LPArray)] byte[] pData, ref int pcbSize, int cbSizeHeader);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern int GetRawInputBuffer([MarshalAs(UnmanagedType.LPArray)] byte[] pData, ref int pcbSize, int cbSizeHeader);

    public delegate IntPtr WndProcDelegate(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
    
    public static bool IsWow64()
    {
        if (IntPtr.Size == 8)
            return false;

        if (!IsWow64Process(GetCurrentProcess(), out bool isWow64))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to figure out whether we're running under WOW64.");

        return isWow64;
    }
}
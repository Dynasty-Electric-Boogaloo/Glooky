using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class RawInput : MonoBehaviour
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [SerializeField] private int miceCount = 2;
    [SerializeField] private bool redirectingInputToUnity = false;
    public int MiceCount => miceCount;

    private static RawInput _instance;
    public static RawInput Instance => _instance;
    
    private static readonly Win32.WndProcDelegate s_WndProc = WndProc;
    private static readonly int kRawInputHeaderSize = Marshal.SizeOf<Win32.RAWINPUTHEADER>();
    private static readonly int kRawInputDeviceSize = Marshal.SizeOf<Win32.RAWINPUTDEVICE>();
    private static readonly bool kIsWow64 = Win32.IsWow64();

    private static readonly IntPtr HInstance = Win32.GetModuleHandleW(null);
    private static readonly byte[] RawInputBuffer = new byte[8192];

    private static uint _rawInputEventCount;
    private static byte[] _rawInputEvents = new byte[8192];
    private static int[] _rawInputHeaderIndices = new int[100];
    private static int[] _rawInputDataIndices = new int[100];

    private IntPtr _windowClass = IntPtr.Zero;
    private IntPtr _hwnd = IntPtr.Zero;

    private readonly Win32.RAWINPUTDEVICE[] _devices;
    private Win32.RAWINPUTDEVICE[] _queryDevices;

    public UnityEvent<Win32.RAWINPUT> onMouseMotionEvent = new();
    public UnityEvent<IntPtr> onMouseConnectedEvent = new();
    public UnityEvent<IntPtr> onMouseDisconnectedEvent = new();
    
    public RawInput()
    {
        _devices = new Win32.RAWINPUTDEVICE[miceCount];
        _queryDevices = new Win32.RAWINPUTDEVICE[miceCount];
    }

    private void RegisterRawInputDevices()
    {
        Debug.Log("Registering raw input devices");

        for (var i = 0; i < miceCount; i++)
        {
            ref Win32.RAWINPUTDEVICE mouse = ref _devices[i];
            mouse.usUsagePage = Win32.HID_USAGE_PAGE_GENERIC;
            mouse.usUsage = Win32.HID_USAGE_GENERIC_MOUSE;
            mouse.dwFlags = Win32.RIDEV_DEVNOTIFY;
            mouse.hwndTarget = _hwnd;
        }

        if (!Win32.RegisterRawInputDevices(_devices, miceCount, kRawInputDeviceSize))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register mouse and keyboard for Raw Input.");
    }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
        }

        _instance = this;
        DontDestroyOnLoad(this);
        
        _windowClass = RegisterWindowClass();
        _hwnd = Win32.CreateWindowExW(0, _windowClass, "Mice Raw Input Window", 0, 0, 0, 0, 0, Win32.HWND_MESSAGE, IntPtr.Zero, HInstance, IntPtr.Zero);
        if (_hwnd == null)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create raw input redirection window.");

        RegisterRawInputDevices();
    }
    
    private void Update()
    {
        int numberOfDevices = 0;
        Win32.GetRegisteredRawInputDevices(null, ref numberOfDevices, kRawInputDeviceSize);

        if (_queryDevices.Length < numberOfDevices)
            Array.Resize(ref _queryDevices, numberOfDevices);

        if (Win32.GetRegisteredRawInputDevices(_queryDevices, ref numberOfDevices, kRawInputDeviceSize) == -1)
        {
            var error = Marshal.GetLastWin32Error();
            Debug.LogError("GetRegisteredRawInputDevices failed: " + new Win32Exception(error).Message);
        }
        else
        {
            for (int i = 0; i < numberOfDevices; i++)
            {
                if (_queryDevices[i].usUsage == Win32.HID_USAGE_GENERIC_MOUSE)
                {
                    if (_queryDevices[i].hwndTarget != _hwnd)
                    {
                        RegisterRawInputDevices();
                        break;
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        if (!Win32.DestroyWindow(_hwnd))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to destroy raw input redirection window class.");

        if (!Win32.UnregisterClassW(_windowClass, HInstance))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to unregister raw input redirection window class.");
    }
    
    [MonoPInvokeCallback(typeof(Win32.WndProcDelegate))]
    private static IntPtr WndProc(IntPtr window, uint message, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            switch (message)
            {
                case Win32.WM_INPUT:
                    ProcessRawInputMessage(lParam);

                    for (var i = 0U; i < _rawInputEventCount; i++)
                    {
                        Win32.RAWINPUT rawInput;
                        unsafe
                        {
                            fixed (byte* rawInputData = _rawInputEvents)
                            {
                                rawInput = ((Win32.RAWINPUT*)rawInputData)[i];
                            }
                        }
                        
                        Instance.onMouseMotionEvent?.Invoke(rawInput);
                    }

                    break;
                case Win32.WM_INPUT_DEVICE_CHANGE:
                    switch ((int)wParam)
                    {
                        case 1:
                            Instance.onMouseConnectedEvent?.Invoke(lParam);
                            break;
                        case 2:
                            Instance.onMouseDisconnectedEvent?.Invoke(lParam);
                            break;
                    }
                    break;
            }

            return Win32.DefWindowProcW(window, message, wParam, lParam);
        }
        catch (Exception e)
        {
            // Never let exception escape to native code as that will crash the app
            Debug.LogException(e);
            return IntPtr.Zero;
        }
    }

    private static void ProcessRawInputMessage(IntPtr lParam)
    {
        int rawInputEventCount = 0;
        int rawInputEventsSize = 0;

        // First, process the message we received
        int sizeofRawInputBuffer = RawInputBuffer.Length;
        int result = Win32.GetRawInputData(lParam, Win32.RID_INPUT, RawInputBuffer, ref sizeofRawInputBuffer, kRawInputHeaderSize);
        if (result == -1)
        {
            var errorCode = Marshal.GetLastWin32Error();
            Debug.LogError($"Failed to get raw input data: {new Win32Exception(errorCode).Message}");
            return;
        }

        CopyEventFromRawInputBuffer(0, kRawInputHeaderSize, ref rawInputEventCount, ref rawInputEventsSize);

        // Next, drain the raw input message queue so they don't keep getting
        // pumped one at a time through PeekMessage/DispatchMessage as that is
        // too slow with high input polling rate devices
        while (true)
        {
            var rawInputCount = Win32.GetRawInputBuffer(RawInputBuffer, ref sizeofRawInputBuffer, kRawInputHeaderSize);
            if (rawInputCount == 0)
                break;

            if (rawInputCount == -1)
            {
                var errorCode = Marshal.GetLastWin32Error();
                Debug.LogError($"Failed to get raw input buffer: {new Win32Exception(errorCode).Message}");
                break;
            }

            int offset = 0;
            for (int i = 0; i < rawInputCount; i++)
            {
                var rawInputDataOffset = kRawInputHeaderSize;

                if (kIsWow64)
                {
                    // RAWINPUTHEADER is 16 bytes on 32-bit, but 24 bytes on 64-bit. Since this data is memcpy-ed from the kernel,
                    // we need to adjust the data pointer by 8 bytes if we're in a 32-bit process but 64-bit kernel.
                    // We only need to do this with data coming from GetRawInputBuffer as GetRawInputData fixes up the single
                    // event it returns.
                    rawInputDataOffset += rawInputDataOffset;
                }

                offset += CopyEventFromRawInputBuffer(offset, rawInputDataOffset, ref rawInputEventCount, ref rawInputEventsSize);
            }
        }

        // Finally, dispatch the events to Unity
        if (_instance.redirectingInputToUnity)
        {
            unsafe
            {
                fixed(int* rawInputHeaderIndices = _rawInputHeaderIndices)
                {
                    fixed(int* rawInputDataIndices = _rawInputDataIndices)
                    {
                        fixed(byte* rawInputData = _rawInputEvents)
                        {
                            UnityEngine.Windows.Input.ForwardRawInput((uint*)rawInputHeaderIndices, (uint*)rawInputDataIndices, (uint)rawInputEventCount, rawInputData, (uint)rawInputEventsSize);
                        }
                    }
                }
            }
        }

        _rawInputEventCount = (uint)rawInputEventCount;
    }
    
    private static int CopyEventFromRawInputBuffer(int offset, int dataOffset, ref int rawInputEventCount, ref int rawInputEventsSize)
    {
        Win32.RAWINPUTHEADER header;
        unsafe
        {
            fixed(byte* rawInputBufferPtr = RawInputBuffer)
                header = *(Win32.RAWINPUTHEADER*)(rawInputBufferPtr + offset);
        }

        if (rawInputEventsSize + header.dwSize > _rawInputEvents.Length)
            Array.Resize(ref _rawInputEvents, Math.Max(rawInputEventsSize + header.dwSize, 2 * _rawInputEvents.Length));

        if (rawInputEventCount == _rawInputHeaderIndices.Length)
        {
            Array.Resize(ref _rawInputHeaderIndices, 2 * _rawInputHeaderIndices.Length);
            Array.Resize(ref _rawInputDataIndices, 2 * _rawInputHeaderIndices.Length);
        }

        _rawInputHeaderIndices[rawInputEventCount] = rawInputEventsSize;
        _rawInputDataIndices[rawInputEventCount] = rawInputEventsSize + dataOffset;
        Array.Copy(RawInputBuffer, offset, _rawInputEvents, rawInputEventsSize, header.dwSize);

        rawInputEventCount++;
        rawInputEventsSize += header.dwSize;
        return header.dwSize;
    }

    private static IntPtr RegisterWindowClass()
    {
        var wndClass = new Win32.WNDCLASSEXW
        {
            cbSize = Marshal.SizeOf<Win32.WNDCLASSEXW>(),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(s_WndProc),
            hInstance = HInstance,
            lpszClassName = "Mice Raw Input"
        };

        var registeredClass = Win32.RegisterClassExW(ref wndClass);
        if (registeredClass == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register the window class.");

        return new IntPtr(registeredClass);
    }
#endif
}
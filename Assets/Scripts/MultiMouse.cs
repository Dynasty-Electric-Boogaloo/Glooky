using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//TODO Change "invalid" mouse handle value from IntPtr.Zero to -1
//Use (IntPtr)(-1) to represent the value

public class MultiMouse : MonoBehaviour
{
    private static MultiMouse _instance;
    public static MultiMouse Instance => _instance;
    
    public struct MouseData
    {
        public struct Button
        {
            public bool Pressed;
            public bool Held;
            public bool Released;
        }
        public IntPtr MouseHandle;
        public Vector2Int Position;
        public Vector2Int Delta;
        public Button Left;
    }
    
    private readonly object _lock = new();
    private MouseData[] _miceData;
    private Stack<int> _spawnedMice = new();

    public UnityEvent<int> onMouseSpawn = new();

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(_instance);
    }

    private void Start()
    {
        lock (_lock)
        {
            _miceData = new MouseData[RawInput.Instance.MiceCount];
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private void OnEnable()
    {
        RawInput.Instance.onMouseMotionEvent.AddListener(OnMouseMotion);
        RawInput.Instance.onMouseConnectedEvent.AddListener(OnMouseConnected);
        RawInput.Instance.onMouseDisconnectedEvent.AddListener(OnMouseDisconnected);
    }
    
    private void OnDisable()
    {
        RawInput.Instance.onMouseMotionEvent.RemoveListener(OnMouseMotion);
        RawInput.Instance.onMouseConnectedEvent.RemoveListener(OnMouseConnected);
        RawInput.Instance.onMouseDisconnectedEvent.RemoveListener(OnMouseDisconnected);
    }

    private void Update()
    {
        while (_spawnedMice.Count > 0)
        {
            onMouseSpawn?.Invoke(_spawnedMice.Pop());
        }
    }

    public void ClearMouseData(int index)
    {
        if (index < 0 || index >= _miceData.Length)
        {
            Debug.LogError("Mouse Data index out of bounds!");
            return;
        }
        
        lock (_lock)
        {
            _miceData[index].Delta = Vector2Int.zero;
            _miceData[index].Left.Pressed = false;
            _miceData[index].Left.Released = false;
        }
    }

    public MouseData GetMouseData(int index)
    {
        if (_miceData == null)
            return default;
        
        if (index < 0 || index >= _miceData.Length)
        {
            Debug.LogError("Mouse Data index out of bounds!");
            return default;
        }
        
        lock (_lock)
        {
            var mouseData = _miceData[index];
            return mouseData;
        }
    }
    
    private void OnMouseMotion(Win32.RAWINPUT input)
    {
        if (input.header.dwType != Win32.RIM_TYPEMOUSE)
            return;

        lock (_lock)
        {
            var index = Array.FindIndex(_miceData, x => x.MouseHandle == input.header.hDevice);

            if (index < 0 && (input.mouse.buttons.usButtonFlags & Win32.RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
            {
                index = Array.FindIndex(_miceData, x => x.MouseHandle == IntPtr.Zero);
                
                if (index >= 0)
                {
                    _spawnedMice.Push(index);
                }
            }

            if (index < 0)
                return;

            ref var mouse = ref _miceData[index];
            mouse.MouseHandle = input.header.hDevice;

            if ((input.mouse.buttons.usButtonFlags & Win32.RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
            {
                mouse.Left.Pressed = true;
                mouse.Left.Held = true;
            }
            
            if ((input.mouse.buttons.usButtonFlags & Win32.RI_MOUSE_LEFT_BUTTON_UP) != 0)
            {
                mouse.Left.Released = true;
                mouse.Left.Held = false;
            }

            if ((input.mouse.usFlags & Win32.MOUSE_MOVE_ABSOLUTE) == Win32.MOUSE_MOVE_RELATIVE)
            {
                mouse.Delta.x += input.mouse.lLastX;
                mouse.Delta.y += input.mouse.lLastY;
                mouse.Position.x += input.mouse.lLastX;
                mouse.Position.y += input.mouse.lLastY;
            }
            else if ((input.mouse.usFlags & Win32.MOUSE_MOVE_ABSOLUTE) == Win32.MOUSE_MOVE_ABSOLUTE)
            {
                mouse.Delta.x += input.mouse.lLastX - mouse.Position.x;
                mouse.Delta.y += input.mouse.lLastY - mouse.Position.y;
                mouse.Position.x = input.mouse.lLastX;
                mouse.Position.y = input.mouse.lLastY;
            }
        }
    }

    private void OnMouseConnected(IntPtr handle)
    {
        
    }
    
    private void OnMouseDisconnected(IntPtr handle)
    {
        lock (_lock)
        {
            var index = Array.FindIndex(_miceData, x => x.MouseHandle == handle);

            if (index < 0)
                return;

            ref var mouse = ref _miceData[index];
            mouse.MouseHandle = IntPtr.Zero;
        }
    }
}
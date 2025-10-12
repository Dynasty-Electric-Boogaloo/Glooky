using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// Multiple Mouse Management Singleton
// TODO Add MouseData strobe buffers in order to lock in copies of the current input data for the whole frame.
public class MultiMouse : MonoBehaviour
{
    private static MultiMouse _instance;
    public static MultiMouse Instance => _instance;
    
    /// Structure containing the input data for a single mouse.
    public struct MouseData
    {
        /// Button clicking data.
        public struct Button
        {
            public bool Pressed;
            public bool Held;
            public bool Released;
        }
        
        /// The unique device identifier associated with this virtual mouse.
        /// Value is -1 when this virtual mouse is disconnected from any physical device.
        public IntPtr MouseHandle;
        /// Absolute position tracked for the virtual mouse, not accurate to the system mouse but can be useful for supporting UI stuff.
        public Vector2Int Position;
        /// The position delta accumulated since the last clear.
        public Vector2Int Delta;
        /// State of the left mouse button.
        public Button Left;
    }
    
    /// Event called when a virtual mouse gets paired to a physical device.
    /// The passed int is the virtual mouse index (aka player index).
    public UnityEvent<int> onMouseSpawn = new();
    
    /// Event called when a virtual mouse gets unpaird from a physical device.
    /// The passed int is the virtual mouse index (aka player index).
    public UnityEvent<int> onMouseDespawn = new();
    
    /// Object used for multi thread safety, since the mouse event callbacks happen asynchronously to Unity's threads.
    private readonly object _lock = new();
    
    /// Data for each player's mouse.
    private MouseData[] _miceData;
    
    /// Stack that accumulates mouse spawn events until the Unity threads can process them.
    private Stack<int> _spawnedMice = new();

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

            for (var i = 0; i < RawInput.Instance.MiceCount; i++)
            {
                _miceData[i].MouseHandle = (IntPtr)(-1);
            }
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

    /// Clear the accumulated virtual mouse input data of a player.
    /// <param name="index">The mouse (aka player) index.</param>
    /// @note Only call this at the end of the frame when you know the inputs will not be polled again until next frame,
    /// this is to keep consistency between GetMouseData calls.
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

    /// Get the accumulated virtual mouse input data of a player.
    /// <param name="index">The mouse (aka player) index.</param>
    /// <returns>The current accumulated data of the virtual mouse.</returns>
    /// @note Further mouse events will not update the returned MouseData,
    /// call the function again if you need more recent data.
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
    
    /// Physical mouse event callback handler.
    /// <param name="input">Raw Win32 input data.</param>
    private void OnMouseMotion(Win32.RAWINPUT input)
    {
        if (input.header.dwType != Win32.RIM_TYPEMOUSE)
            return;

        lock (_lock)
        {
            var index = Array.FindIndex(_miceData, x => x.MouseHandle == input.header.hDevice);

            if (index < 0 && (input.mouse.buttons.usButtonFlags & Win32.RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
            {
                index = Array.FindIndex(_miceData, x => x.MouseHandle == (IntPtr)(-1));
                
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

    /// Physical mouse connexion event handler.
    /// <param name="handle">Physical device identifier.</param>
    private void OnMouseConnected(IntPtr handle)
    {
        
    }
    
    /// Physical mouse disconnexion event handler.
    /// <param name="handle">Physical device identifier.</param>
    private void OnMouseDisconnected(IntPtr handle)
    {
        lock (_lock)
        {
            var index = Array.FindIndex(_miceData, x => x.MouseHandle == handle);

            if (index < 0)
                return;
            
            onMouseDespawn?.Invoke(index);

            ref var mouse = ref _miceData[index];
            mouse.MouseHandle = (IntPtr)(-1);
        }
    }
}
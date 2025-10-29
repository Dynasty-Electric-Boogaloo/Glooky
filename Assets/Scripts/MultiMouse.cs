using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

/// Multiple Mouse Management Singleton
// TODO Add MouseData strobe buffers in order to lock in copies of the current input data for the whole frame.
public class MultiMouse : MonoBehaviour
{
    private static MultiMouse _instance;
    public static MultiMouse Instance => _instance;
    
    /// Structure containing the input data for a single mouse.
    public struct MouseData
    {
        /// Input device type information
        public enum InputType
        {
            None,
            Mouse,
            Controller
        }
        
        /// Button clicking data.
        public struct Button
        {
            public bool Pressed;
            public bool Held;
            public bool Released;
        }
        
        /// The device type of the virtual mouse
        public InputType Type;
        
        /// The unique device identifier associated with this virtual mouse.
        /// Value is -1 when this virtual mouse is disconnected from any physical device.
        public IntPtr MouseHandle;

        /// Input user associated with the gamepad input device.
        public InputUser DeviceUser;
        
        /// Input map associated with the gamepad input device.
        public JoystickInputs JoystickInput;
        
        /// Absolute position tracked for the virtual mouse, not accurate to the system mouse but can be useful for supporting UI stuff.
        public Vector2Int Position;
        /// The position delta accumulated since the last clear.
        public Vector2Int Delta;
        /// State of the left mouse button.
        public Button Left;
    }

    /// The delta multiplier to match the joystick movement to mouse movement.
    [Tooltip("The delta multiplier to match the joystick movement to mouse movement.")]
    [SerializeField] private float cursorDeltaSpeed;
    
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
                _miceData[i].Type = MouseData.InputType.None;
            }
        }
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;
        
        _instance = null;
    }

    private void OnEnable()
    {
        RawInput.Instance.onMouseMotionEvent.AddListener(OnMouseMotion);
        RawInput.Instance.onMouseConnectedEvent.AddListener(OnMouseConnected);
        RawInput.Instance.onMouseDisconnectedEvent.AddListener(OnMouseDisconnected);
        InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
        InputUser.onChange += OnDeviceChanged;
        InputUser.listenForUnpairedDeviceActivity = RawInput.Instance.MiceCount;
    }
    
    private void OnDisable()
    {
        RawInput.Instance.onMouseMotionEvent.RemoveListener(OnMouseMotion);
        RawInput.Instance.onMouseConnectedEvent.RemoveListener(OnMouseConnected);
        RawInput.Instance.onMouseDisconnectedEvent.RemoveListener(OnMouseDisconnected);
        InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
        InputUser.onChange -= OnDeviceChanged;
        InputUser.listenForUnpairedDeviceActivity = 0;
    }

    private void Update()
    {
        while (_spawnedMice.Count > 0)
        {
            onMouseSpawn?.Invoke(_spawnedMice.Pop());
        }

        lock (_lock)
        {
            for (var i = 0; i < RawInput.Instance.MiceCount; i++)
            {
                if (_miceData[i].Type != MouseData.InputType.Controller)
                    continue;

                if (!_miceData[i].DeviceUser.valid)
                    continue;

                var delta = _miceData[i].JoystickInput.Cursor.MoveDelta.ReadValue<Vector2>() * cursorDeltaSpeed;
                var intDelta = new Vector2Int(Mathf.RoundToInt(delta.x), -Mathf.RoundToInt(delta.y));
                _miceData[i].Delta += intDelta;
                _miceData[i].Position += intDelta;
                _miceData[i].Left.Held = _miceData[i].JoystickInput.Cursor.Click.IsPressed();
                _miceData[i].Left.Pressed |= _miceData[i].JoystickInput.Cursor.Click.WasPressedThisFrame();
                _miceData[i].Left.Released |= _miceData[i].JoystickInput.Cursor.Click.WasReleasedThisFrame();
            }
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
                index = Array.FindIndex(_miceData, x => !IsDevicePaired(x));
                
                if (index >= 0)
                {
                    _spawnedMice.Push(index);
                }
            }

            if (index < 0)
                return;

            ref var mouse = ref _miceData[index];
            mouse.MouseHandle = input.header.hDevice;
            mouse.Type = MouseData.InputType.Mouse;

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
            mouse.Type = MouseData.InputType.None;
        }
    }

    /// Callback to handle unpaired gamepads and connect them to available virtual mice. 
    /// <param name="control">Input that triggered the callback.</param>
    /// <param name="eventPtr">Event data pointer.</param>
    private void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
    {
        if (control is not ButtonControl) 
            return;
        
        var index = Array.FindIndex(_miceData, x => !IsDevicePaired(x));
        if (index < 0)
            return;

        var user = InputUser.PerformPairingWithDevice(control.device);
        var userInput = new JoystickInputs();

        if (!userInput.asset.IsUsableWithDevice(control.device))
        {
            user.UnpairDevicesAndRemoveUser();
            return;
        }

        user.AssociateActionsWithUser(userInput);

        _miceData[index].DeviceUser = user;
        _miceData[index].JoystickInput = userInput;
        _miceData[index].Type = MouseData.InputType.Controller;

        userInput.Enable();
        _spawnedMice.Push(index);
    }
    
    /// Callback to handle gamepad disconnection.
    /// <param name="user">Associated input user.</param>
    /// <param name="change">Event type.</param>
    /// <param name="device">Associated input device.</param>
    private void OnDeviceChanged(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change != InputUserChange.DeviceLost)
            return;

        lock (_lock)
        {
            var index = Array.FindIndex(_miceData, x => x.DeviceUser == user);
            if (index < 0)
                return;

            if (!user.valid || !_miceData[index].DeviceUser.valid)
                return;
            
            user.UnpairDevicesAndRemoveUser();
            _miceData[index].DeviceUser = default;
            _miceData[index].JoystickInput.Disable();
            _miceData[index].JoystickInput.Dispose();
            _miceData[index].JoystickInput = null;
            _miceData[index].Type = MouseData.InputType.None;
            onMouseDespawn?.Invoke(index);
        }
    }

    /// Get whether or not the passed mouse is paired to any device.
    /// <param name="mouseData">Virtual mouse data to be checked.</param>
    /// <returns>true if passed mouse is paired to any device, false otherwise.</returns>
    public bool IsDevicePaired(MouseData mouseData)
    {
        lock (_lock)
        {
            switch (mouseData.Type)
            {
                case MouseData.InputType.None:
                    return false;
                case MouseData.InputType.Mouse:
                    return mouseData.MouseHandle != (IntPtr)(-1);
                case MouseData.InputType.Controller:
                    return mouseData.DeviceUser.valid;
            }
        }

        return false;
    }
}
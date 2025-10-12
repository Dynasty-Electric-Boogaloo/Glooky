using System;
using UnityEngine;
using UnityEngine.Events;

/// Basic implementation of CursorController
// TODO Rework movement to be more physics based.
public class CursorController : MonoBehaviour
{
    [SerializeField] private int mouseIndex;
    [SerializeField] private Host startHost;
    
    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private Vector2 movementRange;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float slowDownSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float groundCheckLength;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float hoverHeight;

    [Header("Interaction")]
    [SerializeField] private float interactionRange;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private LayerMask interactionMask;
    [SerializeField] private int maxInteractionFound;
    
    [Header("Feedbacks")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private Transform chainEndTransform;
    [SerializeField] private Colorizer colorizer;
    [SerializeField] private ParticleSystem clickParticles;
    [SerializeField] private AudioSource clickSound;

    /// Event called each time the player clicks the mouse, in any context.
    public UnityEvent onClick;
    
    /// Event called when the controller is paired to an input device, or when the latter gets disconnected.
    public UnityEvent<int> onPlayerChange;

    private Material _baseMaterial;
    private Transform _camera;
    private Rigidbody _rigidbody;
    private Host _host;
    private Vector3 _inputDelta;
    private float _verticalVelocity;
    
    private Collider[] _interactionColliders;
    
    private void Start()
    {
        _camera = Camera.main.transform;
        _rigidbody = GetComponent<Rigidbody>();
        _interactionColliders = new Collider[maxInteractionFound];
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (startHost != null)
            AssignHost(startHost);
        
        onPlayerChange?.Invoke(-1);
    }

    private void OnEnable()
    {
        MultiMouse.Instance.onMouseSpawn.AddListener(OnMouseSpawn);
        MultiMouse.Instance.onMouseDespawn.AddListener(OnMouseDespawn);
    }

    private void OnDisable()
    {
        MultiMouse.Instance?.onMouseSpawn.RemoveListener(OnMouseSpawn);
        MultiMouse.Instance?.onMouseDespawn.RemoveListener(OnMouseDespawn);
    }

    private void Update()
    {
        MultiMouse.MouseData mouseData = MultiMouse.Instance.GetMouseData(mouseIndex);

        if (mouseData.Left.Pressed)
            Interact();

        modelTransform.localPosition = Vector3.down * (mouseData.Left.Held ? .25f : 0);
        
        Vector2 input = mouseData.Delta;

        var camForward = _camera.forward;
        camForward.y = 0;
        camForward.Normalize();

        var camRight = _camera.right;
        camRight.y = 0;
        camRight.Normalize();

        _inputDelta += new Vector3(camForward.x * -input.y + camRight.x * input.x, 0, camForward.z * -input.y + camRight.z * input.x);

        if (_inputDelta.magnitude > movementRange.y)
            _inputDelta = _inputDelta.normalized * movementRange.y;

        MultiMouse.Instance.ClearMouseData(mouseIndex);
    }

    private void FixedUpdate()
    {
        GroundCheck();
        if (_inputDelta.magnitude > movementRange.x)
            modelTransform.forward = Vector3.Slerp(modelTransform.forward, new Vector3(_inputDelta.normalized.x, 0, _inputDelta.normalized.z), turnSpeed * Time.fixedDeltaTime);
        else
            _inputDelta = Vector3.zero;

        var speed = ComputeSpeed();
        _rigidbody.linearVelocity = new Vector3(_inputDelta.x * speed, _verticalVelocity, _inputDelta.z * speed);
        _inputDelta = Vector3.Lerp(_inputDelta, Vector3.zero, slowDownSpeed * Time.fixedDeltaTime);

        if (_host)
            transform.position = _host.RestrainCursorPosition();
    }

    /// Sets the current attached Host of the CursorController, calls provided Host's Capture function.
    /// <param name="host">Host to be captured by the mouse.</param>
    public void AssignHost(Host host)
    {
        if (_host)
            _host.Release();
        
        host.Capture(this);
        _host = host;
    }
    
    /// Eject the CursorController from its Host, by Host demand.
    /// @note Do not call this from somewhere other than Host code that properly handles the dissociation.
    public void EjectFromHost()
    {
        _host = null;
    }
    
    /// Get assigned player index, may be -1 if no player is assigned to the CursorController.
    /// <returns>Player index value.</returns>
    public int GetPlayerIndex()
    {
        if (mouseIndex >= 0)
        {
            if (MultiMouse.Instance.GetMouseData(mouseIndex).MouseHandle == (IntPtr)(-1))
                return -1;
        }
        
        return mouseIndex;
    }

    /// Get the chain attachment point of the CursorController.
    /// <returns>Chain attachment point Transform.</returns>
    public Transform GetChainEndTransform()
    {
        return chainEndTransform;
    }

    /// Get the currently attached Host
    /// <returns>The current host. The value can be null, make sure to check for it if you use this function.</returns>
    public Host GetHost()
    {
        return _host;
    }

    /// Handle interactions with the environment, notably IClickable objects.
    private void Interact()
    {
        onClick?.Invoke();
        
        var count = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRange, _interactionColliders, interactionMask);

        if (count <= 0)
            return;
        
        var closestDistance = interactionRange;
        IClickable clickable = null;
        for (var i = 0; i < count; i++)
        {
            if (_interactionColliders[i].gameObject == gameObject)
                continue;
            
            if (_host != null && _interactionColliders[i].gameObject == _host.gameObject)
                continue;
            
            var distance = Vector3.Distance(interactionPoint.position,
                _interactionColliders[i].ClosestPoint(interactionPoint.position));

            if (distance < closestDistance && _interactionColliders[i].TryGetComponent(out IClickable newClosestClickable))
            {
                closestDistance = distance;
                clickable = newClosestClickable;
            }
        }

        clickable?.Click(this);
    }

    /// Handle ground alignment and gravity
    private void GroundCheck()
    {
        var ray = new Ray(_rigidbody.position, Vector3.down);

        if (!Physics.Raycast(ray, out var hit, groundCheckLength, groundMask))
        {
            _verticalVelocity = Mathf.Max(_verticalVelocity + gravity * Time.fixedDeltaTime, maxFallSpeed);
            return;
        }
        
        _verticalVelocity = 0;
        _rigidbody.position = new Vector3(_rigidbody.position.x, hit.point.y + hoverHeight, _rigidbody.position.z);
    }

    /// Get the CursorController speed based on context like if the controller has an attached Host and/or is within range of it.
    private float ComputeSpeed()
    {
        if (!_host)
            return movementSpeed / 2;

        if (_host.IsCursorInRange())
            return movementSpeed * 2;

        return movementSpeed;
    }

    /// Handle player connexion event.
    /// <param name="playerIndex">The connected player index.</param>
    private void OnMouseSpawn(int playerIndex)
    {
        if (playerIndex == mouseIndex)
            onPlayerChange?.Invoke(playerIndex);
    }

    /// Handle player disconnexion event.
    /// <param name="playerIndex">The disconnected player index.</param>
    private void OnMouseDespawn(int playerIndex)
    {
        if (playerIndex == mouseIndex)
            onPlayerChange?.Invoke(-1);
    }
}

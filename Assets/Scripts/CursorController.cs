using System;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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

    public UnityEvent onClick;
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

    public void AssignHost(Host host)
    {
        if (_host)
            _host.Release();
        
        host.Capture(this);
        _host = host;
    }
    
    public void EjectFromHost()
    {
        _host = null;
    }

    public int GetPlayerIndex()
    {
        if (mouseIndex >= 0)
        {
            if (MultiMouse.Instance.GetMouseData(mouseIndex).MouseHandle == (IntPtr)(-1))
                return -1;
        }
        
        return mouseIndex;
    }

    public Transform GetChainEndTransform()
    {
        return chainEndTransform;
    }

    public Host GetHost()
    {
        return _host;
    }

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

    private float ComputeSpeed()
    {
        if (!_host)
            return movementSpeed / 2;

        if (_host.IsCursorInRange())
            return movementSpeed * 2;

        return movementSpeed;
    }

    private void OnMouseSpawn(int playerIndex)
    {
        if (playerIndex == mouseIndex)
            onPlayerChange?.Invoke(playerIndex);
    }

    private void OnMouseDespawn(int playerIndex)
    {
        if (playerIndex == mouseIndex)
            onPlayerChange?.Invoke(-1);
    }
}

using System;
using UnityEngine;
using UnityEngine.Events;

/// Basic implementation of Host
public class Host : MonoBehaviour, IClickable
{
    private static readonly int Speed = Animator.StringToHash("Speed");

    [Header("Movement")]
    [SerializeField] private float followSpeed;
    [SerializeField] private float followMaxSpeed;
    [SerializeField] private float turnSpeed;
    
    [Header("Chain")]
    [SerializeField] private float chainLength;
    [SerializeField] private float restrainDistance;
    [SerializeField] private float interactionBreakDistance;
    [SerializeField] private float chainSlackHeight;
    [SerializeField] private int chainLinkCount;
    [SerializeField] private Transform chainHolderTransform;
    [SerializeField] private Transform chainLinkPrefab;
    
    [Header("Feedbacks")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private Animator animator;

    public UnityEvent<int> onPlayerChange;

    private Transform[] _chainLinks;

    private CursorController _cursor;
    private PhysicsController _physicsController;
    private Rigidbody _rigidbody;
    private float _verticalVelocity;
    private float _timer;
    private Interactable _targetInteractable;
    
    private void Awake()
    {
        _chainLinks = new Transform[chainLinkCount];
        
        for (var i = 0; i < _chainLinks.Length; i++)
        {
            _chainLinks[i] = Instantiate(chainLinkPrefab, transform);
            _chainLinks[i].gameObject.SetActive(false);
            onPlayerChange.AddListener(_chainLinks[i].GetComponentInChildren<Colorizer>().SetPlayerIndex);
        }
        
        onPlayerChange?.Invoke(-1);
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _physicsController = GetComponent<PhysicsController>();
    }

    private void Update()
    {
        UpdateAnimator();

        if (_cursor != null)
            UpdateChainPosition();
    }

    private void FixedUpdate()
    {
        UpdatePullForce();

        if (_rigidbody.linearVelocity.magnitude > 0.1f)
            UpdateDirection(_rigidbody.linearVelocity);
    }
    
    /// Handle being captured by a CursorController.
    /// <param name="cursor">CursorController that is connecting to this Host</param>
    public void Capture(CursorController cursor)
    {
        if (!cursor)
            return;
        
        if (_cursor)
        {
            _cursor.onPlayerChange.RemoveListener(OnPlayerChange);
            _cursor.EjectFromHost();
        }
        
        _cursor = cursor;
        _cursor.onPlayerChange.AddListener(OnPlayerChange);
        
        OnPlayerChange(_cursor.GetPlayerIndex());

        foreach (var chainLink in _chainLinks)
        {
            chainLink.gameObject.SetActive(true);
            chainLink.localPosition = Vector3.zero;
        }
    }

    /// Release Host from CursorController.
    public void Release()
    {
        _cursor.onPlayerChange.RemoveListener(OnPlayerChange);
        _cursor.EjectFromHost();
        _cursor = null;
        
        OnPlayerChange(-1);
        
        foreach (var chainLink in _chainLinks)
        {
            chainLink.gameObject.SetActive(false);
        }
    }

    /// Begin interacting state with provided Interactable.
    /// <param name="interactable">The interactable to interact with.</param>
    public void BeginInteraction(Interactable interactable)
    {
        _targetInteractable = interactable;
    }

    /// Stop interacting and go back to following the cursor controller.
    public void EndInteraction()
    {
        _targetInteractable.Interact(false);
        _targetInteractable = null;
    }
    
    /// Handle being clicked by a CursorController, effectively beginning the capturing process.
    /// <param name="controller">CursorController that is capturing Host.</param>
    /// <returns>Whether or not the interaction succeeded.</returns>
    public bool Click(CursorController controller)
    {
        controller.AssignHost(this);
        return true;
    }

    /// Get the Clickable interaction type.
    /// <returns>Clickable interaction type.</returns>
    public ClickableType GetClickableType()
    {
        return ClickableType.Host;
    }

    /// Check whether the connected CursorController is within the chain length of the Host.
    /// <returns>Whether or not the CursorController is within the chain length of the Host.</returns>
    public bool IsCursorInRange()
    {
        if (!_cursor)
            return false;
        
        var diff = _cursor.GetChainEndTransform().position - chainHolderTransform.position;
        return diff.magnitude < chainLength;
    }
    
    /// Restrain the position of the connected CursorController to be within the chain length of the host.
    /// <returns>Constrained CursorController position.</returns>
    public Vector3 RestrainCursorPosition()
    {
        var diff = _cursor.transform.position - transform.position;
        if (diff.magnitude < restrainDistance)
            return _cursor.transform.position;

        var position = transform.position + diff.normalized * restrainDistance;
        position.y = _cursor.transform.position.y;
        return position;
    }
    
    /// Update the connected CursorController chain pulling force.
    private void UpdatePullForce()
    {
        var direction = Vector3.zero;
        var diff = Vector3.zero;
        
        if (_cursor)
            diff = _cursor.GetChainEndTransform().position - _rigidbody.position;
        
        if (_targetInteractable)
        {
            var target = _targetInteractable.GetTargetPoint();
            var inDiff = target - _rigidbody.position;
            inDiff.y = 0;

            if (inDiff.magnitude > 0.2f)
            {
                direction = inDiff * followSpeed;
                direction = Vector3.ClampMagnitude(direction, followMaxSpeed);
            }
            else
            {
                _targetInteractable.Interact(true);
            }

            if (diff.magnitude > interactionBreakDistance)
                EndInteraction();
        }
        else if (diff.magnitude > chainLength)
        {
            direction = (diff - diff.normalized * chainLength) * followSpeed;
            var magnitude = direction.magnitude;
            direction.y = 0;
            direction = direction.normalized * magnitude;
            direction = Vector3.ClampMagnitude(direction, followMaxSpeed);
        }

        _physicsController.SetMovementDirection(direction);
    }

    /// Update the Host model direction based on the difference between the Host and CursorController positions.
    /// <param name="direction">Difference between the Host and CursorController positions</param>
    private void UpdateDirection(Vector3 direction)
    {
        direction.y = 0;
        direction.Normalize();
            
        modelTransform.forward = Vector3.Slerp(modelTransform.forward, direction, turnSpeed * Time.fixedDeltaTime);
    }

    /// Update Animator animation speed and transform for some juicy feedback on the speed you're pulling the Host.
    private void UpdateAnimator()
    {
        var speed = _rigidbody.linearVelocity.magnitude;
        animator.SetFloat(Speed, speed);
        _timer += speed * Time.deltaTime;
        _timer %= 4;

        animator.transform.localEulerAngles = new Vector3(speed, 0, Mathf.Sin(_timer / 2 * Mathf.PI) * speed / 2);
    }

    /// Update the chain links between the Host and CursorController, with some slack included, for that extra chain-ish feel.
    private void UpdateChainPosition()
    {
        var diff = _cursor.GetChainEndTransform().position - chainHolderTransform.position;
        var chainSlack = chainSlackHeight * Mathf.Clamp01(1 - diff.magnitude / chainLength);

        for (var i = 0; i < _chainLinks.Length; i++)
        {
            var factor = (i + .5f) / _chainLinks.Length;
            var position = chainHolderTransform.position + diff * factor + Vector3.down * (chainSlack * Mathf.Sin(factor * Mathf.PI));
            _chainLinks[i].position = position;
        }
    }

    /// Handle player connexion or disconnection events.
    /// <param name="playerIndex">The connected player index.</param>
    private void OnPlayerChange(int playerIndex)
    {
        onPlayerChange?.Invoke(playerIndex);
    }
}
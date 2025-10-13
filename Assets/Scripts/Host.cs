using UnityEngine;
using UnityEngine.Events;

/// Basic implementation of Host
// TODO Rework movement to be more physics based.
public class Host : MonoBehaviour, IClickable
{
    private static readonly int Speed = Animator.StringToHash("Speed");

    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float slowDownSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float groundCheckLength;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float hoverHeight;
    
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
    private Rigidbody _rigidbody;
    private float _verticalVelocity;
    private Vector3 _pullForce;
    private float _timer;
    private Interactable _targetInteractable;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _chainLinks = new Transform[chainLinkCount];
        
        for (var i = 0; i < _chainLinks.Length; i++)
        {
            _chainLinks[i] = Instantiate(chainLinkPrefab, transform);
            _chainLinks[i].gameObject.SetActive(false);
            onPlayerChange.AddListener(_chainLinks[i].GetComponentInChildren<Colorizer>().SetPlayerIndex);
        }
        
        onPlayerChange?.Invoke(-1);
    }

    private void Update()
    {
        UpdateAnimator();

        if (_cursor != null)
            UpdateChainPosition();
    }

    private void FixedUpdate()
    {
        GroundCheck();

        var diff = _cursor ? _cursor.GetChainEndTransform().position - chainHolderTransform.position : Vector3.zero;

        UpdatePullForce(diff);

        if (diff.magnitude > 0.1f)
            UpdateDirection(diff);
        
        _rigidbody.linearVelocity = new Vector3(_pullForce.x, _verticalVelocity, _pullForce.z);
    }
    
    /// Handle being captured by a CursorController.
    /// <param name="cursor">CursorController that is connecting to this Host</param>
    // TODO Handle cursor == null case.
    public void Capture(CursorController cursor)
    {
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
        _targetInteractable = null;
    }
    
    /// Handle being clicked by a CursorController, effectively beginning the capturing process.
    /// <param name="controller">CursorController that is capturing Host.</param>
    /// <returns>Whether or not the interaction succeded.</returns>
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
    // TODO Handle _cursor == null case.
    public bool IsCursorInRange()
    {
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
    
    /// Update the connected CursorController chain pulling force, based on the difference between the Host and
    /// CursorController positions.
    /// <param name="diff">Difference between the Host and CursorController positions</param>
    private void UpdatePullForce(Vector3 diff)
    {
        if (_targetInteractable)
        {
            var target = _targetInteractable.GetTargetPoint();
            var inDiff = target - _rigidbody.position;
            inDiff.y = 0;

            if (inDiff.magnitude > 0.2f)
                _pullForce = inDiff * movementSpeed;
            else
                Slowdown();

            if (diff.magnitude > interactionBreakDistance)
                EndInteraction();

            return;
        }
        
        if (diff.magnitude > chainLength)
        {
            _pullForce = (diff - diff.normalized * chainLength) * movementSpeed;
            var magnitude = _pullForce.magnitude;
            _pullForce.y = 0;
            _pullForce = _pullForce.normalized * magnitude;
            return;
        }
        
        Slowdown();
    }

    /// Update the Host model direction based on the difference between the Host and CursorController positions.
    /// <param name="direction">Difference between the Host and CursorController positions</param>
    private void UpdateDirection(Vector3 direction)
    {
        direction.y = 0;
        direction.Normalize();
            
        modelTransform.forward = Vector3.Slerp(modelTransform.forward, direction, turnSpeed * Time.fixedDeltaTime);
    }

    /// Slow down the pulling force towards a magnitude of 0.
    private void Slowdown()
    {
        _pullForce = Vector3.Lerp(_pullForce, Vector3.zero, slowDownSpeed * Time.fixedDeltaTime);
    }

    /// Update Animator animation speed and transform for some juicy feedback on the speed you're pulling the Host.
    private void UpdateAnimator()
    {
        animator.SetFloat(Speed, _pullForce.magnitude);
        _timer += _pullForce.magnitude * Time.deltaTime;
        _timer %= 4;

        animator.transform.localEulerAngles = new Vector3(_pullForce.magnitude, 0, Mathf.Sin(_timer / 2 * Mathf.PI) * _pullForce.magnitude / 2);
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

    /// Handle player connexion or disconnexion events.
    /// <param name="playerIndex">The connected player index.</param>
    private void OnPlayerChange(int playerIndex)
    {
        onPlayerChange?.Invoke(playerIndex);
    }
}
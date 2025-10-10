using UnityEngine;
using UnityEngine.Events;

/// Basic implementation of Host
/// TODO Document further
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
    
    public void Capture(CursorController cursor)
    {
        if (!_cursor)
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
    
    public bool Click(CursorController controller)
    {
        controller.AssignHost(this);
        return true;
    }

    public ClickableType GetClickableType()
    {
        return ClickableType.Host;
    }

    public bool IsCursorInRange()
    {
        var diff = _cursor.GetChainEndTransform().position - chainHolderTransform.position;
        return diff.magnitude < chainLength;
    }
    
    public Vector3 RestrainCursorPosition()
    {
        var diff = _cursor.transform.position - transform.position;
        if (diff.magnitude < restrainDistance)
            return _cursor.transform.position;

        var position = transform.position + diff.normalized * restrainDistance;
        position.y = _cursor.transform.position.y;
        return position;
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
    
    private void UpdatePullForce(Vector3 diff)
    {
        if (diff.magnitude > chainLength)
        {
            _pullForce = (diff - diff.normalized * chainLength) * movementSpeed;
            var magnitude = _pullForce.magnitude;
            _pullForce.y = 0;
            _pullForce = _pullForce.normalized * magnitude;
        }
        else
        {
            Slowdown();
        }
    }

    private void UpdateDirection(Vector3 direction)
    {
        direction.y = 0;
        direction.Normalize();
            
        modelTransform.forward = Vector3.Slerp(modelTransform.forward, direction, turnSpeed * Time.fixedDeltaTime);
    }

    private void Slowdown()
    {
        _pullForce = Vector3.Lerp(_pullForce, Vector3.zero, slowDownSpeed * Time.fixedDeltaTime);
    }

    private void UpdateAnimator()
    {
        animator.SetFloat(Speed, _pullForce.magnitude);
        _timer += _pullForce.magnitude * Time.deltaTime;
        _timer %= 4;

        animator.transform.localEulerAngles = new Vector3(_pullForce.magnitude, 0, Mathf.Sin(_timer / 2 * Mathf.PI) * _pullForce.magnitude / 2);
    }

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

    private void OnPlayerChange(int playerIndex)
    {
        onPlayerChange?.Invoke(playerIndex);
    }
}
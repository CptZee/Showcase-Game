using UniRx;
using UnityEngine;

public class TouchingDirections : MonoBehaviour
{
    [SerializeField]
    private ContactFilter2D castFilter;
    [SerializeField]
    private float groundDistance = 0.02f;
    [SerializeField]
    private float wallDistance = 0.02f;
    [SerializeField]
    private float ceilingDistance = 0.05f;
    [SerializeField]
    private bool _isGrounded = true;
    [SerializeField]
    private bool _isOnWall;
    [SerializeField]
    private bool _isOnCeiling;
    protected Collider2D touchingCol;
    protected Animator animator;
    protected CompositeDisposable disposables;

    RaycastHit2D[] groundHits = new RaycastHit2D[5];
    RaycastHit2D[] wallHits = new RaycastHit2D[5];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];
    private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

    public bool IsGrounded
    {
        get { return _isGrounded; }
        private set
        {
            _isGrounded = value;
            animator.SetBool(StaticStrings.isGrounded, value);
        }
    }

    public bool IsOnWall
    {
        get { return _isOnWall; }
        private set
        {
            _isOnWall = value;
            animator.SetBool(StaticStrings.isOnWall, value);
        }
    }
    public bool IsOnCeiling
    {
        get { return _isOnCeiling; }
        private set
        {
            _isOnCeiling = value;
            animator.SetBool(StaticStrings.isOnCeiling, value);
        }
    }

    void Awake()
    {
        touchingCol = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        disposables = new CompositeDisposable();
    }

    void Start()
    {
        /**
         * This needs to be executed every fixed update to check if the player is grounded, on wall, or on ceiling.
         * We can't use Interval() here since we need this to happen in real time.
         */
        Observable.EveryFixedUpdate()
            .Subscribe(_ =>
            {
                IsGrounded = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
                IsOnWall = touchingCol.Cast(wallCheckDirection, castFilter, wallHits, wallDistance) > 0;
                IsOnCeiling = touchingCol.Cast(Vector2.up, castFilter, ceilingHits, ceilingDistance) > 0;
            }).AddTo(disposables);
    }

    void OnDestroy()
    {
        disposables.Dispose();
    }
}

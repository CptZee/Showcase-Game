using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;

[RequireComponent(typeof(TouchingDirections), typeof(Damageable))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed = 5f;
    [SerializeField]
    private float jumpImpulse = 6f;
    [SerializeField]
    private float airSpeed = 3f;
    [SerializeField]
    [Tooltip("DEBUG ONLY - DO NOT EDIT")]
    private bool _isFacingRight = true;
    [SerializeField]
    [Tooltip("DEBUG ONLY - DO NOT EDIT")]
    private bool _isMoving = true;
    [SerializeField]
    [Tooltip("DEBUG ONLY - DO NOT EDIT")]
    private List<Collider2D> interactables = new List<Collider2D>();
    protected Vector2 moveInput;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected CompositeDisposable disposables;
    protected TouchingDirections touchingDirections;
    protected Damageable damageable;
    protected ParticleSystem dust;

    public ReactiveProperty<int> coins = new ReactiveProperty<int>();
    /**
    * The calculations of this property is not really here, I just proxied it in this class
    * to adhere to the instructions of the exam.
    *
    * I would normally place it in the Damageable class and subscribe to it in the UIController class.
    * This is so that I can reuse the Damageable class in other objects that can take damage. E.g enemies, destructible objects, etc.
    */
    public ReactiveProperty<float> hp = new ReactiveProperty<float>();

    public int Coins
    {
        get { return coins.Value; }
        set
        {
            coins.Value = value;
            PlayerPrefs.SetInt("Coins", value);
        }
    }

    public bool LockVelocity
    {
        get
        {
            return animator.GetBool(StaticStrings.lockVelocity);
        }
        set
        {
            animator.SetBool(StaticStrings.lockVelocity, value);
        }
    }

    public bool CanMove
    {
        get { return animator.GetBool(StaticStrings.canMove); }
        set
        {
            animator.SetBool(StaticStrings.canMove, value);
        }
    }

    public bool IsMoving
    {
        get { return _isMoving; }
        private set
        {
            _isMoving = value;
            animator.SetBool(StaticStrings.isMoving, value);
        }
    }
    public bool IsFacingRight
    {
        get { return _isFacingRight; }
        private set
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
                _isFacingRight = value;
            }
        }
    }

    public float CurrentMoveSpeed
    {
        get
        {
            if (!CanMove)
                return 0;
            if (!IsMoving)
                return 0;
            if (!damageable.IsAlive)
                return 0;
            if (!touchingDirections.IsGrounded)
            {
                if (!touchingDirections.IsOnWall)
                    return airSpeed;
                else
                {
                    return 0;
                }
            }
            if (!touchingDirections.IsOnWall)
                return walkSpeed;
            else
                return 0;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        dust = GetComponentInChildren<ParticleSystem>();
        damageable = GetComponent<Damageable>();
        disposables = new CompositeDisposable();
        coins.Value = PlayerPrefs.GetInt("Coins", 0);
    }

    /**
     * Start
     *
     * Instead of directly checking if the player character have 0 HP, I instead checked if they can move.
     * This is so that I can handle other cases where the player shouldn't be able to move but aren't dead.
     */
    void Start()
    {
        // Again, proxy the hp value to the Damageable class
        damageable.hp.Subscribe(hp =>
        {
            this.hp.Value = hp;
        }).AddTo(disposables);

        Observable.Interval(System.TimeSpan.FromSeconds(0.25f))
            .Where(_ => CanMove)
            .Where(_ => !LockVelocity)
            .Subscribe(_ =>
            {
                rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
                animator.SetFloat(StaticStrings.yVelocity, rb.velocity.y);
            }).AddTo(disposables);

        Observable.Interval(System.TimeSpan.FromSeconds(0.25f))
            .Where(_ => !IsMoving)
            .Subscribe(_ =>
            {
                StopDust();
            }).AddTo(disposables);
    }

    void OnDestroy()
    {
        disposables.Dispose();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (moveInput.x < 0.5f && moveInput.x > -0.5f)
        {
            IsMoving = false;
            return; //Ignore y inputs (We are only listening for x inputs)
        }
        if (!CanMove)
            return;

        if (touchingDirections.IsGrounded)
            CreateDust();
        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (!damageable.IsAlive)
            return;
        if (moveInput.x > 0 && !IsFacingRight)
        {
            // Face the right
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;

        if (!touchingDirections.IsGrounded)
            return;

        if (!CanMove)
            return;
        animator.SetTrigger(StaticStrings.jumpTrigger);
        rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!CanMove)
            return;

        if (interactables.Count == 0)
            return;

        interactables.ForEach(interactable =>
        {
            IInteractable obj = interactable.GetComponent<IInteractable>();
            if (obj == null)
                return;

            obj.OnInteract();
        });
    }

    public void GiveCoins(int amount)
    {
        Coins += amount;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!interactables.Contains(collision))
            interactables.Add(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (interactables.Contains(collision))
            interactables.Remove(collision);
    }

    public void OnHit(float damage, Vector2 knockback)
    {
        LockVelocity = true;
        int direction = IsFacingRight ? -1 : 1;
        rb.velocity = new Vector2(knockback.x * direction, rb.velocity.y + knockback.y);
        LockVelocity = false;
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;
        if (!touchingDirections.IsGrounded)
            return;

        animator.SetTrigger(StaticStrings.attackTrigger);
    }

    void CreateDust()
    {
        dust.Play();
    }

    void StopDust()
    {
        dust.Stop();
    }
}

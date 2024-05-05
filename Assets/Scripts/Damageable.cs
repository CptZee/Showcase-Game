using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using UniRx;

public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField]
    private UnityEvent<float, Vector2> damageableHit;
    [Tooltip("The current health of the object.")]
    [SerializeField]
    private float _currentHealth = 20;
    protected Animator animator;
    [SerializeField]
    private float _maxHealth = 20;
    [SerializeField]
    private bool _isAlive = true;
    [SerializeField]
    private bool _isInvincible = false;
    [SerializeField]
    private float invincibilityDuration = 0.25f;
    protected float timeSinceHit;
    protected float timer;
    protected CinemachineBasicMultiChannelPerlin _cbmcp;
    public ReactiveProperty<float> hp = new ReactiveProperty<float>();

    public float MaxHealth
    {
        get { return _maxHealth; }
        set { _maxHealth = value; }
    }

    public float CurrentHealth
    {
        get { return hp.Value; }
        set
        {
            hp.Value = value;
            if (hp.Value <= 0)
            {
                IsAlive = false;
            }
        }
    }
    public bool IsAlive
    {
        get { return _isAlive; }
        private set
        {
            _isAlive = value;
            animator.SetBool(StaticStrings.isAlive, value);
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        hp.Value = _currentHealth;

        /**
         * This cannot be migrated into the ReactiveProperty since this needs to listen for the timer to reach 0
         * and not for every changes in the value of the timer.
         */

        Observable.EveryFixedUpdate()
            .Where(_ => _isInvincible)
            .Subscribe(_ =>
            {
                if (timeSinceHit > invincibilityDuration)
                {
                    _isInvincible = false;
                    timeSinceHit = 0;
                }

                timeSinceHit += Time.deltaTime;
            });

    }

    public bool TakeDamage(float damage, Vector2 knockback)
    {
        if (IsAlive && !_isInvincible)
        {
            CurrentHealth -= damage;
            _isInvincible = true;

            animator.SetTrigger(StaticStrings.hitTrigger);
            damageableHit?.Invoke(damage, knockback);

            return true;
        }
        return false;
    }

    public bool Heal(float healAmount)
    {
        if (IsAlive)
        {
            CurrentHealth += healAmount;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
            return true;
        }
        return false;
    }
}

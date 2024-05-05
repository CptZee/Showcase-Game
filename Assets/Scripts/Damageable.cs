using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using UniRx;
using System.Collections;

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
    protected SpriteRenderer spriteRenderer;
    protected float blinkDuration = 0.2f;
    protected Color hurtBlinkColor = Color.red;
    protected Color healBlinkColor = Color.green;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
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

            StartCoroutine(HurtBlinkSprite());
            return true;
        }
        return false;
    }

    private IEnumerator HurtBlinkSprite()
    {
        Color originalColor = spriteRenderer.color;

        spriteRenderer.color = hurtBlinkColor;

        yield return new WaitForSeconds(blinkDuration);

        spriteRenderer.color = originalColor;
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
            StartCoroutine(HealBlinkSprite());
            return true;
        }
        return false;
    }

    private IEnumerator HealBlinkSprite()
    {
        Color originalColor = spriteRenderer.color;

        spriteRenderer.color = healBlinkColor;

        yield return new WaitForSeconds(blinkDuration);

        spriteRenderer.color = originalColor;
    }
}

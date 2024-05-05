using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using UniRx;

public class Damageable : MonoBehaviour, IDamageable
{
    private UnityEvent<float, Vector2> damageableHit;
    [SerializeField]
    [Tooltip("The virtual camera for the screen effects.")]
    private CinemachineVirtualCamera virtualCamera;
    [SerializeField]
    [Tooltip("The intensity of the screen shake.")]
    private float shakeIntensity = 1f;
    [Tooltip("The duration of the screen shake.")]
    [SerializeField]
    private float shakeTime = 0.5f;
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
    private ReactiveProperty<bool> isInvincible = new ReactiveProperty<bool>();

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
        StopShake();
        hp.Value = _currentHealth;
        isInvincible.Value = _isInvincible;

        isInvincible.Subscribe(isInvincible =>
        {
            _isInvincible = isInvincible;
            
            if (timeSinceHit > invincibilityDuration)
            {
                isInvincible = false;
                timeSinceHit = 0;
            }

            timeSinceHit += Time.deltaTime;
        });

        /**
         * This cannot be migrated into the ReactiveProperty since this needs to listen for the timer to reach 0
         * and not for every changes in the value of the timer.
         */

        Observable.EveryFixedUpdate()
            .Where(_ => timer > 0)
            .Subscribe(_ =>
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    StopShake();
                }
            });

    }

    public bool TakeDamage(float damage, Vector2 knockback)
    {
        if (IsAlive && !isInvincible.Value)
        {
            CurrentHealth -= damage;
            isInvincible.Value = true;

            animator.SetTrigger(StaticStrings.hitTrigger);
            damageableHit?.Invoke(damage, knockback);

            ShakeCamera();

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

    public void ShakeCamera()
    {
        CinemachineBasicMultiChannelPerlin _cbmcp = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = shakeIntensity;

        timer = shakeTime;
    }

    void StopShake()
    {
        CinemachineBasicMultiChannelPerlin _cbmcp = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = 0f;

        timer = 0f;
    }
}

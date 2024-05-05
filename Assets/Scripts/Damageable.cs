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
    protected Animator animator;
    [SerializeField]
    private float _maxHealth = 20;
    [SerializeField]
    private float _currentHealth = 20;
    [SerializeField]
    private bool _isAlive = true;
    [SerializeField]
    private bool isInvincible = false;
    [SerializeField]
    private float invincibilityDuration = 0.25f;
    protected float timeSinceHit;

    protected float timer;
    protected CinemachineBasicMultiChannelPerlin _cbmcp;

    public float MaxHealth
    {
        get { return _maxHealth; }
        set { _maxHealth = value; }
    }

    public float CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = value;
            if (_currentHealth <= 0)
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
        Observable.EveryUpdate()
            .Where(_ => isInvincible)
            .Where(_ => timeSinceHit > invincibilityDuration)
            .Subscribe(_ =>
            {
                isInvincible = false;
                timeSinceHit = 0;

                timeSinceHit += Time.deltaTime;
            });
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
        if (IsAlive && !isInvincible)
        {
            CurrentHealth -= damage;
            isInvincible = true;

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

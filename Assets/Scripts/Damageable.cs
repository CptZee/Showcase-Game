using System;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour, IDamageable
{
    public UnityEvent<float, Vector2> damageableHit;
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

    void Update()
    {
        if (isInvincible)
        {
            if(timeSinceHit > invincibilityDuration)
            {
                isInvincible = false;
                timeSinceHit = 0;
            }

            timeSinceHit += Time.deltaTime;
        }
    }

    public bool TakeDamage(float damage, Vector2 knockback)
    {
        if (IsAlive && !isInvincible)
        {
            CurrentHealth -= damage;
            isInvincible = true;

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
            if(CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
            return true;
        }
        return false;
    }
}

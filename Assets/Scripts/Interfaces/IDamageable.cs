using UnityEngine;
public interface IDamageable
{
    float CurrentHealth { get; set; }
    float MaxHealth { get; set; }
    bool TakeDamage(float damage, Vector2 knockback);
    bool Heal(float healAmount);
}
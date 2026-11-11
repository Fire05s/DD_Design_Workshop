using Priestess;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An interface for any objects capable of taking damage to implement.
/// </summary>
public interface IDamageable
{
    public event Action OnDeath;
    public event Action<Vector2, bool> OnAttacked; // attack source (to handle knockback)
    void TakeDamageServerRpc(float damage, List<StatusEffectType> effects, Transform source, bool isStatusEffect = false, StatusEffectType type = StatusEffectType.Bleed);
    void HealServerRpc(float heal, bool scored = true);
}

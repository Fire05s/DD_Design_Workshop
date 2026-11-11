using Priestess;
using System.Collections;
using PurrNet;
using UnityEngine;

public class ReducedCooldownState : State
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _buffAnim;

    [Header("Audio")]
    [SerializeField] private AudioClip _abilityAudio;

    [Header("Component")]
    [SerializeField] private MovementController _movement;
    [SerializeField] private HealthHandler _healthHandler;

    [Header("Attack Type")]
    [SerializeField] private SyncedStats.AttackType _attackType;

    [Header("Buff Stats")]
    // [SerializeField] private float _cooldownBuffAmountModifier;
    // [SerializeField] private float _movementBuffAmountModifier;
    [SerializeField] private float _chargeTime;
    // [SerializeField] private float _buffDuration;
    // private float _cooldownBuffAmountPercentage;
    // private float _movementBuffAmountPercentage;
    // private float _primaryCooldownBuffAmount;
    // private float _dashCooldownBuffAmount;
    // private float _movementBuffAmount;

    private bool _canBuff = true;

    public override void Enter()
    {
        if (!_canBuff)
        {
            isComplete = true;
            return;
        }

        SoundFXManager.Instance?.PlaySoundFXClip(_abilityAudio, transform.position);
        _canBuff = false;
        anim.Play(_buffAnim.name);
        _movement.SetCanMove(false);
        StartCoroutine(BuffCooldown());
    }

    public override void FixedDo()
    {
        if (time >= _chargeTime && !isComplete)
        {
            // _cooldownBuffAmountPercentage = stats.AttackPower * _cooldownBuffAmountModifier / 100;
            // _movementBuffAmountPercentage = stats.AttackPower * _movementBuffAmountModifier / 100;
            // _primaryCooldownBuffAmount = stats.PrimaryAbilityCooldown.value * _cooldownBuffAmountPercentage;
            // _dashCooldownBuffAmount = stats.DashCooldown.value * _cooldownBuffAmountPercentage;
            // _movementBuffAmount = stats.MovementSpeed.value * _movementBuffAmountPercentage;
            // StartCoroutine(ApplyBuff());
            _healthHandler.ApplyStatusEffectServerRpc(StatusEffectType.LinayBuff, transform);
            isComplete = true;
        }
    }

    public override void Exit()
    {
        _movement.SetCanMove(true);
    }

    // /// <summary>
    // /// Applies the cooldown and movement speed buff value for the buff duration.
    // /// </summary>
    // private IEnumerator ApplyBuff()
    // {
    //     ApplyCooldownMovementBuff();
    //     _healthHandler.SpawnStatusEffectRpc(StatusEffectType.LinayBuff);
    //     yield return new WaitForSeconds(_buffDuration);
    //     RemoveCooldownMovementBuff();
    // }

    // /// <summary>
    // /// Applies the cooldown and movement speed buff value on the server.
    // /// </summary>
    // [ServerRpc]
    // private void ApplyCooldownMovementBuff()
    // {
    //     stats.MovementSpeed.value = stats.MovementSpeed.value + _movementBuffAmount;
    //     float targetPrimaryAbilityCooldownReduction = stats.PrimaryAbilityCooldown.value - _primaryCooldownBuffAmount;
    //     stats.PrimaryAbilityCooldown.value = Mathf.Clamp(stats.PrimaryAbilityCooldown.value - _primaryCooldownBuffAmount, 0.1f, stats.PrimaryAbilityCooldown.value);
    //     _primaryCooldownBuffAmount -= stats.PrimaryAbilityCooldown.value - targetPrimaryAbilityCooldownReduction;
    //     float targetDashAbilityCooldownReduction = stats.DashCooldown.value - _dashCooldownBuffAmount;
    //     stats.DashCooldown.value = Mathf.Clamp(stats.DashCooldown.value - _dashCooldownBuffAmount, 0.1f, stats.DashCooldown.value);
    //     _dashCooldownBuffAmount -= stats.DashCooldown.value - targetDashAbilityCooldownReduction;
    // }

    // /// <summary>
    // /// Applies the cooldown and movement speed buff value on the server.
    // /// </summary>
    // [ServerRpc]
    // private void RemoveCooldownMovementBuff()
    // {
    //     stats.MovementSpeed.value -= _movementBuffAmount;
    //     stats.PrimaryAbilityCooldown.value += _primaryCooldownBuffAmount;
    //     stats.DashCooldown.value += _dashCooldownBuffAmount;
    // }

    private IEnumerator BuffCooldown()
    {
        yield return new WaitForSeconds(stats.GetCooldown(_attackType));
        _canBuff = true;
    }
}

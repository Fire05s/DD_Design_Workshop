using Priestess;
using System.Collections;
using PurrNet;
using UnityEngine;

/// <summary>
/// Handles the logic and animations for the Shield State for the Tank Challenger.
/// </summary>
public class ShieldState : State
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _shieldUpAnim;
    [SerializeField] private AnimationClip _shieldDownAnim;

    [Header("Audio")]
    [SerializeField] private AudioClip _abilityAudio;

    [Header("Components")]
    [SerializeField] private SyncedInput _input;
    [SerializeField] private MovementController _movement;
    [SerializeField] private HealthHandler _healthHandler;

    [Header("Attack Type")]
    [SerializeField] private SyncedStats.AttackType _attackType;

    [Header("Shield Stats")]
    [SerializeField] private float _armTime = 0.5f;
    [SerializeField] private float _unarmTime = 0.5f;

    private bool _canBuff = true;
    private bool _buffApplied = false;

    public override void Enter()
    {
        if (!_canBuff)
        {
            isComplete = true;
            return;
        }

        SoundFXManager.Instance?.PlaySoundFXClip(_abilityAudio, transform.position);
        _canBuff = false;
        _buffApplied = false;
        anim.Play(_shieldUpAnim.name);
        _movement.SetCanMove(false);
        StartCoroutine(ShieldCooldown());
    }

    public override void FixedDo()
    {
        if (time >= _armTime + _unarmTime && !isComplete)
        {
            isComplete = true;
        }
        else if (time >= _armTime && !_buffApplied && !isComplete)
        {
            _buffApplied = true;
            _healthHandler.ApplyStatusEffectServerRpc(StatusEffectType.AmberBuff, transform);
        }
    }

    public override void Exit()
    {
        _movement.SetCanMove(true);
    }

    private IEnumerator ShieldCooldown()
    {
        yield return new WaitForSeconds(stats.GetCooldown(_attackType));
        _canBuff = true;
    }
}
using Priestess;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the logic and animations for the Buff State.
/// </summary>
public class AttackBuffState : State
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
    [SerializeField] private float _chargeTime;

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
            _healthHandler.ApplyStatusEffectServerRpc(StatusEffectType.YarrowBuff, transform);
            isComplete = true;
        }
    }

    public override void Exit()
    {
        _movement.SetCanMove(true);
    }

    private IEnumerator BuffCooldown()
    {
        yield return new WaitForSeconds(stats.GetCooldown(_attackType));
        _canBuff = true;
    }
}

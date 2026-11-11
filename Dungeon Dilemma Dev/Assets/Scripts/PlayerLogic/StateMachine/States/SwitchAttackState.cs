using Priestess;
using UnityEngine;
using System.Collections;

public class SwitchAttackState : State
{
    [Header("Primary States")]
    [SerializeField] private State state1;
    [SerializeField] private State state2;

    [Header("Components")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private MovementController _movement;
    [SerializeField] private HealthHandler _healthHandler;

    [Header("Attack Type")]
    [SerializeField] private SyncedStats.AttackType _attackType;

    [Header("Animation")]
    [SerializeField] private AnimationClip _switchAnim1;
    [SerializeField] private AnimationClip _switchAnim2;
    [SerializeField] private float _switchTime;

    [Header("Audio")]
    [SerializeField] private AudioClip _abilityAudio;

    private bool _isSwitched;
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
        if (_isSwitched)
        {
            anim.Play(_switchAnim1.name);
        }
        else
        {
            anim.Play(_switchAnim2.name);
        }
        _movement.SetCanMove(false);
        StartCoroutine(BuffCooldown());
        GetComponentInParent<PlayerUiManager>().SwitchIcons();

        _isSwitched = !_isSwitched;

        playerAttack.SetPrimary(_isSwitched ? state2 : state1);
    }

    public override void FixedDo()
    {
        if (time >= _switchTime && !isComplete)
        {
            _healthHandler.ApplyStatusEffectServerRpc(StatusEffectType.RuruBuff, transform);
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
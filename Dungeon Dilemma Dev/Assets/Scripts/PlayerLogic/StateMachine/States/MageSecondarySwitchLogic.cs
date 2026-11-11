using Priestess;
using System.Collections;
using PurrNet;
using UnityEngine;

public class MageSecondarySwitchLogic : State
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _switchAnim;

    [Header("Audio")]
    [SerializeField] private AudioClip _abilityAudio;

    [Header("Abilities")]
    public State _primary; // Slots will appear once these are saved
    public State _secondary;
    [SerializeField] private float _switchTime;

    [Header("Components")]
    [SerializeField] private SyncedInput _input;
    [SerializeField] private MovementController _movement;
    [SerializeField] private HealthHandler _healthHandler;

    [Header("Buff Stats")]
    [SerializeField] private float _buffAmountMultiplier;
    [SerializeField] private float _buffDuration;

    public override void Enter()
    {
        base.Enter();
        SoundFXManager.Instance?.PlaySoundFXClip(_abilityAudio, transform.position);
        anim.Play(_switchAnim.name);
        _movement.SetCanMove(false);
        StartCoroutine(SwitchAnimationLock());
        if (_input.PrimaryAbilityPressed && _primary != null)
            machine.Set(_primary, true);
        else if (_input.SecondaryAbilityPressed && _secondary != null)
            machine.Set(_secondary, true);
        StartCoroutine(ApplyBuff(_buffAmountMultiplier * stats.AttackPower.value / 100));
        isComplete = true;
    }

    public override void Do()
    {
        if (machine != null && machine.state != null && machine.state.isComplete)
        {
            isComplete = true;
        }
    }
    private IEnumerator SwitchAnimationLock()
    {
        yield return new WaitForSeconds(_switchTime);
    }

    /// <summary>
    /// Applies the attack buff value for the buff duration.
    /// </summary>
    /// <param name="amount"> New attack buff value. </param>
    private IEnumerator ApplyBuff(float amount)
    {
        ModifyAttackBuffServerRpc(amount);
        _healthHandler.SpawnStatusEffectRpc(StatusEffectType.RuruBuff);
        yield return new WaitForSeconds(_buffDuration);
        ModifyAttackBuffServerRpc(-amount);
    }

    /// <summary>
    /// Changes the attack buff value on the server.
    /// </summary>
    /// <param name="amount"> Value added to attack buff. </param>
    [ServerRpc]
    private void ModifyAttackBuffServerRpc(float amount)
    {
        stats.AttackBuff.value += amount;
    }
}
using System.Collections;
using ControllerLogic;
using PurrNet;
using UnityEngine;

public class BossDeathState : State
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _deathAnim;

    [Header("Components")]
    [SerializeField] private MovementController _movement;
    public override void Enter()
    {
        if (!isOwner) return;
        combat.ClearStatuses();
        anim.Play(_deathAnim.name);
        _movement.SetCanMove(false);
        StartCoroutine(DeathTimer());
    }

    public override void Exit()
    {
        _movement.SetCanMove(true);
    }

    public void RevivePlayer(float health)
    {
        SetHP(health);
    }

    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(1.5f);
        GlobalController.Singleton.EndRound("Challengers");
    }

    private void SetHP(float amount)
    {
        health.HealServerRpc(amount, scored: false);
    }
}

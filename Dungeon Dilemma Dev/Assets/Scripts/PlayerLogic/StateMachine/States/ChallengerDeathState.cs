using System.Collections;
using System.Collections.Generic;
using ControllerLogic;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;

public class ChallengerDeathState : State
{
    [Header("Sprite")]
    [SerializeField] private SpriteRenderer _playerSprite;
    [Header("Audio")]
    [SerializeField] private AudioClip _deathAudio;
    [Header("VFX")]
    [SerializeField] private HealthHandler _healthHandler;
    public override void Enter()
    {
        HandlePlayerDeath();
        SetHP(0); // removes any lingering regenerated hp
    }

    public void FixedUpdate()
    {
        if (core.CompareTag("Dead"))
        {
            _playerSprite.color = new Color(0.3f, 0.6f, 1f, 0.4f);
        }
    }

    public void RevivePlayer(float health)
    {
        HealHP(health);
        HandlePlayerRevive();
    }

    [ObserversRpc(runLocally: true)]
    private void HandlePlayerDeath()
    {
        core.tag = "Dead";
        core.transform.parent.tag = "Dead";
        core.gameObject.layer = LayerMask.NameToLayer("Dead");
        _playerSprite.color = new Color(0.3f, 0.6f, 1f, 0.4f);

        combat.ClearStatuses();

        // play death toll to all players locally
        List<SyncedStats> allStats = GlobalController.Singleton.GetAllStats();
        foreach (SyncedStats s in allStats) {
            SoundFXManager.Instance?.PlayLocalSound(_deathAudio, s.transform.position);
        }

        StartCoroutine(DeathTimer());
    }

    [ObserversRpc(runLocally: true)]
    private void HandlePlayerRevive()
    {
        core.tag = "Challenger";
        core.transform.parent.tag = "Challenger";
        core.gameObject.layer = LayerMask.NameToLayer("Challenger");
        _playerSprite.color = Color.white;
        _healthHandler.SpawnStatusEffectRpc(Priestess.StatusEffectType.ReviveSuccessVFX);
    }

    private void HealHP(float amount)
    {
        health.HealServerRpc(amount, scored: false);
    }

    [ServerRpc]
    private void SetHP(float newAmount)
    {
        stats.CurrentHealth.value = newAmount;
    }

    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(1.5f);
        GlobalController.Singleton.CheckChallengerWipe();
    }
}

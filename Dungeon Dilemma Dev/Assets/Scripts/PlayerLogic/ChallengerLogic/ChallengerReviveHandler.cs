using ControllerLogic;
using Sync_Modules;
using UnityEngine;

public class ChallengerReviveHandler : MonoBehaviour
{
    [Header("Revive Stats")]
    [SerializeField] private float _reviveTime = 3f;

    [Header("SFX")]
    [SerializeField] private AudioClip _reviveProgressSFX;
    [SerializeField] private AudioClip _reviveCompleteSFX;

    [Header("VFX")]
    [SerializeField] private HealthHandler _healthHandler;

    public int ReviveAmount { get; private set; }

    private float _reviveTimer;
    private Challenger _currentTarget;
    private GameObject _cachedObject;
    private SyncedStats _stats;
    private Challenger _ownerChallenger;
    private float _nextParticleTime;

    private void Start()
    {
        _stats = GetComponentInParent<SyncedStats>();
        _ownerChallenger = GetComponentInParent<Challenger>();
    }

    private void Update()
    {
        if (_stats == null)
        {
            _stats = GetComponentInParent<SyncedStats>();
        }

        if (GlobalController.Singleton.WorldData.RoundState == RoundState.Setup)
        {
            ReviveAmount = _stats.Revives.value;
        }
        HandleRevive();
    }

    private void HandleRevive()
    {
        if (_currentTarget == null || ReviveAmount <= 0 || _ownerChallenger.CompareTag("Dead")) return;

        _reviveTimer += Time.deltaTime;

        if (_reviveTimer > _reviveTime)
        {
            _currentTarget.Revive(0.5f);
            ReviveAmount--;
            _currentTarget = null;
            _cachedObject = null;
            _reviveTimer = 0;
            SoundFXManager.Instance.StopLoop(transform);
            SoundFXManager.Instance.PlaySoundFXClip(_reviveCompleteSFX, transform.position);

            // give 3 Support Points for successful revive
            GlobalController.Singleton.UpdatePlayerScoreData(_stats.owner.Value, ScoreType.SupportPoints, 3);
        }

        if (_reviveTimer > _nextParticleTime)
        {
            _nextParticleTime++;
            _healthHandler.SpawnStatusEffectRpc(Priestess.StatusEffectType.ReviveVFX);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_currentTarget) return;
        if (collision.gameObject.CompareTag("Dead") && ReviveAmount > 0)
        {
            if (collision.gameObject.TryGetComponent(out Challenger challenger))
            {
                _currentTarget = challenger;
                _cachedObject = collision.gameObject;
                _reviveTimer = 0;
                SoundFXManager.Instance.PlayLoop(_reviveProgressSFX, transform);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_currentTarget != null && collision.gameObject == _cachedObject)
        {
            _currentTarget = null;
            _cachedObject = null;
            _reviveTimer = 0;
            SoundFXManager.Instance.StopLoop(transform);
        }
    }
}

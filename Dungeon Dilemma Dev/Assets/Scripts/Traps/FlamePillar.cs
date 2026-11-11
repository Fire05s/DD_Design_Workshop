using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priestess;
using PurrNet;

public class FlamePillar : Trap
{
    [Header("Flame Settings")]
    [SerializeField] private float _trapDuration = 3.0f;
    [SerializeField] private float _damageInterval = 0.5f;
    [SerializeField] private SpriteRenderer _flameSprite;
    [Header("Audio")]
    [SerializeField] private AudioClip _flamePillarStart;
    [SerializeField] private AudioClip _flamePillarSustain;
    [SerializeField] private AudioClip _flamePillarEnd;

    // key = challenger, value = the time they can be damaged again
    private Dictionary<GameObject, float> _nextDamageTimeMap = new Dictionary<GameObject, float>();
    private SyncVar<bool> _isActive = new();
    private bool _isCycling = false;

    private List<GameObject> _challengersInRange = new List<GameObject>();

    protected override void ActivateTrap(Collider2D challenger)
    {
        ; // no necessary activation for this looping trap
    }

    private void Update()
    {
        if (isServer && !_isCycling) 
        {
            StartCoroutine(FlameTrapCycle());
        }

        // process damage for everyone on this trap if active, applying per defined interval
        if (_isActive.value && isServer)
        {
            for (int i = _challengersInRange.Count - 1; i >= 0; i--)
            {
                GameObject challenger = _challengersInRange[i];

                // safety check in case object was destroyed
                if (challenger == null) {
                    _challengersInRange.RemoveAt(i);
                    continue;
                }

                if (!_nextDamageTimeMap.ContainsKey(challenger)) _nextDamageTimeMap[challenger] = 0;

                if (Time.time >= _nextDamageTimeMap[challenger])
                {
                    ApplyDamage(challenger);
                    _nextDamageTimeMap[challenger] = Time.time + _damageInterval;
                }
            }
        }

        _flameSprite.enabled = _isActive.value;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) { return; }
        // add challengers on this trap to a tracking list
        if (other.CompareTag("Challenger") && !_challengersInRange.Contains(other.gameObject))
        {
            _challengersInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isServer) { return; }
        // remove challengers leaving this trap from the tracking list
        if (other.CompareTag("Challenger") || other.CompareTag("Dead"))
        {
            _challengersInRange.Remove(other.gameObject);
            if (_nextDamageTimeMap.ContainsKey(other.gameObject)) 
                _nextDamageTimeMap.Remove(other.gameObject);
        }
    }

    private IEnumerator FlameTrapCycle()
    {
        // active damaging state
        _isCycling = true;
        _isActive.value = true;

        // play starting audio sound
        SoundFXManager.Instance?.PlaySoundFXClip(_flamePillarStart, transform.position);
        yield return new WaitForSeconds(0.2f);

        // play sustained audio sound
        SoundFXManager.Instance?.PlayLoop(_flamePillarSustain, transform);

        yield return new WaitForSeconds(_trapDuration - 0.2f);

        // play ending audio sound
        SoundFXManager.Instance?.PlaySoundFXClip(_flamePillarEnd, transform.position);
        yield return new WaitForSeconds(0.2f);

        // inactive off state
        _isActive.value = false;
        _nextDamageTimeMap.Clear();

        // end sustained audio sound
        SoundFXManager.Instance?.StopLoop(transform);

        yield return new WaitForSeconds(_cooldown);

        // resetting cycle
        _isCycling = false;
    }

    private void ApplyDamage(GameObject challengerObject)
    {
        HealthHandler health = challengerObject.GetComponent<HealthHandler>();
        if (health != null)
        {
            health.TakeDamageServerRpc(_damage, new List<StatusEffectType>(){StatusEffectType.Burn}, transform);
        }
    }
}
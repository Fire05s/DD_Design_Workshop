using PurrNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : NetworkBehaviour
{
    public static SoundFXManager Instance { get; private set; }
    [Header("Networked Audio Pool")]
    [SerializeField] private NetworkAudioSource audioSourcePrefab;
    [SerializeField] private int initialPoolSize = 10;
    [Header("Local Audio Pool")]
    [SerializeField] private AudioSource localAudioSourcePrefab;
    [SerializeField] private int localPoolSize = 5;

    [Header("Settings")]
    [SerializeField] private AudioClip _defaultAudio;
    [SerializeField, Range(0f, 1f)]
    private float _globalSFXVolume = 1;
    [Range(0f, 1f)]
    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private bool _isMuted;

    private SyncQueue<NetworkAudioSource> _pool = new();
    private Queue<AudioSource> _localPool = new();
    private SyncDictionary<Transform, NetworkAudioSource> _activeLoops = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeLocalPool();
    }

    protected override void OnSpawned()
    {
        if (!isServer) return;
        InitializePool();
    }

    //      NETWORKED SFX AUDIO
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            NetworkAudioSource source = Instantiate(audioSourcePrefab, transform);
            //ToggleOnClients(source.gameObject, false);
            source.clip = _defaultAudio;
            _pool.Enqueue(source);
        }
    }

    private NetworkAudioSource GetSource()
    {
        if (_pool.Count > 0)
        {
            var source = _pool.Dequeue();
            //ToggleOnClients(source.gameObject, true);
            return source;
        }

        return Instantiate(audioSourcePrefab, transform);
    }

    private void ReturnToPool(NetworkAudioSource source)
    {
        source.Stop();
        //ToggleOnClients(source.gameObject, false);
        source.clip = _defaultAudio;
        _pool.Enqueue(source);
    }

    [ObserversRpc(runLocally:true)]
    private void ToggleOnClients(GameObject obj, bool isActive)
    {
        obj.SetActive(isActive);
    }

    [ServerRpc]
    public void PlaySoundFXClip(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null || _isMuted) return;

        NetworkAudioSource source = GetSource();

        source.clip = clip;
        source.transform.position = position;
        source.volume = volume * _globalSFXVolume;

        // Slight pitch variation
        source.pitch = Random.Range(0.95f, 1.05f);

        source.Play();
        StartCoroutine(ReturnAfterPlay(source));
    }

    private IEnumerator ReturnAfterPlay(NetworkAudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        ReturnToPool(source);
    }

    public void PlayRandomSoundFXClip(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        if (clips == null || clips.Length == 0 || _isMuted) return;

        int rand = Random.Range(0, clips.Length);
        PlaySoundFXClip(clips[rand], position, volume);
    }

    [ServerRpc]
    public void PlayLoop(AudioClip clip, Transform owner, float volume = 1f)
    {
        if (clip == null || _activeLoops.ContainsKey(owner))
            return;

        var source = GetSource();
        source.transform.position = owner.position;
        source.clip = clip;
        source.volume = volume * _globalSFXVolume;
        source.loop = true;
        source.Play();

        _activeLoops.Add(owner, source);
    }

    [ServerRpc]
    public void SetLoopPitch(Transform owner, float pitch)
    {
        if (!_activeLoops.TryGetValue(owner, out var source))
            return;

        source.pitch = pitch;
    }

    [ServerRpc]
    public void StopLoop(Transform owner)
    {
        if (!_activeLoops.TryGetValue(owner, out var source))
            return;

        source.loop = false;
        ReturnToPool(source);
        _activeLoops.Remove(owner);
    }

    //      LOCAL SFX AUDIO
    private void InitializeLocalPool()
    {
        for (int i = 0; i < localPoolSize; i++)
        {
            AudioSource source = Instantiate(localAudioSourcePrefab, transform);
            source.gameObject.SetActive(false);
            _localPool.Enqueue(source);
        }
    }

    private AudioSource GetLocalSource()
    {
        if (_localPool.Count > 0)
        {
            var source = _localPool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }

        return Instantiate(localAudioSourcePrefab, transform);
    }

    private void ReturnLocalSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.loop = false;
        source.gameObject.SetActive(false);
        _localPool.Enqueue(source);
    }

    public void PlayLocalSound(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null || _isMuted) return;

        var source = GetLocalSource();
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume * _globalSFXVolume;
        source.pitch = Random.Range(0.95f, 1.05f);

        source.Play();
        StartCoroutine(ReturnLocalAfterPlay(source));
    }

    private IEnumerator ReturnLocalAfterPlay(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        ReturnLocalSource(source);
    }

    private void LateUpdate()
    {
        if (!isServer) return;
        // Keep loop sources following owners
        foreach (var pair in _activeLoops)
        {
            if (pair.Key != null)
                pair.Value.transform.position = pair.Key.transform.position;
        }
    }

    public void ToggleSFX()
    {
        _isMuted = !_isMuted;
    }

    public void SFXVolume(float volume)
    {
        _globalSFXVolume = volume;
        localAudioSourcePrefab.volume = _globalSFXVolume * _masterVolume;
    }
    public void MasterVolume(float volume)
    {
        _masterVolume = volume;
        localAudioSourcePrefab.volume = _globalSFXVolume * _masterVolume;
    }
}

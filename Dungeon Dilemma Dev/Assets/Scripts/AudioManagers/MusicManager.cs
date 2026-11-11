using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Library")]
    [SerializeField] private Sound[] _musicTracks;

    [Header("Audio Source")]
    [SerializeField] private AudioSource _musicSource;

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float _musicVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private float _sceneTransition = 5f;

    [Header("Scenes")]
    [SerializeField] private string _lobbySceneName;

    private Coroutine _fadeRoutine;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource.loop = true;
        _musicSource.volume = _musicVolume;
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        PlayMusic("Menu");
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
    {
        if (nextScene.name == _lobbySceneName)
        {
            PlayMusic("Menu", true, _sceneTransition);
        }
        else
        {
            PlayMusic("Setup", true, _sceneTransition);
        }
    }

    // PLAY MUSIC
    public void PlayMusic(string name, bool fade = false, float fadeTime = 1f)
    {
        Sound track = Array.Find(_musicTracks, x => x.name == name);

        if (track == null)
        {
            Debug.LogWarning($"No music found: {name}");
            return;
        }

        if (_musicSource.clip == track.clip)
            return;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        if (fade)
            _fadeRoutine = StartCoroutine(FadeAndSwitch(track.clip, fadeTime));
        else
        {
            _musicSource.clip = track.clip;
            _musicSource.Play();
        }
    }

    // STOP MUSIC
    public void StopMusic(bool fade = false, float fadeTime = 1f)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        if (fade)
            _fadeRoutine = StartCoroutine(FadeOut(fadeTime));
        else
            _musicSource.Stop();
    }

    // FADE SYSTEM
    private IEnumerator FadeAndSwitch(AudioClip newClip, float duration)
    {
        yield return Fade(0f, duration);

        _musicSource.clip = newClip;
        _musicSource.Play();

        yield return Fade(_musicVolume, duration);
    }

    private IEnumerator FadeOut(float duration)
    {
        yield return Fade(0f, duration);
        _musicSource.Stop();
    }

    private IEnumerator Fade(float target, float duration)
    {
        float start = _musicSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        _musicSource.volume = target;
    }

    // SETTINGS
    public void ToggleMusic()
    {
        _musicSource.mute = !_musicSource.mute;
    }

    public void MusicVolume(float volume)
    {
        _musicVolume = volume;
        _musicSource.volume = _musicVolume * _masterVolume;
    }

    public void MasterVolume(float volume)
    {
        _masterVolume = volume;
        _musicSource.volume = _musicVolume * _masterVolume;
    }
}

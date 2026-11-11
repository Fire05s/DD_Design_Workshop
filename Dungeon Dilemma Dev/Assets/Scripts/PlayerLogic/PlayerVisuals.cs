using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class PlayerVisuals : NetworkBehaviour, IFlashable, IEmittable
{
    private SpriteRenderer _sprite;
    public Camera _camera;
    private ParticleSystem _particleSystem;
    private HealthHandler _healthHandler;

    public void Start()
    {
        SetComponentReferences();
        _healthHandler.OnAttacked += TakeDamageFlash;
    }


    /// <summary>
    /// Set component references for an instance of PlayerVisuals
    /// </summary>
    public void SetComponentReferences()
    {
        _sprite = gameObject.GetComponentInChildren<SpriteRenderer>();
        _camera = gameObject.GetComponentInChildren<Camera>();
        _particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        _healthHandler = gameObject.GetComponentInChildren<HealthHandler>();
    }

    /// <summary>
    /// Flash red and play blood particles for taking damage
    /// </summary>
    public void TakeDamageFlash(Vector2 source, bool isStatusEffect)
    {
        if (!_sprite)
        {
            return;
        }

        StartCoroutine(FlashSpriteColor(Color.red));
    }

    /// <summary>
    /// Flip the sprite horizontally
    /// </summary>
    /// <param name="xVelocity">A positive or negative float representing x velocity</param>
    public void FlipSprite(float xVelocity)
    {
        if (!_sprite)
        {
            return;
        }

        if (xVelocity >= 0)
        {
            _sprite.flipX = false;
        }
        else
        {
            _sprite.flipX = true;
        }
    }

    /// <summary>
    /// Set the sprite color and play particles for a set duration
    /// </summary>
    /// <param name="color">Color to set the sprite, uses rgba</param>
    /// <param name="timeLength">Length of flash, defaults to 0.2</param>
    /// <returns></returns>
    private IEnumerator FlashSpriteColor(Color color, float timeLength = 0.2f)
    {
        Color originalColor = _sprite.color;
        _sprite.color = color;
        ActivateParticles();
        yield return new WaitForSeconds(timeLength);
        if (_sprite.color == color)
        {
            _sprite.color = originalColor;
        }
        DeactivateParticles();
    }

    /// <summary>
    /// Public method to shake the player's camera
    /// </summary>
    /// <param name="intensity">The multiplicative factor for the camera shake, defaulted to 0.1x</param>
    /// <param name="duration">How long the camera should be shaking, defaulted to 0.2 seconds</param>
    public void CameraShake(float intensity = 0.1f, float duration = 0.2f)
    {
        if (!_camera)
        {
            return;
        }
        StartCoroutine(ShakeRoutine(intensity, duration, _camera.transform.localPosition));
    }

    /// <summary>
    /// Coroutine method for CameraShake controlling how much and how long the camera is shaking
    /// </summary>
    /// <param name="intensity">The multiplicative factor for the camera shake</param>
    /// <param name="duration">How long the camera should be shaking</param>
    /// <returns></returns>
    private IEnumerator ShakeRoutine(float intensity, float duration, Vector3 originalPos)
    {
        float elapsed = 0f;
        float shakeMultiplier = PlayerPrefs.GetFloat("CameraShake" + owner.Value, 100f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = UnityEngine.Random.Range(-1f, 1f) * intensity * (shakeMultiplier / 100);
            float y = UnityEngine.Random.Range(-1f, 1f) * intensity * (shakeMultiplier / 100);

            _camera.transform.localPosition = originalPos + new Vector3(x, y, 0);

            yield return null;
        }

        _camera.transform.localPosition = originalPos;
    }

    /// <summary>
    /// Activate the particle effects (blood) for this player
    /// </summary>
    public void ActivateParticles()
    {
        if (!_particleSystem)
        {
            return;
        }

        _particleSystem.Play();
    }

    /// <summary>
    /// Deactivate the particle effects (blood) for this player
    /// </summary>
    public void DeactivateParticles()
    {
        if (!_particleSystem)
        {
            return;
        }

        _particleSystem.Stop();
    }
}
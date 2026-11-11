using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class UnitVisuals : MonoBehaviour, IFlashable, IEmittable
{
    private SpriteRenderer _sprite;
    private ParticleSystem _particleSystem;
    private HealthHandler _healthHandler;
    private TDHealthHandler _tdHealthHandler;

    public void Start()
    {
        SetComponentReferences();
        if (_healthHandler != null)
        {
            _healthHandler.OnAttacked += TakeDamageFlash;
        }
        else if (_tdHealthHandler != null)
        {
            _tdHealthHandler.OnAttacked += TakeDamageFlash;
        }
    }


    /// <summary>
    /// Set component references for an instance of UnitVisuals
    /// </summary>
    public void SetComponentReferences()
    {
        _sprite = gameObject.GetComponentInChildren<SpriteRenderer>();
        _particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        _healthHandler = gameObject.GetComponentInChildren<HealthHandler>();
        _tdHealthHandler = gameObject.GetComponentInChildren<TDHealthHandler>();
    }

    /// <summary>
    /// Flash red for taking damage
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
    /// Set the sprite color for a set duration
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
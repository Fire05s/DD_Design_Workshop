using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class ParticleVisuals : MonoBehaviour, IEmittable
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private short _duration;

    public void Start()
    {
        ActivateParticles();
    }

    public void Update()
    {
        // deactivate particle effect on death
        if (transform.parent != null && transform.parent.CompareTag("Dead"))
        {
            DeactivateParticles();
        }
    }

    /// <summary>
    /// Activate the particle effects for this object and destroy it after the given duration
    /// </summary>
    public void ActivateParticles()
    {
        if (!_particleSystem)
        {
            return;
        }

        transform.localScale = Vector3.one; // normalize scale of particle effect
        _particleSystem.Play();
        Destroy(gameObject, _duration);
    }

    /// <summary>
    /// Deactivate the particle effects for this object
    /// </summary>
    public void DeactivateParticles()
    {
        if (!_particleSystem)
        {
            return;
        }

        _particleSystem.Stop();
        Destroy(gameObject);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the visuals of a fire type (particles and realtime light). It's completely offline, no networking behaviour.
/// </summary>
public class FireEffect : MonoBehaviour
{
    public enum EffectType
    {
        SmolderingEmbers = 0,
        RegularFire = 1,
        BlueSpiritFire = 2,
        RegularFireBig = 3,
        BlueSpiritFireBig = 4
    }

    public bool animateLight;
    public AnimationCurve lightCurve;

    private bool _isActive;
    private List<ParticleSystem> _particleSystems = new();
    private List<Light> _lights = new();

    private void Awake()
    {
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>()) _particleSystems.Add(ps);
        foreach (Light l in GetComponentsInChildren<Light>()) _lights.Add(l);
    }

    private void Update()
    {
        if(_isActive && animateLight)
            foreach (Light l in GetComponentsInChildren<Light>())
                l.intensity = lightCurve.Evaluate(Time.deltaTime);
    }

    public void Activate()
    {
        _isActive = true;
        foreach (ParticleSystem ps in _particleSystems) ps.Play();
        foreach (Light l in _lights) l.enabled = true;
    }

    public void Deactivate()
    {
        _isActive = false;
        foreach (ParticleSystem ps in _particleSystems) ps.Stop();
        foreach (Light l in _lights) l.enabled = false;
    }
}
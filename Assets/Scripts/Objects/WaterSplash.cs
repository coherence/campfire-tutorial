using UnityEngine;

public class WaterSplash : MonoBehaviour
{
    public ParticleSystem waterSplashEffect;
    public ParticleSystem waterRunEffect;
    
    [Header("Audio")]
    public SoundHandler soundHandler;
    public SFXPreset wadeSFX;
    public SFXPreset splashSFX;

    public bool IsUnderwater { get; private set; }
    
    private float _waterLevel = -6.8f; // Hardcoded from the value of the water in the scene 
    private bool _wasUnderWater;
    private Vector3 _lastPosition;

    private void FixedUpdate()
    {
        IsUnderwater = transform.position.y < _waterLevel;
        if (IsUnderwater && (_wasUnderWater != IsUnderwater)) Splash();

        // Check if player moved, if so, trigger some particles 
        if (IsUnderwater && (_lastPosition - transform.position).magnitude > .1f)
        {
            SmallSplash();
            _lastPosition = transform.position;
        }

        if (!IsUnderwater) _lastPosition = transform.position;
        
        _wasUnderWater = IsUnderwater;
    }

    private void Splash()
    {
        waterSplashEffect.Play();
        soundHandler.Play(splashSFX);
    }

    private void SmallSplash()
    {
        waterRunEffect.Play();
        soundHandler.Play(wadeSFX);
    }
}
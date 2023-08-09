using UnityEngine;

/// <summary>
/// These events are running for remote players too, so they play particles and sounds as a result of the animation,
/// without having to network those. In addition, they will play in perfect sync with the animation.
/// </summary>
public class PlayerAnimationEvents : MonoBehaviour
{
    [Header("Audio")]
    public SoundHandler soundHandler;
    public WaterSplash waterSplash;
    public SFXPreset footstepSFX;
    public SFXPreset waterFootstepSFX;
    public SFXPreset jumpSFX;
    public SFXPreset landSFX;
    
    [Header("Particles")]
    public ParticleSystem runParticles;
    public ParticleSystem jumpParticles;
    public ParticleSystem landParticles;

    private Transform _landParticlesTransform;

    private void Awake()
    {
        _landParticlesTransform = landParticles.transform;
    }

    public void PlayStepSound(AnimationEvent evt)
    {
        if (evt.animatorClipInfo.weight > 0.5)
        {
            soundHandler.Play(waterSplash.IsUnderwater ? waterFootstepSFX : footstepSFX);
        }
    }

    public void PlayRunParticles()
    {
        runParticles.Play();
    }

    public void StopRunParticles()
    {
        runParticles.Stop();
    }

    public void PlayLandEffects()
    {
        // Calculate the landing particles position before playing them,
        // to account for the fact that remote players might still be in the air when this is invoked
        Ray ray = new(transform.position + Vector3.up * .5f, Vector3.down);
        Physics.Raycast(ray, out RaycastHit raycastHit, 1.3f);
        _landParticlesTransform.position = raycastHit.point + Vector3.up * .1f;

        landParticles.Play();
        soundHandler.Play(landSFX);
    }

    public void PlayJumpEffects()
    {
        soundHandler.Play(jumpSFX);
        jumpParticles.Play();
        StopRunParticles();
    }
}
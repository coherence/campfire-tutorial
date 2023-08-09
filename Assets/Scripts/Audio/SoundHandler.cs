using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Used to play SFXs on an existing AudioSource. Individual SFX will be played using PlayOneShot,
/// looping ones will play directly on the AudioSource.
/// </summary>
public class SoundHandler : MonoBehaviour
{
    public AudioSource audioSource;
    
    /// <summary>
    /// <para>Plays a sound, either one shot or looping depending on the <see cref="SFXPreset.loops"/> parameter of the <see cref="SFXPreset"/>.</para>
    /// <para>Note: if playing a one-shot SFX while a looping one is playing, the volume and pitch
    /// of the looping one might be modified. It is suggested to use multiple instances of SoundHandler.</para>
    /// </summary>
    public void Play(SFXPreset preset)
    {
        if (!audioSource.enabled) return;
        
        audioSource.spatialBlend = preset.spatialBlend;
        audioSource.pitch = preset.randomisePitch
            ? Random.Range(preset.randomPitchRange.x, preset.randomPitchRange.y)
            : 1f;
        audioSource.maxDistance = preset.maxDistance;

        if (preset.loops)
        {
            audioSource.loop = true;
            audioSource.clip = preset.GetClip();
            audioSource.volume = preset.volume;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(preset.GetClip(), preset.volume);
        }
    }

    /// <summary>Attempts to auto setup the AudioSource reference.</summary>
    private void Reset()
    {
        if (TryGetComponent(out AudioSource componentFound))
            audioSource = componentFound;
    }
}
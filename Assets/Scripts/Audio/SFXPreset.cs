using UnityEngine;

[CreateAssetMenu(fileName = "SFXPreset", menuName = "coherence Campfire/SFX Preset")]
public class SFXPreset : ScriptableObject
{
    [SerializeField] public AudioClip[] _audioClips;

    [Header("Pitch and volume")]
    [Range(0f, 1f)] public float volume = 1f;
    public bool randomisePitch;
    public Vector2 randomPitchRange = new(0.8f, 1.2f);

    [Header("Playback settings")]
    public bool loops = false;

    [Header("Spatial settings")]
    [Range(0f, 1f)] public float spatialBlend;
    public float maxDistance = 500f;

    private int _previousRandomClip;

    /// <summary>
    /// Use this method to get a random clip each time.
    /// </summary>
    public AudioClip GetClip()
    {
        switch (_audioClips.Length)
        {
            case 0:
                Debug.LogError("SFX Preset has no clips.", this);
                return null;
            case 1:
                return _audioClips[0];
            default:
                return GetRandomClip();
        }
    }

    private AudioClip GetRandomClip()
    {
        int randomInt = Random.Range(0, _audioClips.Length);

        if (randomInt == _previousRandomClip)
            // Just move to the next one, random enough
            randomInt = (randomInt + 1) % _audioClips.Length;

        _previousRandomClip = randomInt;
        return _audioClips[randomInt];
    }

    /// <summary>
    /// Used to generate a random pitch.
    /// </summary>
    public float GetRandomPitchValue()
    {
        return Random.Range(randomPitchRange.x, randomPitchRange.y);
    }
    
#if UNITY_EDITOR        
    public void PlayPreview(AudioSource previewAudioSource)
    { 
        previewAudioSource.clip = GetClip();
        previewAudioSource.spatialBlend = 0;
        previewAudioSource.volume = volume;
        previewAudioSource.pitch = randomisePitch ? GetRandomPitchValue() : 1f;
        previewAudioSource.Play();
    }

    public void Stop(AudioSource previewAudioSource)
    {
        previewAudioSource.Stop();
    }
#endif
}
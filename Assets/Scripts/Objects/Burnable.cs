using System.Collections;
using Coherence.Toolkit;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Represents a networked object that can be burned on the the <see cref="Campfire"/>.
/// Disabled on non-authoritative Clients.
/// </summary>
public class Burnable : MonoBehaviour
{
    public SoundHandler soundHandler;
    public FireEffect.EffectType fireEffectType = FireEffect.EffectType.RegularFire;
    public FireEffect.EffectType bigFireEffectType = FireEffect.EffectType.RegularFireBig;
    public SFXPreset burntSFX;
    public SFXPreset thudSFX;
    
    [HideInInspector] public bool checkCollisions = true;

    public event UnityAction Burned;

    private Campfire _campfire;
    private Collider _collider;
    private CoherenceSync _sync;
    private bool _hasBurned;

    private void Awake()
    {
        _sync = GetComponent<CoherenceSync>();
        _collider = GetComponent<Collider>();
        _campfire = FindObjectOfType<Campfire>();
    }

    private void OnEnable()
    {
        _hasBurned = false;
    }

    private void FixedUpdate()
    {
        if(checkCollisions && !_hasBurned) CheckFireplaceCollisions();
    }

    private void CheckFireplaceCollisions()
    {
        if (_collider.bounds.Intersects(_campfire.CollisionBounds))
        {
            StartCoroutine(GetBurned());
        }
    }

    private IEnumerator GetBurned()
    {
        // Informs GrabAction to invoke LetGo,
        // so if a player is carrying the object it will be as if they had released it
        Burned?.Invoke();

        _hasBurned = true;
        _campfire.BurnObjectLocal(_sync);

        _sync.SendConnectedEntity(); //TODO: remove this later when we release a patch

        yield return new WaitForEndOfFrame();

        _sync.ReleaseInstance();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(gameObject.activeInHierarchy)
            soundHandler.Play(thudSFX);
    }
}
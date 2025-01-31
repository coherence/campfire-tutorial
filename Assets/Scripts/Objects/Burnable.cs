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
    public bool timedSelfDestruct; // Used so logs remove themselves when they exist for too long
    
    [HideInInspector] public bool checkCollisions = true;

    public event UnityAction Burned;

    private Collider _collider;
    private CoherenceSync _sync;
    private bool _hasBurned;
    private float _creationTime;
    private readonly float _selfDestructTime = 1800f;

    private void Awake()
    {
        _sync = GetComponent<CoherenceSync>();
        _collider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        _hasBurned = false;
        _creationTime = Time.time;
    }

    private void Update()
    {
        if(timedSelfDestruct && Time.time > _creationTime + _selfDestructTime) Remove();
    }

    private void FixedUpdate()
    {
        if(checkCollisions && !_hasBurned) CheckFireplaceCollisions();
    }

    private void CheckFireplaceCollisions()
    {
        if (Campfire.TryGet(out var campfire) && _collider.bounds.Intersects(campfire.CollisionBounds))
        {
            Burn(campfire);
        }
    }

    private void Burn(Campfire campfire)
    {
	    campfire.BurnObjectLocal(_sync);
        Remove();
    }

    private void Remove()
    {
        // Informs GrabAction to invoke LetGo,
        // so if a player is carrying the object it will be as if they had released it
        Burned?.Invoke();

        _hasBurned = true; // Prevents more campfire collisions
        GetComponent<Grabbable>().isBeingCarried = true; // Prevents pickup requests (GrabAction)
        GetComponentInChildren<Interactable>().SetCollider(false); // Prevents object highlighting (InteractionInput)
        // These are reset in the relevant OnEnable

        _sync.ReleaseInstance();
    }

    private void OnCollisionEnter(Collision _)
    {
        if(gameObject.activeInHierarchy)
            soundHandler.Play(thudSFX);
    }
}
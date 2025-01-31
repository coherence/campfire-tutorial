using System.Collections.Generic;
using UnityEngine;
using Coherence;
using Coherence.Toolkit;

/// <summary>
/// Represents a networked campfire, where <see cref="Burnable"/> objects can be burned.
/// Disabled on non-authoritative Clients.
/// </summary>
public class Campfire : MonoBehaviour
{
    private static Campfire _instance;

    [Header("Runtime state")]
    [Sync] public int activeFireEffect;
    [Sync] public float fireTimer; // Time left to burn before fire goes off
    [Sync] public float bigFireTimer; // If it's over 0, big fire is on

    [Header("Configuration")]
    public float fireDuration = 5f;
    public float bigFireDuration = 5f;
    public float teamEffortLength = 2.5f; // If players put two things on the fire within this time, a big fire starts
    
    [Header("Components and assets")]
    public List<FireEffect> fireEffects; // Effects are listed in the array in the order defined by FireEffect.EffectType enum
    public ParticleSystem particlesFlameBurst;
    public SoundHandler soundHandler;
    public SoundHandler loopingAudioHandler;
    public SFXPreset normalFireLoop;
    public SFXPreset bigFireLoop;
    public SFXPreset magicFireLoop;
    public SFXPreset bigMagicFireLoop;
    public SFXPreset smolderingEmbersLoop;
    public SFXPreset igniteSFX;
    public CoherenceSyncConfigRegistry configRegistry;
    
    public Bounds CollisionBounds => _collider.bounds;

    private Collider _collider;
    private CoherenceSync _sync;
    private float _teamEffortTimer; // Time left to put another item on the fire, to provoke a big fire 

    private bool IsBigFireOn => bigFireTimer > 0;
    
    public static bool TryGet(out Campfire campfire)
    {
        if(!_instance)
        {
            _instance =
#if UNITY_6000_0_OR_NEWER || UNITY_2022_3 || UNITY_2021_3
                FindAnyObjectByType<Campfire>(FindObjectsInactive.Exclude);
#else
                FindObjectOfType<Campfire>();
#endif
        }

        return campfire = _instance;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _sync = GetComponent<CoherenceSync>();
        _sync.CoherenceBridge.onLiveQuerySynced.AddListener(OnLiveQuerySynced);
    }

    private void OnEnable() => _instance = this;

    /// <summary>
    /// If this build is being run as a Simulator, the Simulator takes authority over the campfire.
    /// Normal Clients won't steal authority from each other, but they might eventually still receive it if the authority disconnects.
    /// </summary>
    private void OnLiveQuerySynced(CoherenceBridge bridge)
    {
        if (_sync.HasStateAuthority)
        {
            // If this client has state authority on first sync of the live query, it's because
            // they are the first to connect ever, they are the one bringing the campfire into the network simulation
            FireStateChanged((int)FireEffect.EffectType.SmolderingEmbers, (int)FireEffect.EffectType.SmolderingEmbers, "");
        }
        else
        {
            // Sync with whatever state the already-existing persistent campfire is in
            FireStateChanged((int)FireEffect.EffectType.SmolderingEmbers, activeFireEffect, "");
            
            if (SimulatorUtility.IsSimulator)
            {
                // If this client is a Simulator, it takes over
                _sync.RequestAuthority(AuthorityType.Full);
            }
            else
            {
                // If the entity is not orphaned, it means someone else is connected and already has control of it
                if (!_sync.EntityState.IsOrphaned)
                {
                    // Listen to a potential authority change in case another Client or the Simulator leaves
                    _sync.OnStateAuthority.AddListener(OnStateAuthority);
                }
            }
        }

        _sync.CoherenceBridge.onLiveQuerySynced.RemoveListener(OnLiveQuerySynced);
    }

    private void OnStateAuthority()
    {
        FireStateChanged((int)FireEffect.EffectType.SmolderingEmbers, activeFireEffect, "");
    }

    /// <summary>
    /// Consume and eventually die down fire.
    /// </summary>
    private void Update()
    {
        if (fireTimer > 0f)
        {
            if (fireTimer > 0f) fireTimer -= Time.deltaTime;
            if (_teamEffortTimer > 0f) _teamEffortTimer -= Time.deltaTime;
            
            if (fireTimer <= 0f)
            {
                // Fire dies out completely
                ChangeFireState(FireEffect.EffectType.SmolderingEmbers);
            }
            else
            {
                if (bigFireTimer > 0f)
                {
                    bigFireTimer -= Time.deltaTime;
                    if (bigFireTimer <= 0f)
                    {
                        // Tone down to regular fire - or regular magic fire -
                        // depending on which big fire was playing so far
                        FireEffect.EffectType newEffect = activeFireEffect == (int)FireEffect.EffectType.RegularFireBig
                            ? FireEffect.EffectType.RegularFire
                            : FireEffect.EffectType.BlueSpiritFire;
                        ChangeFireState(newEffect);
                        bigFireTimer = 0f;
                    }
                }
            }
        }
    }

    private void OnDisable() => _instance = null;

    /// <summary>
    /// Invoked locally by a <see cref="Burnable"/> that collided with the campfire. Can be on the campfire authority, or not.
    /// Triggers a message to everyone (<see cref="PlayInstantEffects"/>) to play instant particle effects for the quickest visual feedback,
    /// plus a message to the authority (<see cref="BurnObject"/>) which will calculate the new fire state,
    /// which will then be broadcasted to all the others.
    /// </summary>
    public void BurnObjectLocal(CoherenceSync syncToBurn)
    {
        PlayInstantEffects();
        _sync.SendCommand<Campfire>(nameof(PlayInstantEffects), MessageTarget.Other);
        
        if (_sync.HasStateAuthority)
            BurnObject(syncToBurn.CoherenceSyncConfig.ID);
        else
            _sync.SendCommand<Campfire>(nameof(BurnObject), MessageTarget.AuthorityOnly, syncToBurn.CoherenceSyncConfig.ID);
    }

    /// <summary>
    /// Sent by authority and non-authority to all other Clients regardless of which object was thrown into the fire.
    /// This command is the fastest route to see a visual feedback.
    /// </summary>
    [Command]
    public void PlayInstantEffects()
    {
        particlesFlameBurst.Play();
        soundHandler.Play(igniteSFX);
    }

    [Command(defaultRouting = MessageTarget.AuthorityOnly)]
    public void BurnObject(string syncConfigID)
    {
        if (RetrieveBurnableInConfigRegistry(syncConfigID, out Burnable burnable))
        {
            // Did two objects get burned at the same time? Calculate big fire time.
            if (_teamEffortTimer > 0f) bigFireTimer = bigFireDuration;
            _teamEffortTimer = teamEffortLength;
            fireTimer = fireDuration;

            ChangeFireState(IsBigFireOn ? burnable.bigFireEffectType : burnable.fireEffectType, syncConfigID);
        }
        else
        {
            Debug.LogWarning(
                $"The Burnable with ID {syncConfigID} wasn't found in the CoherenceConfigRegistry! Can't send it via Command.");
        }
    }

    /// <summary>
    /// Only invoked locally by the authority. Happens when an object gets burned, or in time when the fire timers run out.
    /// </summary>
    private void ChangeFireState(FireEffect.EffectType newEffectType, string syncConfigID = "")
    {
        int newEffectID = (int)newEffectType;
        
        // Invoke locally
        UpdateFireEffects(activeFireEffect, newEffectID, syncConfigID);
        
        // Invoke a different command depending on whether it was an object that provoked the change
        if(string.IsNullOrEmpty(syncConfigID))
            _sync.SendCommand<Campfire>(nameof(FireDiedDown), MessageTarget.Other, activeFireEffect, newEffectID);
        else
            _sync.SendCommand<Campfire>(nameof(FireStateChanged), MessageTarget.Other, activeFireEffect, newEffectID, syncConfigID);
        
        activeFireEffect = newEffectID;
    }

    /// <summary>
    /// Command invoked from the authority on other Clients.
    /// Used to play a new fire effect that happened as a result of time, so no object involved.
    /// Happens when the fire dies out completely (becomes "smoldering embers") or when a big fire
    /// quiets down into a regular sized fire (either normal or magic).
    /// </summary>
    [Command]
    public void FireDiedDown(int oldEffectID, int newEffectID) => UpdateFireEffects(oldEffectID, newEffectID);

    /// <summary>
    /// Command invoked from the authority on other Clients.
    /// Used to play a new fire effect that happened as a result of an object being burned.
    /// </summary>
    [Command]
    public void FireStateChanged(int oldEffectID, int newEffectID, string syncConfigID) => UpdateFireEffects(oldEffectID, newEffectID, syncConfigID);

    /// <summary>
    /// Updates visuals and sound to the new fire effect.
    /// It's invoked directly on the authority, and as a result of 2 network commands on non-authoritative Clients.
    /// </summary>
    private void UpdateFireEffects(int oldEffectID, int newEffectID, string syncConfigID = "")
    {
        // Update the visuals
        fireEffects[oldEffectID].Deactivate();
        fireEffects[newEffectID].Activate();

        // If the state changed because of an object being burnt,
        // we need to play a specific sound effect
        if (!string.IsNullOrEmpty(syncConfigID))
        {
            if (RetrieveBurnableInConfigRegistry(syncConfigID, out Burnable burnable))
            {
                soundHandler.Play(burnable.burntSFX);
            }
            else
            {
                Debug.LogWarning($"The Burnable with ID {syncConfigID} wasn't found in the CoherenceConfigRegistry! No sound was played.");
            }
        }
        
        // Play looping sound effect
        switch (newEffectID)
        {
            case (int)FireEffect.EffectType.SmolderingEmbers:
                loopingAudioHandler.Play(smolderingEmbersLoop);
                break;
            case (int)FireEffect.EffectType.RegularFire:
                loopingAudioHandler.Play(normalFireLoop);
                break;
            case (int)FireEffect.EffectType.BlueSpiritFire:
                loopingAudioHandler.Play(magicFireLoop);
                break;
            case (int)FireEffect.EffectType.RegularFireBig:
                loopingAudioHandler.Play(bigFireLoop);
                break;
            case (int)FireEffect.EffectType.BlueSpiritFireBig:
                loopingAudioHandler.Play(bigMagicFireLoop);
                break;
        }
    }
    
    /// <summary>
    /// Starting from the ID of a <see cref="CoherenceSyncConfig"/>, it finds it in coherence's <see cref="CoherenceSyncConfigRegistry"/>,
    /// and returns a reference to the <see cref="Burnable"/> script attached to the prefab that is associated with the entry.
    /// </summary>
    private bool RetrieveBurnableInConfigRegistry(string syncConfigID, out Burnable burnable)
    {
        foreach (CoherenceSyncConfig config in configRegistry)
        {
            if (config.ID == syncConfigID)
            {
                burnable = ((CoherenceSync)config.Provider.LoadAsset(config.ID)).GetComponent<Burnable>();
                return true;
            }
        }

        burnable = null;
        return false;
    }

    private void OnDestroy()
    {
        if(_sync.CoherenceBridge != null)
            _sync.CoherenceBridge.onLiveQuerySynced.RemoveListener(OnLiveQuerySynced);
        
        _sync.OnStateAuthority.RemoveListener(OnStateAuthority);
    }
}
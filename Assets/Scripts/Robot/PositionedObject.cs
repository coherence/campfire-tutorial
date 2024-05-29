using Coherence;
using Coherence.Toolkit;
using UnityEngine;

/// <summary>
/// <para>A unique object that has a specific position in the scene, so that the <see cref="KeeperRobot"/>
/// can put them back into their place, even if they have been burned on the campfire.</para>
/// <para>The object spawns an <see cref="ObjectAnchor"/> instance that holds information about the position,
/// rotation, and keeps a reference to the original object that spawned it (using its UUID).</para>
/// <para>As a way to spawn and destroy them, PositionedObjects should use the <see cref="UniqueBurnableInstantiator"/>.</para> 
/// </summary>
public class PositionedObject : MonoBehaviour
{
    public CoherenceSync anchorPrefab;
    [Sync] public CoherenceSync objectAnchorSync;

    private CoherenceSync _sync;
    private CoherenceBridge _bridge;
    private string _anchorUniqueId;
    private Interactable _interactableComp;

    private void Awake()
    {
        _interactableComp = GetComponentInChildren<Interactable>();
        _sync = GetComponent<CoherenceSync>();
        _bridge = _sync.CoherenceBridge;
        _anchorUniqueId = $"{_sync.ManualUniqueId}-anchor";
        GetComponent<Burnable>().Burned += OnBurned;
    }

    private void Start()
    {
        if(_bridge != null)
            _bridge.onLiveQuerySynced.AddListener(OnLiveQuerySynced);
    }

    private void OnLiveQuerySynced(CoherenceBridge bridge)
    {
        UniqueObjectReplacement uor = _bridge.UniquenessManager.TryGetUniqueObject(_anchorUniqueId);
        if (uor?.localObject != null)
        {
            // ObjectAnchor is in the scene
            objectAnchorSync = (CoherenceSync)uor.localObject;
            if (!objectAnchorSync.GetComponent<ObjectAnchor>().isObjectPresent)
            {
                // But the original object has been destroyed
                Destroy(gameObject);
            }
        }
        else
        {
            SpawnAnchor();
        }
        
        _bridge.onLiveQuerySynced.RemoveListener(OnLiveQuerySynced);
    }

    private void SpawnAnchor()
    {
        string syncConfigID = _sync.CoherenceSyncConfig.ID;
        string uniqueID = _sync.ManualUniqueId;
        Transform anchorsParent = GameObject.Find("ObjectAnchors").transform;

        _bridge.UniquenessManager.RegisterUniqueId(_anchorUniqueId);
        objectAnchorSync = Instantiate(anchorPrefab, transform.position, transform.rotation, anchorsParent);
        objectAnchorSync.gameObject.name = "ObjectAnchor_" + uniqueID;
        objectAnchorSync.GetComponent<ObjectAnchor>().Init(syncConfigID, uniqueID);
    }

    /// <summary>
    /// Informs the object anchor that its associated object is gone. See <see cref="ObjectAnchor.isObjectPresent"/>.
    /// </summary>
    private void OnBurned()
    {
        if(_bridge != null && _bridge.IsConnected)
            objectAnchorSync.GetComponent<ObjectAnchor>().LinkedObjectBurned();
    }
    
    public void Restore(ObjectAnchor objectAnchor)
    {
        PlayReappearShaderEffect();
        _sync.SendCommand<PositionedObject>(nameof(PlayReappearShaderEffect), MessageTarget.Other);
        
        objectAnchorSync = objectAnchor.GetComponent<CoherenceSync>();
    }

    [Command]
    public void PlayReappearShaderEffect()
    {
        // Assuming one object with just one material
        _interactableComp.objectsToHighlight[0].GetComponent<Renderer>().material.SetFloat("_EffectStart", Time.time);
    }

    private void OnDestroy()
    {
        GetComponent<Burnable>().Burned -= OnBurned;
    }
}
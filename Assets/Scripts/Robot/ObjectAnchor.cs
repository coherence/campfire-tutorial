using Coherence;
using Coherence.Toolkit;
using UnityEngine;

/// <summary>
/// An empty, invisible persistent object that holds all the information necessary to respawn a <see cref="PositionedObject"/>.
/// </summary>
public class ObjectAnchor : MonoBehaviour
{
    [Sync] public string syncConfigId;
    [Sync] public string holdingForUUID;
    [Sync] public bool isObjectPresent;

    private CoherenceSync _sync;

    private void Awake()
    {
        _sync = GetComponent<CoherenceSync>();
    }

    public void Init(string referenceSyncConfigID, string uuid)
    {
        syncConfigId = referenceSyncConfigID;
        holdingForUUID = uuid;
        isObjectPresent = true;
    }

    public void LinkedObjectBurned() => ChangeLinkedObjectState(false);
    public void LinkedObjectReinstated() => ChangeLinkedObjectState(true);

    private void ChangeLinkedObjectState(bool newState)
    {
        if (_sync.HasStateAuthority) ChangeLinkedObjectStateAuth(newState);
        else _sync.SendCommand<ObjectAnchor>(nameof(ChangeLinkedObjectStateAuth), MessageTarget.AuthorityOnly, newState);
    }

    public GameObject GetLinkedObject()
    {
        UniqueObjectReplacement uor = _sync.CoherenceBridge.UniquenessManager.TryGetUniqueObject(holdingForUUID);
        bool found = uor.localObject != null;
        return found ? ((CoherenceSync)uor.localObject).gameObject : null;
    }

    [Command(defaultRouting = MessageTarget.AuthorityOnly)]
    public void ChangeLinkedObjectStateAuth(bool newState)
    {
        isObjectPresent = newState;
    }
}
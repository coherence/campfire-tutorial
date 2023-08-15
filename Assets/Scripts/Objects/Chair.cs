using Coherence;
using Coherence.Toolkit;
using UnityEngine;

public class Chair : MonoBehaviour
{
    public Transform sittingAnchor;
    public CoherenceSync sync;
    
    private Interactable _interactable;

    [Sync] public bool isBusy;

    private void Awake()
    {
        _interactable = GetComponentInChildren<Interactable>();
        sync.OnStateAuthority.AddListener(OnStateAuthority);
    }

    private void OnStateAuthority()
    {
        // Free the chair if it has no children.
        // This could happen if a player who previously had authority
        // disconnected while was siting on the chair
        if(sittingAnchor.childCount == 0) Free();
    }

    public void Occupy()
    {
        if (sync.HasStateAuthority) ChangeState(true);
        else sync.SendCommand<Chair>(nameof(ChangeState), MessageTarget.AuthorityOnly, true);
    }

    public void Free()
    {
        if (sync.HasStateAuthority) ChangeState(false);
        else sync.SendCommand<Chair>(nameof(ChangeState), MessageTarget.AuthorityOnly, false);
    }

    [Command(defaultRouting = MessageTarget.AuthorityOnly)]
    public void ChangeState(bool newBusyState)
    {
        isBusy = newBusyState;
        _interactable.SetCollider(!newBusyState);
    }
}
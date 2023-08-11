using Coherence;
using Coherence.Toolkit;
using UnityEngine;
using UnityEngine.Events;

public class GrabAction : MonoBehaviour, INetworkInteraction
{
    public Animator animator;
    public Transform holdSocket;
    public CoherenceSync sync;
    public SoundHandler soundHandler;

    public bool IsCarryingSomething { get; private set; }

    public event UnityAction<bool> Done;
    public event UnityAction PickedUpObject; 

    private Grabbable _grabbedObject;
    private Rigidbody _grabbedObjectRB;
    private Collider _grabbedObjectCollider;

    public SFXPreset _sfx_grab;
    public SFXPreset _sfx_toss;

    public void RequestPickup(Grabbable grabbableComponent)
    {
        _grabbedObject = grabbableComponent;
        _grabbedObject.PickupValidated += OnPickUpValidated;
        _grabbedObject.RequestPickup();
    }

    /// <summary>
    /// Received a response from the grabbable we're trying to pick up.
    /// Since the object could be remote, this includes a request of authority,
    /// which might lead it to fail if the object is set to not concede authority.
    /// </summary>
    /// <param name="success">Whether the pickup was authorized or not.</param>
    private void OnPickUpValidated(bool success)
    {
        _grabbedObject.PickupValidated -= OnPickUpValidated;
        if (success)
        {
            PickedUpObject?.Invoke();
            PickUp(_grabbedObject);
        }
        else
        {
            // Pickup can fail when a Grabbable that was free up to a moment ago JUST
            // got picked up by another player on the network.
            // Locally this client is not aware yet because the Grabbable.isBeingCarried property
            // hasn't synced yet, but when requesting authority for the pickup - they get rejected
            // in the Grabbable.OnAuthorityRequested callback.
            Done?.Invoke(false);
            _grabbedObject = null;
        }
    }

    public void PickUp(Grabbable target)
    {
        animator.SetBool("CarryingBig", true);
        PlayPickUpSound();
        sync.SendCommand<GrabAction>(nameof(PlayPickUpSound), MessageTarget.Other);
        _grabbedObjectRB = target.gameObject.GetComponent<Rigidbody>();
        _grabbedObjectCollider = target.GetComponent<Collider>();
        _grabbedObjectRB.isKinematic = true;
        _grabbedObjectCollider.enabled = false;
        _grabbedObject = target;
        _grabbedObject.transform.SetParent(holdSocket, false);
        _grabbedObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        if (_grabbedObject.TryGetComponent(out Burnable burnable)) burnable.Burned += OnGrabbableBurned;
        IsCarryingSomething = true;
    }

    [Command]
    public void PlayPickUpSound()
    {
        soundHandler.Play(_sfx_grab);
    }

    /// <summary>
    /// Contains the actions that are performed on the grabbable.
    /// Only happens when the player presses the input to throw/drop the interactable.
    /// </summary>
    public void Drop(float throwStrength = 0f)
    {
        PlayDropSound();
        sync.SendCommand<GrabAction>(nameof(PlayDropSound), MessageTarget.Other);
        _grabbedObjectCollider.enabled = true;
        _grabbedObjectRB.isKinematic = false;
        _grabbedObjectRB.AddForce(throwStrength * transform.forward, ForceMode.VelocityChange);
        _grabbedObjectRB.AddTorque(-transform.right * throwStrength * 1f, ForceMode.VelocityChange);

        LetGo(false);
    }

    [Command]
    public void PlayDropSound()
    {
        soundHandler.Play(_sfx_toss);
    }

    /// <summary>
    /// Happens in response to the Grabbable being burned, while still being held by the player.
    /// It invokes <see cref="LetGo"/> with the addition of invalidating the interaction target,
    /// so that the player loses focus of the Grabbable and cannot pick it up again - because it's been disabled.
    /// </summary>
    private void OnGrabbableBurned() => LetGo(true);

    /// <summary>
    /// Contains the actions that are performed by the character on itself,
    /// that is, the ones that happen when the <see cref="Grabbable"/> is let go.
    /// </summary>
    private void LetGo(bool clearInteractionTarget)
    {
        animator.SetBool("CarryingBig", false);

        _grabbedObject.Release();
        _grabbedObject.transform.SetParent(null, true);
        if (_grabbedObject.TryGetComponent(out Burnable burnable)) burnable.Burned -= OnGrabbableBurned;

        _grabbedObjectRB = null;
        _grabbedObjectCollider = null;
        _grabbedObject = null;
        IsCarryingSomething = false;
        
        Done?.Invoke(clearInteractionTarget); //Informs InteractionInput
    }

    private void OnDisable()
    {
        // Since the player gets destroyed when exiting Play mode,
        // make sure to unparent the crate by dropping it
        if (IsCarryingSomething) Drop();
    }
}
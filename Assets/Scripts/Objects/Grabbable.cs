using System;
using Coherence;
using Coherence.Connection;
using Coherence.Toolkit;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    /// <summary>
    /// This is a flag used to check if a Grabbable can be picked up or not, set to sync.
    /// However, since syncing over the network takes time, it might still be false when
    /// a remote player is trying to pick the Grabbable up, so we also filter authority
    /// requests with the <see cref="OnAuthorityRequested"/> callback.
    /// </summary>
    [Sync] public bool isBeingCarried;

    public event Action<bool> PickupValidated;

    private CoherenceSync _sync;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private bool _pickupRequested;
    private bool _collisionHappened;
    private float _lastAuthorityChangeTime;
    private float _floatingForce = 40f;
    private float _floatingY = -7.5f; //the water level

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _sync = GetComponent<CoherenceSync>();
    }

    private void OnEnable()
    {
        _sync.OnAuthorityRequested += OnAuthorityRequested;
        _sync.OnStateAuthority.AddListener(OnStateAuthority);
        _sync.OnStateRemote.AddListener(OnStateRemote);

        // This check is fundamental for remote Log objects being enabled,
        // since they are coming from the object pool.
        // They fire the OnEnable too late to hook into OnStateRemote,
        // so we need to set them as non-kinematic here
        if (!_sync.HasStateAuthority) _rigidbody.isKinematic = true;
    }

    private void OnDisable()
    {
        // Clean up the instance, this is useful for Logs because they are pooled
        isBeingCarried = false;
        _rigidbody.isKinematic = false;
        _collider.enabled = true;

        _sync.OnAuthorityRequested -= OnAuthorityRequested;
        _sync.OnStateAuthority.RemoveListener(OnStateAuthority);
        _sync.OnStateRemote.RemoveListener(OnStateRemote);
    }

    /// <summary>
    /// This method is called when another client requests authority. It will reject it if the grabbable
    /// has just been picked up by the local player. This avoids many race conditions,
    /// where another player would request authority in the same frame that the local player is picking
    /// the Grabbable up, leading to a the object being in a broken state.
    /// </summary>
    private bool OnAuthorityRequested(ClientID requesterid, AuthorityType authoritytype, CoherenceSync sync)
    {
        return !isBeingCarried;
    }

    private void FixedUpdate()
    {
        if (!_rigidbody.isKinematic)
        {
            float waterDiff = _rigidbody.position.y - _floatingY;
            if (waterDiff < 0)
            {
                _rigidbody.linearVelocity *= .9f;
                _rigidbody.angularVelocity *= .97f;
                _rigidbody.AddForce(Vector3.up * -waterDiff * _floatingForce, ForceMode.Acceleration);
            }
        }
    }

    /// <summary>
    /// Verifies that the object can be picked up. If the player attempting the action has authority, it succeeds instantly.
    /// If not, it requests it. Scripts need to listen to <see cref="PickupValidated"/> to get the asynchronous result.
    /// </summary>
    public void RequestPickup()
    {
        // Act as if a request was sent, and got denied
        if (isBeingCarried || !gameObject.activeSelf)
        {
            PickupValidated?.Invoke(false);
            return;
        }
        
        if (_sync.HasStateAuthority)
        {
            ConfirmPickup();
        }
        else
        {
            _pickupRequested = true;
            _sync.RequestAuthority(AuthorityType.Full);
            _sync.OnAuthorityRequestRejected.AddListener(OnRequestRejected);
        }
    }

    private void OnRequestRejected(AuthorityType arg0)
    {
        _sync.OnAuthorityRequestRejected.RemoveListener(OnRequestRejected);
        PickupValidated?.Invoke(false);
    }

    /// <summary>
    /// Object can become authoritative in different ways: when picked up,
    /// and when it bumps into a player that doesn't have authority.
    /// This callback is also called when the player sees a persistent orphan object,
    /// and automatically gets assigned authority over it.
    /// </summary>
    private void OnStateAuthority()
    {
        _lastAuthorityChangeTime = Time.time;
        if (_pickupRequested)
        {
            // Authority change happened as a result of a pick up action
            _sync.OnAuthorityRequestRejected.RemoveListener(OnRequestRejected);
            _pickupRequested = false;
            ConfirmPickup();
        }
        else if (_collisionHappened)
        {
            // Authority change happened because of a collision
            _collisionHappened = false;
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
            isBeingCarried = false;
        }
        else
        {
            // Authority was granted before connecting (offline gameplay), or when adopting a persistent orphan entity.
            if (isBeingCarried)
            {
                // An object that was being carried by a player was left in a dirty state on the Replication Server.
                // This can happen if the player disconnected abruptly. So we fix its base properties so it can be
                // interacted with again.
                isBeingCarried = false;
                _rigidbody.isKinematic = false;
            }
        }
    }

    /// <summary>
    /// Losing authority can happen because of two reasons: see <see cref="OnStateAuthority"/>.
    /// </summary>
    private void OnStateRemote()
    {
        _lastAuthorityChangeTime = Time.time;
        _rigidbody.isKinematic = true;
    }

    private bool CanChangeAuthority()
    {
        return Time.time > _lastAuthorityChangeTime + 1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_sync.HasStateAuthority
            || isBeingCarried
            || !CanChangeAuthority()) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // If the player colliding is local, take authority
            CoherenceSync collidingPlayersSync = collision.gameObject.GetComponent<CoherenceSync>();
            if (collidingPlayersSync.HasStateAuthority)
            {
                // If player is this client's player Prefab, take authority
                _collisionHappened = true;
                _sync.RequestAuthority(AuthorityType.Full);
            }
        }
    }

    private void ConfirmPickup()
    {
        isBeingCarried = true;
        PickupValidated?.Invoke(true);
    }

    public void Release()
    {
        isBeingCarried = false;
    }
}
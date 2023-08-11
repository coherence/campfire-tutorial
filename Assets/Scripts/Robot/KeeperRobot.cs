using System;
using System.Collections;
using Coherence;
using Coherence.Toolkit;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class KeeperRobot : MonoBehaviour
{
    public CoherenceSyncConfigRegistry configRegistry;
    public Animator animator;
    public SoundHandler soundHandler;
    public SFXPreset humLoop;
    public SFXPreset voices;
    public SFXPreset objectConjure;
    public SFXPreset objectAppear;
    public Transform holdSocket;

    public float situationCheckCooldown = 12f;

    private CoherenceSync _sync;
    private NavMeshAgent _navMeshAgent;
    public RobotState _state = RobotState.Sleeping;
    public enum RobotState
    {
        Idle, // Waiting to check the situation again shortly
        RecreatingObject, // Wait for the reappear shader animation to play out
        CarryingObject, // Carrying an object to its original position
        SeekingObject, // Going to pick up an object
        GoingToSleep, // Going back to its base
        Sleeping, // Robot is in the base, will check again
    }

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private ObjectAnchor[] _anchors;
    private int _anchorNumber;
    private Transform _targetObject;
    private Grabbable _targetObjectGrabbable;
    private Transform _targetAnchor;
    private Coroutine _adjustDestinationCoroutine;
    private Coroutine _recheckCoroutine;

    [SerializeField] private bool _log = true;

#if COHERENCE_SIMULATOR || UNITY_EDITOR
    private void Awake()
    {
        _sync = GetComponent<CoherenceSync>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        
        Transform chargeStation = GameObject.Find("ChargeStation").transform;
        _initialPosition = chargeStation.position;
        _initialRotation = chargeStation.rotation;
        
        _sync.CoherenceBridge.onLiveQuerySynced.AddListener(OnLiveQuerySynced);
    }

    private void OnLiveQuerySynced(CoherenceBridge bridge)
    {
        PlayHumSound();
        _sync.SendCommand<KeeperRobot>(nameof(PlayHumSound), MessageTarget.Other);

        if (_sync.HasStateAuthority)
        {
            if(_log) Debug.Log("OnLiveQuerySynced");
            
            // Begin acting
            _recheckCoroutine = StartCoroutine(WaitThenRecheck(situationCheckCooldown));
        }
    }

    private IEnumerator WaitThenRecheck(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        if(_log) Debug.Log("Checking object situation...");
        animator.SetBool("IsFlailing", true);
        
        PlayVoiceSound();
        _sync.SendCommand<KeeperRobot>(nameof(PlayVoiceSound), MessageTarget.Other);
        
        yield return new WaitForSeconds(1.2f);
        
        animator.SetBool("IsFlailing", false);
        CheckAnchors();
    }

    /// <summary>
    /// Cycles through all <see cref="ObjectAnchor"/> objects in the scene.
    /// </summary>
    private void CheckAnchors()
    {
        _anchors = FindObjectsByType<ObjectAnchor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        for (_anchorNumber = 0; _anchorNumber < _anchors.Length; _anchorNumber++)
        {
            bool needToAct = ActOnAnchor(_anchorNumber);
            if (needToAct) return;
        }
        
        SetState(RobotState.GoingToSleep);
    }

    /// <summary>
    /// Checks if an anchor needs to be acted upon (the object has been destroyed, or is out of place...)
    /// </summary>
    /// <returns>True if there is an action to perform, false if not.</returns>
    private bool ActOnAnchor(int anchorNumber)
    {
        ObjectAnchor anchor = _anchors[anchorNumber];
        _targetAnchor = anchor.transform;

        if (anchor.isObjectPresent)
        {
            // Object still exists
            _targetObject = anchor.GetLinkedObject().transform;
            _targetObjectGrabbable = _targetObject.GetComponent<Grabbable>();
            
            // Is it being carried by anyone?
            if (_targetObjectGrabbable.isBeingCarried) return false;
            
            if (Vector3.Distance(_targetAnchor.position, _targetObject.position) > .5f
                || Quaternion.Angle(_targetAnchor.rotation, _targetObject.rotation) > 15f)
            {
                // Object is out of place
                if(_log) Debug.Log($"{_targetObject.name} is not in its place.");
                SetState(RobotState.SeekingObject);
                return true;
            }
            else
            {
                // Object is in its correct place
                return false;
            }
        }
        else
        {
            // Object has been destroyed, need to recreate it
            _targetObject = RecreateObject(anchor);
            _targetObjectGrabbable = _targetObject.GetComponent<Grabbable>();
            if(_log) Debug.Log($"Recreating {_targetObject.name}.");
            
            // No need to request because the object has been just created,
            // so this Simulator is the Authority
            ConcludeObjectPickup();
            _targetObjectGrabbable.isBeingCarried = true;
            
            SetState(RobotState.RecreatingObject);

            return true;
        }
    }

    private void TryPickupObject()
    {
        // Pick up object
        _targetObjectGrabbable.PickupValidated += OnPickupResponse;
        _targetObjectGrabbable.RequestPickup();
    }

    private void OnPickupResponse(bool response)
    {
        _targetObjectGrabbable.PickupValidated -= OnPickupResponse;
        if (response)
        {
            ConcludeObjectPickup();
            SetState(RobotState.CarryingObject);
        }
        else
        {
            SetState(RobotState.Idle);
        }
    }

    private void ConcludeObjectPickup()
    {
        _targetObject.SetParent(holdSocket, true);
        _targetObject.SetLocalPositionAndRotation(Vector3.zero, quaternion.identity);
        _targetObject.GetComponent<Burnable>().checkCollisions = false;
        _targetObject.GetComponent<Collider>().enabled = false;
        _targetObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    private Transform RecreateObject(ObjectAnchor objAnchor)
    {
        foreach (CoherenceSyncConfig config in configRegistry)
        {
            if (config.ID == objAnchor.syncConfigId)
            {
                _sync.CoherenceBridge.UniquenessManager.RegisterUniqueId(objAnchor.holdingForUUID);
                CoherenceSync newSync = config.GetInstance(holdSocket.position, holdSocket.rotation);
                newSync.GetComponent<PositionedObject>().Restore(objAnchor);
                objAnchor.LinkedObjectReinstated();

                return newSync.transform;
            }
        }
        return null;
    }
    
    private void SetState(RobotState newState)
    {
        if(_adjustDestinationCoroutine != null) StopCoroutine(_adjustDestinationCoroutine);
        if(_recheckCoroutine != null) StopCoroutine(_recheckCoroutine);
        
        if(_log) Debug.Log($"New state: {newState}");

        switch (newState)
        {
            case RobotState.Idle:
                animator.SetBool("IsCarrying", false);
                _recheckCoroutine = StartCoroutine(WaitThenRecheck(4f));
                break;
            
            case RobotState.RecreatingObject:
                StartCoroutine(ObjectRecreationSequence());
                break;
            
            case RobotState.SeekingObject:
                MoveTowards(GetClosestPointOnNavmesh(_targetObject.position));
                _adjustDestinationCoroutine = StartCoroutine(AdjustDestinationIfNeeded());
                animator.SetBool("IsCarrying", false);
                break;
            
            case RobotState.CarryingObject:
                animator.SetBool("IsCarrying", true);
                MoveTowards(GetClosestPointOnNavmesh(_targetAnchor.position));
                break;
            
            case RobotState.GoingToSleep:
                MoveTowards(_initialPosition);
                break;
            
            case RobotState.Sleeping:
                _recheckCoroutine = StartCoroutine(WaitThenRecheck(situationCheckCooldown));
                break;
        }

        _state = newState;
    }

    private IEnumerator ObjectRecreationSequence()
    {
        _navMeshAgent.isStopped = true;
        animator.SetBool("IsCarrying", true);
        PlayConjure();
        _sync.SendCommand<KeeperRobot>(nameof(PlayConjure), MessageTarget.Other);
        
        yield return new WaitForSeconds(2.3f);
        
        PlayAppear();
        _sync.SendCommand<KeeperRobot>(nameof(PlayAppear), MessageTarget.Other);
        
        yield return new WaitForSeconds(0.5f);
        
        SetState(RobotState.CarryingObject);
    }

    private void Update()
    {
        switch (_state)
        {
            case RobotState.CarryingObject:
                if (CheckIfArrived())
                {
                    ReleaseObject();
                    SetState(RobotState.Idle);
                }
                break;
            
            case RobotState.SeekingObject:
                if (CheckIfArrived())
                {
                    if (_targetObject == null || _targetObjectGrabbable.isBeingCarried)
                    {
                        // Object has been burned in the meantime, or has been picked up again
                        // Try with some other object
                        if(_log) Debug.Log($"Object I was going for is missing. Search again.");
                        SetState(RobotState.Idle);
                    }
                    else
                    {
                        TryPickupObject();
                    }
                }
                break;
            
            case RobotState.GoingToSleep:
                if (CheckIfArrived())
                {
                    _navMeshAgent.updateRotation = false;
                    transform.rotation = _initialRotation;
                    SetState(RobotState.Sleeping);
                }
                break;
        }
    }

    private IEnumerator AdjustDestinationIfNeeded()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            // Has object been burned in the meantime?
            if (_targetObject == null) break;
            
            Vector3 possibleDestination = GetClosestPointOnNavmesh(_targetObject.position);
            if (Vector3.Distance(_navMeshAgent.destination, possibleDestination) > _navMeshAgent.stoppingDistance)
            {
                _navMeshAgent.destination = possibleDestination;
            }
        }
    }

    private void ReleaseObject()
    {
        if (_targetObject == null || _targetAnchor == null)
        {
            Debug.LogError("Why is carried object null?");
            Debug.Log($"targetObject: {_targetObject}");
            Debug.Log($"_targetAnchor: {_targetAnchor}");
            SetState(RobotState.Idle);
            return;
        }
        
        _targetObject.SetPositionAndRotation(_targetAnchor.position, _targetAnchor.rotation);
        _targetObject.SetParent(null, true);
        _targetObject.GetComponent<Grabbable>().Release();
        _targetObject.GetComponent<Burnable>().checkCollisions = true;
        _targetObject.GetComponent<Collider>().enabled = true;
    }

    /// <summary>
    /// We do all this because there are areas that the robot cannot reach, so the returned value defaults to something safe.
    /// </summary>
    private Vector3 GetClosestPointOnNavmesh(Vector3 desiredLocation)
    {
        Vector3 smallTweak = (transform.position - desiredLocation).normalized * .5f;
        if (NavMesh.SamplePosition(desiredLocation + smallTweak, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            return hit.position;
        else
            return transform.position; // Default to a reachable position
    }

    private void MoveTowards(Vector3 destination)
    {
        _navMeshAgent.updateRotation = true;
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(destination);
    }

    private bool CheckIfArrived()
    {
        if (_targetObject == null) return true;
        if (_navMeshAgent.isStopped) return false;

        float threshold = _navMeshAgent.stoppingDistance;
        if (Vector3.SqrMagnitude(transform.position - _navMeshAgent.destination) <= threshold * threshold)
        {
            if(_log) Debug.Log($"Arrived while in state {_state}");
            _navMeshAgent.isStopped = true;
            return true;
        }

        return false;
    }

    private void OnDisable()
    {
        if(_recheckCoroutine != null) StopCoroutine(_recheckCoroutine);
    }

    private void OnDestroy()
    {
        if(_targetObjectGrabbable != null)
            _targetObjectGrabbable.PickupValidated -= OnPickupResponse;
        
        if(_sync.CoherenceBridge != null)
            _sync.CoherenceBridge.onLiveQuerySynced.RemoveListener(OnLiveQuerySynced);
    }
#endif

    [Command] public void PlayHumSound() => soundHandler.Play(humLoop);
    [Command] public void PlayVoiceSound() => soundHandler.Play(voices);
    [Command] public void PlayConjure() => soundHandler.Play(objectConjure);
    [Command] public void PlayAppear() => soundHandler.Play(objectAppear);
}
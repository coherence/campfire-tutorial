using Coherence;
using Coherence.Toolkit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SitAction : MonoBehaviour, INetworkInteraction
{
    public CoherenceSync sync;
    public Animator animator;
    public SoundHandler soundHandler;
    public SFXPreset sitSFX;

    public event UnityAction<bool> Done;

    private PlayerInput _playerInput;
    private Move _move;
    private Chair _chair;
    private Transform _baseTransform;

    private float _lerpZ;
    private Vector3 _archOffset;

    private void Awake()
    {
        _playerInput = GetComponentInParent<PlayerInput>();
        _move = GetComponentInParent<Move>();
        _baseTransform = _move.gameObject.transform;
    }

    private void Update()
    {
        if (_chair == null) return;

        Vector3 targetPos = _chair.sittingAnchor.position;
        Vector3 targetLookdir = _chair.sittingAnchor.forward;

        _baseTransform.position = Vector3.Lerp(_baseTransform.position, targetPos + _archOffset, _lerpZ);
        _baseTransform.forward = Vector3.Slerp(_baseTransform.forward, targetLookdir, _lerpZ);
        _archOffset = Vector3.Lerp(_archOffset, Vector3.zero, _lerpZ);
        _lerpZ = Mathf.Lerp(_lerpZ, .333f, Time.deltaTime * 2f);
    }

    public void Sit(Chair chairComponent)
    {
        _chair = chairComponent;
        if (_chair.isBusy)
        {
            // Invalidate the action
            _chair = null;
            Done?.Invoke(true);
            return;
        }
        
        _chair.Occupy();
        
        _move.SetKinematic(true);
        animator.SetBool("IsSitting", true);
        _playerInput.jumpAction.action.performed += JumpInputPerformed;
        _archOffset = new Vector3(0f, 3f, 0f);
        _lerpZ = 0f;
        
        _baseTransform.SetParent(_chair.sittingAnchor.transform, true);

        PlaySitSound();
        sync.SendCommand<SitAction>(nameof(PlaySitSound), MessageTarget.Other);
    }
    
    [Command(defaultRouting = MessageTarget.Other)]
    public void PlaySitSound() => soundHandler.Play(sitSFX);

    private void JumpInputPerformed(InputAction.CallbackContext context) => JumpUp();

    public void JumpUp()
    {
        StandUp();
        _move.Jump();
    }
    
    public void StandUp()
    {
        _baseTransform.SetParent(null, true);
        
        _chair.Free();
        
        _chair = null;
        _playerInput.jumpAction.action.performed -= JumpInputPerformed;
        _move.SetKinematic(false);
        animator.SetBool("IsSitting", false);
        _move.ApplyThrowPushback(-1f);
        Done?.Invoke(true);
    }
    
    private void OnDisable()
    {
        // Necessary in case the game gets stopped in the editor while the player is sitting down.
        _playerInput.jumpAction.action.performed -= JumpInputPerformed;
    }
}
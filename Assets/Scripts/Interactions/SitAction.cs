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
        Debug.DrawRay(_chair.sittingAnchor.position, _chair.sittingAnchor.forward, Color.green);

        _baseTransform.position = Vector3.Lerp(_baseTransform.position, targetPos + _archOffset, _lerpZ);
        _baseTransform.forward = Vector3.Slerp(_baseTransform.forward, targetLookdir, _lerpZ);
        _archOffset = Vector3.Lerp(_archOffset, Vector3.zero, _lerpZ);
        _lerpZ = Mathf.Lerp(_lerpZ, .333f, .05f);
    }

    public void Sit(Chair chairComponent)
    {
        _chair = chairComponent;
        _move.SetKinematic(true);
        animator.SetBool("IsSitting", true);
        _playerInput.jumpAction.action.performed += JumpInputPerformed;
        _archOffset = new Vector3(0, 3, 0);
        _lerpZ = 0;

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
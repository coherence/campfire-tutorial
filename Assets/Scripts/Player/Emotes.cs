using UnityEngine;
using UnityEngine.InputSystem;
using Coherence;
using Coherence.Toolkit;

public class Emotes : MonoBehaviour
{
    public InputActionReference danceAction;
    public InputActionReference waveAction;
    public InputActionReference yesAction;
    public InputActionReference noAction;

    public Animator animator;
    public CoherenceSync sync;
    public SoundHandler soundHandler;
    public SFXPreset noSFX;
    public SFXPreset yesSFX;
    public SFXPreset heySFX;

    private void OnEnable()
    {
        danceAction.asset.Enable();
        waveAction.asset.Enable();
        yesAction.asset.Enable();
        noAction.asset.Enable();

        danceAction.action.performed += OnDancePerformed;
        waveAction.action.performed += OnWavePerformed;
        yesAction.action.performed += OnYesPerformed;
        noAction.action.performed += OnNoPerformed;
    }

    private void OnDisable()
    {
        danceAction.action.performed -= OnDancePerformed;
        waveAction.action.performed -= OnWavePerformed;
        yesAction.action.performed -= OnYesPerformed;
        noAction.action.performed -= OnNoPerformed;
    }

    private void OnDancePerformed(InputAction.CallbackContext obj)
    {
        animator.SetTrigger("Dance");
        sync.SendCommand<Animator>(nameof(Animator.SetTrigger), MessageTarget.Other, "Dance");
    }

    private void OnWavePerformed(InputAction.CallbackContext obj)
    {
        animator.SetTrigger("Wave");
        sync.SendCommand<Animator>(nameof(Animator.SetTrigger), MessageTarget.Other, "Wave");
        PlayHeySound();
        sync.SendCommand<Emotes>(nameof(PlayHeySound), MessageTarget.Other);
    }

    private void OnYesPerformed(InputAction.CallbackContext obj)
    {
        animator.SetTrigger("Yes");
        sync.SendCommand<Animator>(nameof(Animator.SetTrigger), MessageTarget.Other, "Yes");
        PlayYesSound();
        sync.SendCommand<Emotes>(nameof(PlayYesSound), MessageTarget.Other);
    }

    private void OnNoPerformed(InputAction.CallbackContext obj)
    {
        animator.SetTrigger("No");
        sync.SendCommand<Animator>(nameof(Animator.SetTrigger), MessageTarget.Other, "No");
        PlayNoSound();
        sync.SendCommand<Emotes>(nameof(PlayNoSound), MessageTarget.Other);
    }

    [Command] public void PlayYesSound() => soundHandler.Play(yesSFX);
    [Command] public void PlayNoSound() => soundHandler.Play(noSFX);
    [Command] public void PlayHeySound() => soundHandler.Play(heySFX);
}
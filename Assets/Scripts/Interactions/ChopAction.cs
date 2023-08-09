using System.Collections;
using Coherence;
using Coherence.Toolkit;
using UnityEngine;
using UnityEngine.Events;

public class ChopAction : MonoBehaviour, INetworkInteraction
{
    public CoherenceSync sync;
    public Animator animator;
    public SoundHandler soundHandler;
    public SFXPreset chopSFX;
    public float chopTime; // Time in seconds at which the chop connects to the tree

    public event UnityAction<bool> Done;

    public void Chop(ChoppableTree tree)
    {
        StartCoroutine(DoneChopping(tree));
    }

    private IEnumerator DoneChopping(ChoppableTree tree)
    {
        if (tree.energy <= 0)
        {
            Done?.Invoke(true);
            yield break;
        }
        
        animator.SetTrigger("Chop");
        sync.SendCommand<Animator>(nameof(Animator.SetTrigger), MessageTarget.Other, "Chop");

        yield return new WaitForSeconds(chopTime);

        // This unlocks the player's interaction, to be able to perform a new chop.
        // It also clears the interaction target if the tree was completely chopped,
        // allowing to highlight and pick up the newly-spawned log straight away.
        Done?.Invoke(tree.energy == 0);

        tree.TryChop();
        
        PlayChopSound();
        sync.SendCommand<ChopAction>(nameof(PlayChopSound), MessageTarget.Other);
    }

    [Command(defaultRouting = MessageTarget.Other)]
    public void PlayChopSound()
    {
        soundHandler.Play(chopSFX);
    }
}
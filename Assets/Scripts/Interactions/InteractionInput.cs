using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionInput : MonoBehaviour
{
    public InputActionReference interactAction;
    public Move moveScript;
    public InteractionState _interactionState;

    [Header("Highlight material")] public Material highlightMaterial;
    public Shader regularHighlight, treeHighlight;

    public enum InteractionState
    {
        Free,
        Carrying,
        Sitting,
        Chopping,
        AwaitingResponse
    }

    private Interactable _interactionTarget; // The Interactable currently in focus
    private Burnable _targetBurnable;
    private GrabAction _grabAction;
    private SitAction _sitAction;
    private ChopAction _chopAction;
    private bool _canToss;

    private void Awake()
    {
        _chopAction = GetComponent<ChopAction>();
        _sitAction = GetComponent<SitAction>();
        _grabAction = GetComponent<GrabAction>();
    }

    private void OnEnable()
    {
        interactAction.asset.Enable();
        interactAction.action.performed += OnInteractionPerformed;
        _chopAction.Done += OnDone;
        _sitAction.Done += OnDone;
        _grabAction.Done += OnDone;
        _grabAction.PickedUpObject += OnObjectPickedUp;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteractionPerformed;
        _sitAction.Done -= OnDone;
        _chopAction.Done -= OnDone;
        _grabAction.Done -= OnDone;
        _grabAction.PickedUpObject -= OnObjectPickedUp;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_interactionState != InteractionState.Free || !other.gameObject.activeInHierarchy)
            return;

        // Found an interactable object
        if (other.CompareTag("Interactable"))
        {
            if (_interactionTarget != null)
            {
                if(_interactionTarget == other.GetComponent<Interactable>()) return;
                
                // Clean up previous one                
                _interactionTarget.RemoveHighlight();
                if (_interactionTarget.mainObject.TryGetComponent(out _targetBurnable))
                    _targetBurnable.Burned -= OnInteractionTargetBurned;   
            }

                // New interactable
            _interactionTarget = other.GetComponent<Interactable>();
            _interactionTarget.Highlight();
            UpdateHighlightMaterial(_interactionTarget.Style);
            
            if (_interactionTarget.mainObject.TryGetComponent(out _targetBurnable))
                _targetBurnable.Burned += OnInteractionTargetBurned;
        }
    }

    private void UpdateHighlightMaterial(Interactable.HighlightStyle style)
    {
        highlightMaterial.shader = style == Interactable.HighlightStyle.Regular ? regularHighlight : treeHighlight;
        highlightMaterial.SetFloat("_Thickness", style == Interactable.HighlightStyle.Regular ? .4f : .8f);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_interactionState != InteractionState.Free
            || _interactionTarget == null)
            return;

        if (other.GetComponent<Interactable>() == _interactionTarget)
        {
            _interactionTarget.RemoveHighlight();
            if (_interactionTarget.mainObject.TryGetComponent(out _targetBurnable))
                _targetBurnable.Burned -= OnInteractionTargetBurned;
            _interactionTarget = null;
        }
    }

    /// <summary>
    /// Fires when the button is pressed.
    /// </summary>
    private void OnInteractionPerformed(InputAction.CallbackContext obj)
    {
        switch (_interactionState)
        {
            case InteractionState.Free:
                if (_interactionTarget != null)
                {
                    if (_interactionTarget.mainObject.TryGetComponent(out Grabbable grabbableComponent))
                    {
                        if (!grabbableComponent.isBeingCarried)
                        {
                            // Pick up
                            _interactionState = InteractionState.AwaitingResponse;
                            _interactionTarget.RemoveHighlight();
                            _targetBurnable.Burned -= OnInteractionTargetBurned;
                            _grabAction.RequestPickup(grabbableComponent);
                        }
                    }
                    else if (_interactionTarget.mainObject.TryGetComponent(out ChoppableTree treeComponent))
                    {
                        // Chop
                        _interactionState = InteractionState.Chopping;
                        _chopAction.Chop(treeComponent);
                    }
                    else if (_interactionTarget.mainObject.TryGetComponent(out Chair chairComponent))
                    {
                        // Sit down
                        _interactionTarget.RemoveHighlight();
                        _interactionState = InteractionState.Sitting;
                        _sitAction.Sit(chairComponent);
                    }
                }

                break;

            case InteractionState.Carrying:
                // Release or throw
                float speed = moveScript.ThrowSpeed();
                _grabAction.Drop(speed);
                moveScript.ApplyThrowPushback(speed * .8f);
                break;

            case InteractionState.Chopping:
                break;

            case InteractionState.Sitting:
                _sitAction.StandUp();
                break;
        }
    }

    private void OnObjectPickedUp()
    {
        _interactionState = InteractionState.Carrying;
    }

    private void OnInteractionTargetBurned()
    {
        _targetBurnable.Burned -= OnInteractionTargetBurned;
        _interactionTarget.RemoveHighlight();
        _interactionTarget = null;
        _targetBurnable = null;
    }

    /// <summary>
    /// This marks the end of an action, releasing whatever target of that action it was
    /// and opening up for a new input from the player.
    /// However, chopping doesn't "release" the target in order to keep the tree highlighted
    /// and ready to be chopped again.
    /// </summary>
    private void OnDone(bool clearTarget = false)
    {
        _interactionState = InteractionState.Free;
        if (clearTarget)
        {
            _interactionTarget.RemoveHighlight();
            _interactionTarget = null;
        }
        else
        {
            if (_interactionTarget.mainObject.TryGetComponent(out _targetBurnable))
                _targetBurnable.Burned += OnInteractionTargetBurned;
        }
    }

    private void OnDestroy()
    {
        // This will restore the Material asset to its original state,
        // so it's not changed after Play Mode (this is really only relevant in the Unity Editor)
        if(enabled) UpdateHighlightMaterial(Interactable.HighlightStyle.Regular);
    }
}
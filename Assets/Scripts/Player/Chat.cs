using System.Text;
using Coherence;
using Coherence.Toolkit;
using UnityEngine;
using UnityEngine.InputSystem;

public class Chat : MonoBehaviour
{
    public InputActionAsset inputAsset;
    public InputActionReference bringUpTextAction;
    public InputActionReference sendTextAction;
    public InputActionReference cancelSendTextAction;
    public CoherenceSync sync;
    public ChatVisualiserUI chatVisualiser;
    public SoundHandler soundHandler;
    public SFXPreset chatSFX;

    private ChatComposerUI _chatComposer;

    private void Awake()
    {
        _chatComposer = FindFirstObjectByType<ChatComposerUI>();
    }

    private void OnEnable()
    {
        bringUpTextAction.action.canceled += DisplayComposer;
        sendTextAction.action.canceled += SendText;
        cancelSendTextAction.action.canceled += CancelSending;
        
        if(sync.HasStateAuthority) bringUpTextAction.action.Enable();
    }

    private void DisplayComposer(InputAction.CallbackContext _)
    {
        if (sendTextAction.action.WasReleasedThisFrame()) return;
        
        inputAsset.FindActionMap("Gameplay").Disable();
        inputAsset.FindActionMap("Chat").Enable();
        _chatComposer.Display();
    }

    private void SendText(InputAction.CallbackContext _)
    {
        string message = _chatComposer.GetText();
        if (message != string.Empty)
        {
            byte[] encodedMessage = Encoding.UTF8.GetBytes(message);
            SendChatMessage(encodedMessage);
            sync.SendCommand<Chat>(nameof(SendChatMessage), MessageTarget.Other, encodedMessage);
        }
        HideComposer();
    }

    [Command(defaultRouting = MessageTarget.Other)]
    public void SendChatMessage(byte[] encodedMessage)
    {
        string decodedMessage = Encoding.UTF8.GetString(encodedMessage);
        chatVisualiser.ShowText(decodedMessage);
        soundHandler.Play(chatSFX);
    }

    private void CancelSending(InputAction.CallbackContext _) => HideComposer();

    private void HideComposer()
    {
        if (bringUpTextAction.action.WasReleasedThisFrame()) return;
        
        inputAsset.FindActionMap("Chat").Disable();
        inputAsset.FindActionMap("Gameplay").Enable();
        _chatComposer.Hide();
    }

    private void OnDisable()
    {
        bringUpTextAction.action.canceled -= DisplayComposer;
        sendTextAction.action.canceled -= SendText;
        cancelSendTextAction.action.canceled -= CancelSending;
    }
}
using System;
using TMPro;
using UnityEngine;

public class ChatComposerUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject chatUI;

    public string GetText() => inputField.text;

    private void Start()
    {
        chatUI.SetActive(false);
    }

    public void Display()
    {
        chatUI.SetActive(true);
        inputField.Select();
        inputField.ActivateInputField();
    }

    public void Hide()
    {
        chatUI.SetActive(false);
        inputField.text = string.Empty;
        inputField.textComponent.text = string.Empty;
        inputField.ReleaseSelection();
    }
}

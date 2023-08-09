using System.Collections;
using TMPro;
using UnityEngine;

public class ChatVisualiserUI : MonoBehaviour
{
    public GameObject chatBubble;
    public TextMeshProUGUI chatTextField;

    private Coroutine _hideCoroutine;

    private void Start()
    {
        chatBubble.SetActive(false);
    }

    public void ShowText(string chatMessage)
    {
        if(_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        
        chatBubble.SetActive(true);
        chatTextField.text = chatMessage;
        _hideCoroutine = StartCoroutine(Hide(chatMessage.Length));
    }

    private IEnumerator Hide(int textLength)
    {
        float duration = 4f + (.05f * textLength);
        
        yield return new WaitForSeconds(duration);
        
        chatBubble.SetActive(false);
    }
}
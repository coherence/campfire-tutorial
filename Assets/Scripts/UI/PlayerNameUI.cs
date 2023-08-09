using TMPro;
using UnityEngine;

/// <summary>
/// Retrieves the player name from a static field on the <see cref="PlayerNameFetcher"/> script.
/// </summary>
public class PlayerNameUI : MonoBehaviour
{
    public TextMeshProUGUI textField;

    private void Start()
    {
        textField.text = PlayerNameFetcher.PLAYER_NAME;
    }
}
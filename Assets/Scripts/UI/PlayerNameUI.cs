using Coherence.Toolkit;
using TMPro;
using UnityEngine;

/// <summary>
/// Retrieves the player name from a static field on the <see cref="PlayerNameFetcher"/> script,
/// and writes it into the networked <see cref="textField"/> so it syncs to other clients.
/// Only the locally-owned player should do this: remote players' <see cref="textField"/> is
/// driven by the incoming coherence sync value and must not be overwritten here.
/// </summary>
public class PlayerNameUI : MonoBehaviour
{
    public TextMeshProUGUI textField;

    private CoherenceSync _coherenceSync;

    private void Awake()
    {
        _coherenceSync = GetComponentInParent<CoherenceSync>();
    }

    private void Start()
    {
        if (_coherenceSync != null && !_coherenceSync.HasStateAuthority)
        {
            return;
        }

        textField.text = PlayerNameFetcher.PLAYER_NAME;
    }
}
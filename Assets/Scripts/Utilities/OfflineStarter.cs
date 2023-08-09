using UnityEngine;

public class OfflineStarter : MonoBehaviour
{
#if !COHERENCE_SIMULATOR
    private void Start()
    {
        FindObjectOfType<PlayerHandler>().SpawnPlayer();
    }
#endif
}
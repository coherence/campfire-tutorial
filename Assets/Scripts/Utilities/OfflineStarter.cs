using UnityEngine;

public class OfflineStarter : MonoBehaviour
{
#if !COHERENCE_SIMULATOR
    private void Start()
    {
        FindFirstObjectByType<PlayerHandler>().SpawnPlayer();
    }
#endif
}
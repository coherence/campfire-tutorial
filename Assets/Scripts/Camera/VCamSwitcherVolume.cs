using Cinemachine;
using Coherence.Toolkit;
using UnityEngine;

public class VCamSwitcherVolume : MonoBehaviour
{
    public CinemachineVirtualCamera vCam;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Only if it's the local player
            if(IsLocalPlayer(other.gameObject)) vCam.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Only if it's the local player
            if(IsLocalPlayer(other.gameObject)) vCam.gameObject.SetActive(false);
        }
    }

    private bool IsLocalPlayer(GameObject player) => player.GetComponent<CoherenceSync>().HasStateAuthority;
}

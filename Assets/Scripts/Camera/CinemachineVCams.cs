using System;
using Cinemachine;
using UnityEngine;

public class CinemachineVCams : MonoBehaviour
{
    public CinemachineVirtualCamera gameplayVCam;
    public CinemachineVirtualCamera initialShotVCam;
    public CinemachineVirtualCamera campfireVCam;
    public CinemachineVirtualCamera beachClubVCam;

    public void PlayerSpawned(Transform playerTransform)
    {
        gameplayVCam.Follow = playerTransform;
        gameplayVCam.gameObject.SetActive(true);
        initialShotVCam.gameObject.SetActive(false);
    }

    public void PlayerDespawned()
    {
        beachClubVCam.gameObject.SetActive(false);
        campfireVCam.gameObject.SetActive(false);
        gameplayVCam.gameObject.SetActive(false);
        initialShotVCam.gameObject.SetActive(true);
    }
}
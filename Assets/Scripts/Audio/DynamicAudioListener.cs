using UnityEngine;

public class DynamicAudioListener : MonoBehaviour
{
    private Transform _cameraTransform;
    private Transform _playerTransform;
    private Camera _camera;
    private AudioListener _cameraListener;
    private bool _hasPlayer;

    private void Awake()
    {
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        _cameraListener = _camera.GetComponent<AudioListener>();
    }

    public void Initialise(Transform player)
    {
        _cameraListener.enabled = false;
        _playerTransform = player;
        _hasPlayer = true;
        LateUpdate(); // Force it once to get immediate syncing
    }

    private void LateUpdate()
    {
        if (_hasPlayer)
        {
            transform.position = _playerTransform.position + Vector3.up * .5f;
            transform.rotation = Quaternion.LookRotation(_cameraTransform.forward, Vector3.up);
        }
    }

    public void RestoreDefaultState()
    {
        _cameraListener.enabled = true;
    }
}
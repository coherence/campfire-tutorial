using Coherence.Connection;
using Coherence.Toolkit;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerHandler : MonoBehaviour
{
    public float spawnRadius = 1f;
    public GameObject playerPrefab;
    public DynamicAudioListener audioListenerPrefab;

    private GameObject _player;
    private DynamicAudioListener _dynamicAudioListener;
    private CinemachineVCams _vCams;
    private CoherenceBridge _coherenceBridge;

#if !COHERENCE_SIMULATOR

    private void Awake()
    {
        _coherenceBridge = FindFirstObjectByType<CoherenceBridge>();
        _coherenceBridge.onConnected.AddListener(OnBridgeConnection);
        _coherenceBridge.onDisconnected.AddListener(OnBridgeDisconnection);

        _vCams = FindFirstObjectByType<CinemachineVCams>();
    }

    private void OnBridgeConnection(CoherenceBridge arg0)
    {
        SpawnPlayer();
    }

    private void OnBridgeDisconnection(CoherenceBridge arg0, ConnectionCloseReason arg1)
    {
        DespawnPlayer();
    }

    public void SpawnPlayer()
    {
        Vector3 initialPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        initialPosition.y = transform.position.y;

        _player = Instantiate(playerPrefab, initialPosition, Quaternion.identity);
        _player.name = "[local] Player";

        _vCams.PlayerSpawned(_player.transform);

        _dynamicAudioListener = Instantiate(audioListenerPrefab, _player.transform.position, transform.rotation);
        _dynamicAudioListener.Initialise(_player.transform);
    }

    public void DespawnPlayer()
    {
        Destroy(_player);
        _dynamicAudioListener.RestoreDefaultState();
        Destroy(_dynamicAudioListener.gameObject);
        _vCams.PlayerDespawned();
    }

    private void OnDestroy()
    {
        _coherenceBridge.onConnected.AddListener(OnBridgeConnection);
        _coherenceBridge.onDisconnected.AddListener(OnBridgeDisconnection);
    }

#endif
}
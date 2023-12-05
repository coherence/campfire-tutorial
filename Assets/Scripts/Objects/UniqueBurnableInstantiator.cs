using System;
using Coherence.Toolkit;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Custom instantiator for <see cref="PositionedObject"/>. It is used for all those unique objects that have been pre-placed in the scene,
/// like the banjo or the radio. When they get burned on the fire and <see cref="Burnable"/> invokes <see cref="CoherenceSync.ReleaseInstance"/>,
/// they get destroyed instead of disabled (as it would happen with the normal instantiator).
/// </summary>
[Serializable, DisplayName("UniqueBurnableObjects", "Custom instantiator for unique objects that are pre-placed in the scene.")]
public class UniqueBurnableInstantiator : INetworkObjectInstantiator
{
    public void OnUniqueObjectReplaced(ICoherenceSync instance) { }
    
    public ICoherenceSync Instantiate(SpawnInfo spawnInfo)
    {
        return Object.Instantiate((CoherenceSync)spawnInfo.prefab, spawnInfo.position, (Quaternion)spawnInfo.rotation);
    }
    
    public void Destroy(ICoherenceSync obj)
    {
        var monoBehaviour = obj as MonoBehaviour;
        Object.Destroy(monoBehaviour.gameObject);
    }
    
    public void WarmUpInstantiator(CoherenceBridge bridge, CoherenceSyncConfig config, INetworkObjectProvider assetLoader) { }
    
    public void OnApplicationQuit() { }
}

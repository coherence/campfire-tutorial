using UnityEngine.Events;

public interface INetworkInteraction
{
    public event UnityAction<bool> Done;
}
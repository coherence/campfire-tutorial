using UnityEngine;

public class BouncyObject : MonoBehaviour
{
    public SoundHandler soundHandler;
    public SFXPreset bounceSFX;
        
    private Vector3 _startingPosition;
    private Vector3 _velocity;

    private void Start()
    {
        _startingPosition = transform.position;
        _velocity = Vector3.zero;
    }

    private void Update()
    {
        Vector3 pos = transform.position;
        _velocity = Vector3.Lerp(_velocity, (_startingPosition - pos) * 8f, .175f);
        pos += _velocity * Time.deltaTime;
        transform.position = pos;
    }

    public void GetJumpedOn()
    {
        // TODO: Network this sound
        soundHandler.Play(bounceSFX);
        _velocity = new Vector3(0, -15f, 0);
    }
}
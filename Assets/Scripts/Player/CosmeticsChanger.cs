using Coherence.Toolkit;
using UnityEngine;
using Random = UnityEngine.Random;

public class CosmeticsChanger : MonoBehaviour
{
    [Header("Synchronised values")]
    //-2 is an invalid value, used to mark a cosmetic as not initialised yet
    [Sync, OnValueSynced(nameof(OnBodyChanged))] public int currentBody = -2;
    [Sync, OnValueSynced(nameof(OnSkinToneChanged))] public int currentSkinTone = -2;
    [Sync, OnValueSynced(nameof(OnClothesChanged))] public int currentClothes = -2;
    [Sync, OnValueSynced(nameof(OnHairStyleChanged))] public int currentHairstyle = -2;
    [Sync, OnValueSynced(nameof(OnBackpackChanged))] public int currentBackpack = -2;

    [Header("Cosmetic elements")]
    public Mesh[] bodies;
    public GameObject[] hairstyles;
    public GameObject[] backpacks;
    public Texture[] skinTones;
    public Texture[] clothes;

    [Header("References")]
    public SkinnedMeshRenderer bodyRenderer;
    public ParticleSystem sparks;
    public SoundHandler soundHandler;
    public SFXPreset cosmeticsChangeSFX;

    private int GetRandomBody()
    {
        return Random.Range(0, bodies.Length);
    }

    private int GetRandomSkinTone()
    {
        return Random.Range(0, skinTones.Length);
    }

    private int GetRandomClothes()
    {
        return Random.Range(0, clothes.Length);
    }

    private int GetRandomBackpack()
    {
        return Random.Range(-1, backpacks.Length);
    }

    private int GetRandomHairstyle()
    {
        return Random.Range(-1, hairstyles.Length);
    }

    private int GetNextBodyType()
    {
        return ++currentBody % bodies.Length;
    }

    private int GetNextSkinTone()
    {
        return ++currentSkinTone % skinTones.Length;
    }

    private int GetNextClothes()
    {
        return ++currentClothes % clothes.Length;
    }

    private int GetNextBackpack()
    {
        return ++currentBackpack % backpacks.Length;
    }

    private int GetNextHairstyle()
    {
        return ++currentHairstyle % hairstyles.Length;
    }

    private void Awake()
    {
        // Reset any item that might have been left on at authoring time
        foreach (GameObject item in hairstyles) item.SetActive(false);
        foreach (GameObject item in backpacks) item.SetActive(false);
    }

    /// <summary>
    /// Changes all cosmetics at once. This is only called on characters that the local Client has authority on, via <see cref="CosmeticsInput"/>.
    /// </summary>
    public void ChangeAllCosmetics()
    {
        OnBodyChanged(currentBody, GetRandomBody());
        OnHairStyleChanged(currentHairstyle, GetRandomHairstyle());
        OnBackpackChanged(currentBackpack, GetRandomBackpack());
        OnSkinToneChanged(currentSkinTone, GetRandomSkinTone());
        OnClothesChanged(currentClothes, GetRandomClothes());
    }

    public void ChangeToNextBodyType()
    {
        OnBodyChanged(currentBody, GetNextBodyType());
    }

    public void ChangeToNextSkinTone()
    {
        OnSkinToneChanged(currentSkinTone, GetNextSkinTone());
    }

    public void ChangeToNextClothes()
    {
        OnClothesChanged(currentClothes, GetNextClothes());
    }

    public void ChangeToNextHairstyle()
    {
        OnHairStyleChanged(currentHairstyle, GetNextHairstyle());
    }

    private void PlayEffects()
    {
        sparks.Play();
        soundHandler.Play(cosmeticsChangeSFX);
    }

    public void OnBodyChanged(int oldValue, int newValue)
    {
        if (newValue != oldValue)
        {
            bodyRenderer.sharedMesh = bodies[newValue];
            currentBody = newValue;

            if (oldValue != -2) PlayEffects();
        }
    }

    public void OnHairStyleChanged(int oldValue, int newValue)
    {
        if (newValue != oldValue)
        {
            if (oldValue > -1) hairstyles[oldValue].SetActive(false);
            if (newValue > -1) hairstyles[newValue].SetActive(true);
            currentHairstyle = newValue;

            if (oldValue != -2) PlayEffects();
        }
    }

    public void OnBackpackChanged(int oldValue, int newValue)
    {
        if (newValue != oldValue)
        {
            if (oldValue > -1) backpacks[oldValue].SetActive(false);
            if (newValue > -1) backpacks[newValue].SetActive(true);
            currentBackpack = newValue;

            if (oldValue != -2) PlayEffects();
        }
    }

    public void OnSkinToneChanged(int oldValue, int newValue)
    {
        if (newValue != oldValue)
        {
            bodyRenderer.material.SetTexture("_BaseMap", skinTones[newValue]);
            currentSkinTone = newValue;

            if (oldValue != -2) PlayEffects();
        }
    }

    public void OnClothesChanged(int oldValue, int newValue)
    {
        if (newValue != oldValue)
        {
            bodyRenderer.material.SetTexture("_ClothesMap", clothes[newValue]);
            currentClothes = newValue;

            if (oldValue != -2) PlayEffects();
        }
    }
}
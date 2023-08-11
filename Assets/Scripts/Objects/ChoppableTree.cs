using System.Collections;
using Coherence;
using Coherence.Toolkit;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChoppableTree : MonoBehaviour
{
    [Sync] public int energy = 3;
    
    public float timeToGrowBack = 5f;
    public SFXPreset choppedDownSFX;
    public SFXPreset springUpSFX;

    [Header("Spawn")]
    public CoherenceSyncConfig logSyncConfig;

    [Header("Internal references")] 
    public CoherenceSync sync;
    public GameObject tree;
    public GameObject stump;
    public ParticleSystem chopEffect;
    public ParticleSystem cutDownEffect;
    public SoundHandler soundHandler;

    private int _fullEnergy;
    private float _defaultY = 0f;
    private float _undergroundY = -8f;

    private void Awake()
    {
        _fullEnergy = energy;

        sync.OnStateAuthority.AddListener(OnStateAuthority);
        sync.OnStateRemote.AddListener(OnStateRemote);
        
        sync.CoherenceBridge.onLiveQuerySynced.AddListener(OnLiveQuerySynced);
    }

    private void OnLiveQuerySynced(CoherenceBridge bridge)
    {
        if (!sync.HasStateAuthority)
        {
            // Catch up with a tree that was cut-down on the network
            if (energy <= 0)
                FastForwardToChopped();
        }
    }
    
    private void OnStateAuthority()
    {
        if (energy == 0)
            StartCoroutine(GrowBack());
    }

    private void OnStateRemote()
    {
        StopCoroutine(GrowBack());
    }

    /// <summary>
    /// If this player is the authority on the tree it will chop it,
    /// otherwise it will request the authority Client to do it.
    /// </summary>
    public void TryChop()
    {
        if (energy <= 0) return;

        if (sync.HasStateAuthority)
            Chop();
        else
        {
            energy--;
            sync.SendCommand<ChoppableTree>(nameof(Chop), MessageTarget.AuthorityOnly);
        }
    }

    [Command(defaultRouting = MessageTarget.AuthorityOnly)]
    public void Chop()
    {
        if (energy <= 0) return;
        
        energy--;
        if (energy <= 0)
            CutDown();
        else
        {
            PlayChopEffect();
            sync.SendCommand<ChoppableTree>(nameof(PlayChopEffect), MessageTarget.Other);
        }
    }

    [Command(defaultRouting = MessageTarget.Other)]
    public void PlayChopEffect()
    {
        chopEffect.Play();
    }

    /// <summary>
    /// The tree reached zero energy, so it disappears into the ground, and spawns a Log object.
    /// </summary>
    private void CutDown()
    {
        // ChangeState plays the needed animations on each Client
        ChangeState(false);
        sync.SendCommand<ChoppableTree>(nameof(ChangeState), MessageTarget.Other, false);

        StartCoroutine(GenerateNewLog());
        StartCoroutine(GrowBack());
    }

    /// <summary>
    /// Generates a new Log object, using coherence's built-in object pooling.
    /// </summary>
    private IEnumerator GenerateNewLog()
    {
        yield return new WaitForSeconds(.2f);

        // Quick and dirty way to cap the amount of logs in the scene.
        // If there's too many Burnables active, we just don't spawn one.
        Burnable[] burnables = (Burnable[])FindObjectsByType(typeof(Burnable), FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (burnables.Length > 50) yield break;

        // Get a new log from the pool
        Vector3 randomRotation =
                new(0f, Random.Range(0f, 360f),
                    70f); // Random Y, and a Z that is not exactly 90 so the log falls sideways
        CoherenceSync newLog =
            logSyncConfig.GetInstance(transform.position + Vector3.up * 2f, Quaternion.Euler(randomRotation));
    }

    /// <summary>
    /// Changes the appearance of the tree between its two states: up, and cut down.
    /// </summary>
    [Command(defaultRouting = MessageTarget.Other)]
    public void ChangeState(bool toUp)
    {
        stump.SetActive(!toUp);
        StartCoroutine(toUp ? SpringUpAnimation() : CutDownAnimation());
    }

    private void FastForwardToChopped()
    {
        stump.SetActive(true);
        tree.transform.localPosition = new Vector3(0f, _undergroundY, 0f);
    }

    private IEnumerator SpringUpAnimation()
    {
        soundHandler.Play(springUpSFX);
        
        DurationTimer springUpTimer = new(.7f);
        tree.transform.localPosition = new Vector3(0f, _undergroundY, 0f);

        while (true)
        {
            springUpTimer.Tick();
            float treeY = Mathf.Lerp(_undergroundY, _defaultY, Easing.Elastic.Out(springUpTimer.GetRatio()));
            tree.transform.localPosition = new Vector3(0f, treeY, 0f);

            yield return new WaitForEndOfFrame();
            if (springUpTimer.HasElapsed()) break;
        }
    }

    private IEnumerator CutDownAnimation()
    {
        cutDownEffect.Play();
        soundHandler.Play(choppedDownSFX);
        
        DurationTimer chopDownTimer = new(.3f);
        tree.transform.localPosition = new Vector3(0f, _defaultY, 0f);

        while (true)
        {
            chopDownTimer.Tick();
            float treeY = Mathf.Lerp(_defaultY, _undergroundY, Easing.Back.In(chopDownTimer.GetRatio()));
            tree.transform.localPosition = new Vector3(0f, treeY, 0f);

            yield return new WaitForEndOfFrame();
            if (chopDownTimer.HasElapsed()) break;
        }
    }

    /// <summary>
    /// Invoked some time after the tree has been <see cref="CutDown"/>, only on the authority.
    /// Grows it back, and restores its functionality. Notifies non-authority clients.
    /// </summary>
    private IEnumerator GrowBack()
    {
        float actualTimeToGrowBack = Mathf.Max(1f, timeToGrowBack + Random.Range(-1f, 2f));

        yield return new WaitForSeconds(actualTimeToGrowBack);

        energy = _fullEnergy;

        ChangeState(true);
        sync.SendCommand<ChoppableTree>(nameof(ChangeState), MessageTarget.Other, true);
    }
    
    private void OnDestroy()
    {
        if(sync.CoherenceBridge != null)
            sync.CoherenceBridge.onLiveQuerySynced.RemoveListener(OnLiveQuerySynced);
        
        sync.OnStateAuthority.RemoveListener(OnStateAuthority);
        sync.OnStateRemote.RemoveListener(OnStateRemote);
    }
}
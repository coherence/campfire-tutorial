using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Changes cosmetics when a button is pressed. Disabled on remote characters.
/// </summary>
public class CosmeticsInput : MonoBehaviour
{
    public InputActionReference randomiseCosmeticsAction;
    public InputActionReference cycleSkinToneAction;
    public InputActionReference cycleBodyTypeAction;
    public InputActionReference cycleHairstyleAction;
    public InputActionReference cycleClothesAction;

    private CosmeticsChanger _cosmetics;

    private void Awake()
    {
        _cosmetics = GetComponent<CosmeticsChanger>();
    }

    private void Start()
    {
        RandomiseCosmetics();
    }

    private void OnEnable()
    {
        randomiseCosmeticsAction.asset.Enable();
        randomiseCosmeticsAction.action.performed += OnCosmeticsActionPerformed;

        //cycleSkinColourAction.action.Enable();
        cycleSkinToneAction.action.performed += OnCycleSkinToneActionPerformed;

        //cycleBodyTypeAction.action.Enable();
        cycleBodyTypeAction.action.performed += OnCycleBodyTypeActionPerformed;

        //cycleHairstyleAction.action.Enable();
        cycleHairstyleAction.action.performed += OnCycleHairstyleActionPerformed;

        //cycleClothesAction.action.Enable();
        cycleClothesAction.action.performed += OnCycleClothesActionPerformed;
    }

    private void OnDisable()
    {
        randomiseCosmeticsAction.action.performed -= OnCosmeticsActionPerformed;
        cycleSkinToneAction.action.performed -= OnCycleSkinToneActionPerformed;
        cycleBodyTypeAction.action.performed -= OnCycleBodyTypeActionPerformed;
        cycleHairstyleAction.action.performed -= OnCycleHairstyleActionPerformed;
        cycleClothesAction.action.performed -= OnCycleClothesActionPerformed;
    }

    private void OnCosmeticsActionPerformed(InputAction.CallbackContext context)
    {
        RandomiseCosmetics();
    }

    private void OnCycleSkinToneActionPerformed(InputAction.CallbackContext context)
    {
        _cosmetics.ChangeToNextSkinTone();
    }

    private void OnCycleBodyTypeActionPerformed(InputAction.CallbackContext context)
    {
        _cosmetics.ChangeToNextBodyType();
    }

    private void OnCycleHairstyleActionPerformed(InputAction.CallbackContext context)
    {
        _cosmetics.ChangeToNextHairstyle();
    }

    private void OnCycleClothesActionPerformed(InputAction.CallbackContext context)
    {
        _cosmetics.ChangeToNextClothes();
    }

    private void RandomiseCosmetics()
    {
        _cosmetics.ChangeAllCosmetics();
    }
}
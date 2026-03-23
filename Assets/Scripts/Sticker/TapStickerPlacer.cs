using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class TapStickerPlacer : MonoBehaviour
{
    private const string RuntimeObjectName = "Tap Sticker Placer";
    
    private static GameObject cachedFairyEffectPrefab;

    private Camera cachedCamera;
    [SerializeField] private PeelSticker3D templateSticker;
    [SerializeField] private FairyCatalogSource fairyCatalogSource;
    private PeelSticker3D templateSourceSticker;
    private StickerSelectionState selectionState;
    private OwnedStickerInventorySource inventorySource;

    private bool isPlacementEnabled = true;

    public event Action<Vector2, bool> PreviewScreenPointChanged;

    public void SetPlacementEnabled(bool enabled)
    {
        isPlacementEnabled = enabled;

        if (!isPlacementEnabled)
        {
            HidePreview();
        }
    }

    public void SetSelectionState(StickerSelectionState selectionState)
    {
        this.selectionState = selectionState;
    }

    public void SetInventorySource(OwnedStickerInventorySource inventorySource)
    {
        this.inventorySource = inventorySource;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureRuntimeInstance()
    {
        if (UnityEngine.Object.FindAnyObjectByType<TapStickerPlacer>() != null)
        {
            return;
        }

        GameObject runtimeObject = new(RuntimeObjectName);
        runtimeObject.AddComponent<TapStickerPlacer>();
    }

    private void Awake()
    {
        cachedCamera = Camera.main;
        CacheTemplateSticker();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        UpdatePreview();

        if (!isPlacementEnabled)
        {
            return;
        }

        if (selectionState?.SelectedSticker == null)
        {
            return;
        }

        if(IsPointerOverUi())
        {
            return;
        }

        if (!TryGetPointerDownPosition(out Vector3 screenPoint))
        {
            return;
        }

        Camera activeCamera = GetActiveCamera();
        if (activeCamera == null)
        {
            return;
        }

        CacheTemplateSticker();
        if (templateSticker == null)
        {
            return;
        }

        if (IsPointerOverSticker(activeCamera, screenPoint))
        {
            return;
        }

        if (!templateSticker.TryGetPlaneHitPoint(activeCamera, screenPoint, out Vector3 worldPoint))
        {
            return;
        }

        StickerDefinition selectedSticker = selectionState.SelectedSticker;
        SpawnSticker(worldPoint);
        inventorySource?.RemoveOwnedSticker(selectedSticker);
    }

    private void OnDestroy()
    {
        NotifyPreviewScreenPoint(Vector2.zero, false);

        if (templateSticker != null)
        {
            Destroy(templateSticker.gameObject);
        }
    }

    private void UpdatePreview()
    {
        StickerDefinition selectedSticker = selectionState?.SelectedSticker;
        int ownedCount = inventorySource != null && selectedSticker != null
            ? inventorySource.GetOwnedStickerCount(selectedSticker)
            : 0;

        if (!isPlacementEnabled || selectedSticker == null || ownedCount <= 0 || IsPointerOverUi())
        {
            HidePreview();
            return;
        }

        Camera activeCamera = GetActiveCamera();
        if (activeCamera == null)
        {
            HidePreview();
            return;
        }

        CacheTemplateSticker();
        if (templateSticker == null)
        {
            HidePreview();
            return;
        }

        Vector3 screenPoint = Input.mousePosition;
        if (!templateSticker.TryGetPlaneHitPoint(activeCamera, screenPoint, out Vector3 worldPoint))
        {
            HidePreview();
            return;
        }

        templateSticker.transform.SetPositionAndRotation(worldPoint, templateSticker.transform.rotation);
        templateSticker.PeelAmount = 0f;
        templateSticker.SetTapPeelEnabled(false);
        SetPreviewVisible(true);
        NotifyPreviewScreenPoint(screenPoint, true);
    }

    private static GameObject LoadFairyEffectPrefab()
    {
        if (cachedFairyEffectPrefab != null)
        {
            return cachedFairyEffectPrefab;
        }

        cachedFairyEffectPrefab = Resources.Load<GameObject>("KiraKiraEffect");
        return cachedFairyEffectPrefab;
    }

    private void CacheTemplateSticker()
    {
        PeelSticker3D selectedStickerPrefab = selectionState?.SelectedSticker?.StickerPrefab;
        if (selectedStickerPrefab == null)
        {
            templateSourceSticker = null;

            if (templateSticker != null)
            {
                Destroy(templateSticker.gameObject);
                templateSticker = null;
            }

            return;
        }

        if (templateSourceSticker == selectedStickerPrefab && templateSticker != null)
        {
            return;
        }

        if (templateSticker != null)
        {
            Destroy(templateSticker.gameObject);
        }

        templateSourceSticker = selectedStickerPrefab;
        templateSticker = Instantiate(selectedStickerPrefab, transform);
        templateSticker.name = "Sticker Template";
        templateSticker.gameObject.SetActive(false);
        templateSticker.PeelAmount = 0f;
        templateSticker.SetTapPeelEnabled(false);
    }

    private bool IsPointerOverSticker(Camera activeCamera, Vector3 screenPoint)
    {
        PeelSticker3D[] stickers = FindObjectsByType<PeelSticker3D>(FindObjectsSortMode.None);
        foreach (PeelSticker3D sticker in stickers)
        {
            if (sticker == null || sticker == templateSticker || !sticker.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (sticker.ContainsScreenPoint(activeCamera, screenPoint))
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnSticker(Vector3 worldPoint)
    {
        PeelSticker3D sticker = Instantiate(templateSticker);
        sticker.name = "Peel Sticker";
        sticker.transform.SetPositionAndRotation(worldPoint, templateSticker.transform.rotation);
        sticker.transform.localScale = templateSticker.transform.localScale;
        sticker.gameObject.SetActive(true);
        sticker.PeelAmount = 0f;
        sticker.SetTapPeelEnabled(false);

        bool shouldAssignFairy = UnityEngine.Random.value < 0.5f;
        FairyDefinition selectedFairy = shouldAssignFairy
            ? FairyWeightedRandomSelector.Select(fairyCatalogSource.GetFairies())
            : null;

        StickerFairyAssignment assignment = selectedFairy != null ? new StickerFairyAssignment(selectedFairy) : null;

        StickerRuntimeRegistry.Register(sticker, assignment);

        if (assignment != null && assignment.HasFairy)
        {
            AttachFairyEffect(sticker);
        }
    }

    private Camera GetActiveCamera()
    {
        if (cachedCamera != null)
        {
            return cachedCamera;
        }

        cachedCamera = Camera.main;
        return cachedCamera;
    }

    private static bool TryGetPointerDownPosition(out Vector3 screenPoint)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                screenPoint = touch.position;
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            screenPoint = Input.mousePosition;
            return true;
        }

        screenPoint = default;
        return false;
    }

    private static bool IsPointerOverUi()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void SetPreviewVisible(bool visible)
    {
        if (templateSticker != null && templateSticker.gameObject.activeSelf != visible)
        {
            templateSticker.gameObject.SetActive(visible);
        }
    }

    private void HidePreview()
    {
        SetPreviewVisible(false);
        NotifyPreviewScreenPoint(Vector2.zero, false);
    }

    private void NotifyPreviewScreenPoint(Vector2 point, bool visible)
    {
        PreviewScreenPointChanged?.Invoke(point, visible);
    }

    private static void AttachFairyEffect(PeelSticker3D sticker)
    {
        if (sticker == null)
        {
            return;
        }

        GameObject fairyEffectPrefab = LoadFairyEffectPrefab();
        if (fairyEffectPrefab == null)
        {
            Debug.LogWarning("KiraKiraEffect.prefab を読み込めませんでした。");
            return;
        }

        GameObject fairyEffect = Instantiate(fairyEffectPrefab, sticker.transform);
        fairyEffect.name = fairyEffectPrefab.name;
        fairyEffect.transform.localPosition = new Vector3(0, 0, 0.1f);
        fairyEffect.transform.localRotation = Quaternion.identity;
        fairyEffect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
}

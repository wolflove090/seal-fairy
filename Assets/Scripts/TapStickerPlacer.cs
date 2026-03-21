//using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class TapStickerPlacer : MonoBehaviour
{
    private const string RuntimeObjectName = "Tap Sticker Placer";
    
    private static GameObject cachedFairyEffectPrefab;

    private Camera cachedCamera;
    [SerializeField] private PeelSticker3D templateSticker;
    private PeelSticker3D templateSourceSticker;
    private StickerSelectionState selectionState;

    private bool isPlacementEnabled = true;

    public void SetPlacementEnabled(bool enabled)
    {
        isPlacementEnabled = enabled;
    }

    public void SetSelectionState(StickerSelectionState selectionState)
    {
        this.selectionState = selectionState;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureRuntimeInstance()
    {
        if (Object.FindAnyObjectByType<TapStickerPlacer>() != null)
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
        if (!Application.isPlaying || !isPlacementEnabled)
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

        SpawnSticker(worldPoint);
    }

    private void OnDestroy()
    {
        if (templateSticker != null)
        {
            Destroy(templateSticker.gameObject);
        }
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
    }

    private bool IsPointerOverSticker(Camera activeCamera, Vector3 screenPoint)
    {
        PeelSticker3D[] stickers = FindObjectsByType<PeelSticker3D>(FindObjectsSortMode.None);
        foreach (PeelSticker3D sticker in stickers)
        {
            if (sticker == null || !sticker.gameObject.activeInHierarchy)
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

        bool hasFairy = Random.value < 0.5f;
        StickerRuntimeRegistry.Register(sticker, hasFairy);

        if (hasFairy)
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

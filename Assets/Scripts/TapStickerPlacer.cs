using UnityEngine;

[DisallowMultipleComponent]
public sealed class TapStickerPlacer : MonoBehaviour
{
    private const string RuntimeObjectName = "Tap Sticker Placer";

    private Camera cachedCamera;
    private PeelSticker3D templateSticker;

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
        if (!Application.isPlaying)
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

    private void CacheTemplateSticker()
    {
        if (templateSticker != null)
        {
            return;
        }

        PeelSticker3D sourceSticker = FindTemplateSource();
        if (sourceSticker == null)
        {
            return;
        }

        templateSticker = Instantiate(sourceSticker, transform);
        templateSticker.name = "Sticker Template";
        templateSticker.gameObject.SetActive(false);
        templateSticker.PeelAmount = 0f;
    }

    private PeelSticker3D FindTemplateSource()
    {
        PeelSticker3D[] stickers = FindObjectsByType<PeelSticker3D>(FindObjectsSortMode.None);
        foreach (PeelSticker3D sticker in stickers)
        {
            if (sticker != null && sticker.gameObject.activeInHierarchy && sticker.gameObject != gameObject)
            {
                return sticker;
            }
        }

        return null;
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

        bool hasFairy = Random.value < 0.5f;
        StickerRuntimeRegistry.Register(sticker, hasFairy);
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
}

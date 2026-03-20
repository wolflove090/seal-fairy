using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class PeelSticker3D : MonoBehaviour
{
    public enum PeelSide
    {
        Right,
        Left
    }

    [Header("Sticker")]
    [SerializeField] private Vector2 size = new(3.2f, 3.2f);
    [SerializeField, Min(4)] private int segments = 28;
    [SerializeField] private PeelSide peelSide = PeelSide.Right;
    [SerializeField, Range(0f, 1f)] private float peelAmount = 0.15f;
    [SerializeField, Range(15f, 180f)] private float maxPeelAngle = 150f;
    [SerializeField, Range(0.2f, 3f)] private float curlExponent = 1.35f;
    [SerializeField, Range(0f, 0.5f)] private float edgeLift = 0.08f;
    [SerializeField, Range(0.5f, 0.98f)] private float detachStart = 0.82f;
    [SerializeField, Range(0f, 1.5f)] private float detachedTravel = 0.45f;
    [SerializeField, Range(0f, 1.5f)] private float detachedLift = 0.55f;
    [SerializeField, Range(120f, 240f)] private float detachedAngle = 188f;

    [Header("Look")]
    [SerializeField] private Texture2D frontTexture;
    [SerializeField] private Texture2D backTexture;
    [SerializeField] private Color frontTint = Color.white;
    [SerializeField] private Color backTint = new(0.86f, 0.83f, 0.76f, 1f);
    [SerializeField] private Color shadowTint = new(0f, 0f, 0f, 0.18f);
    [SerializeField, Range(0f, 0.4f)] private float shadowFlatten = 0.12f;

    [Header("Tap Interaction")]
    [FormerlySerializedAs("allowPointerDrag")]
    [SerializeField, InspectorName("Enable Tap Peel")] private bool allowTapPeel = true;
    [SerializeField, Min(0.05f), InspectorName("Auto Peel Duration")] private float autoPeelDuration = 0.45f;

    private const string FrontChildName = "FrontFace";
    private const string BackChildName = "BackFace";
    private const string ShadowChildName = "Shadow";

    private MeshFilter frontMeshFilter;
    private MeshRenderer frontMeshRenderer;
    private MeshFilter backMeshFilter;
    private MeshRenderer backMeshRenderer;
    private MeshFilter shadowMeshFilter;
    private MeshRenderer shadowMeshRenderer;

    private Mesh frontMesh;
    private Mesh backMesh;
    private Mesh shadowMesh;
    private Material frontMaterial;
    private Material backMaterial;
    private Material shadowMaterial;
    private Camera cachedCamera;
    private bool isAutoPeeling;
    private bool isPeelComplete;

    private bool runtimeTapPeelEnabled;

    public float PeelAmount
    {
        get => peelAmount;
        set
        {
            peelAmount = Mathf.Clamp01(value);
            RebuildGeometry();
        }
    }

    public void SetTapPeelEnabled(bool enabled)
    {
        runtimeTapPeelEnabled = enabled;
    }

    public void SetTextures(Texture2D front, Texture2D back)
    {
        frontTexture = front;
        backTexture = back;
        RebuildGeometry();
    }

    public bool ContainsScreenPoint(Camera activeCamera, Vector3 screenPoint)
    {
        return activeCamera != null
            && TryGetLocalPointer(screenPoint, activeCamera, out Vector3 localPoint)
            && ContainsLocalPoint(localPoint);
    }

    public bool TryGetPlaneHitPoint(Camera activeCamera, Vector3 screenPoint, out Vector3 worldPoint)
    {
        if (activeCamera != null && TryGetWorldPointer(screenPoint, activeCamera, out worldPoint))
        {
            return true;
        }

        worldPoint = default;
        return false;
    }

    private void Awake()
    {
        EnsureSetup();
        RebuildGeometry();
    }

    private void OnEnable()
    {
        EnsureSetup();
        RebuildGeometry();
    }

    private void OnValidate()
    {
        size.x = Mathf.Max(0.01f, size.x);
        size.y = Mathf.Max(0.01f, size.y);
        segments = Mathf.Max(4, segments);
        EnsureSetup();
        RebuildGeometry();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (allowTapPeel && runtimeTapPeelEnabled && !isPeelComplete)
        {
            HandlePointer();
        }

        if (isAutoPeeling && !isPeelComplete)
        {
            float nextAmount = Mathf.MoveTowards(peelAmount, 1f, Time.deltaTime / autoPeelDuration);
            PeelAmount = nextAmount;
            if (nextAmount >= 1f)
            {
                CompletePeel();
            }
        }
    }

    private void OnDestroy()
    {
        DestroyGenerated(frontMesh);
        DestroyGenerated(backMesh);
        DestroyGenerated(shadowMesh);
        DestroyGenerated(frontMaterial);
        DestroyGenerated(backMaterial);
        DestroyGenerated(shadowMaterial);
    }

    private void HandlePointer()
    {
        Camera activeCamera = GetActiveCamera();
        if (activeCamera == null)
        {
            return;
        }

        if (!TryGetPointerDownPosition(out Vector3 pointer))
        {
            return;
        }

        if (!isAutoPeeling && TryGetLocalPointer(pointer, activeCamera, out Vector3 localDown) && ContainsLocalPoint(localDown))
        {
            StartAutoPeel();
        }
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

    private void StartAutoPeel()
    {
        isAutoPeeling = true;
    }

    private void CompletePeel()
    {
        if (isPeelComplete)
        {
            return;
        }

        isPeelComplete = true;
        isAutoPeeling = false;

        if (StickerRuntimeRegistry.TryConsumeFairy(this, out bool hasFairy) && hasFairy)
        {
            Debug.Log("妖精を発見！");
        }

        Destroy(gameObject, 0.5f);
    }

    private bool ContainsLocalPoint(Vector3 localPoint)
    {
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        return Mathf.Abs(localPoint.x) <= halfWidth && Mathf.Abs(localPoint.y) <= halfHeight;
    }

    private bool TryGetLocalPointer(Vector3 screenPoint, Camera activeCamera, out Vector3 localPoint)
    {
        if (TryGetWorldPointer(screenPoint, activeCamera, out Vector3 worldPoint))
        {
            localPoint = transform.InverseTransformPoint(worldPoint);
            return true;
        }

        localPoint = default;
        return false;
    }

    private bool TryGetWorldPointer(Vector3 screenPoint, Camera activeCamera, out Vector3 worldPoint)
    {
        Ray ray = activeCamera.ScreenPointToRay(screenPoint);
        Plane plane = new(transform.forward, transform.position);
        if (plane.Raycast(ray, out float distance))
        {
            worldPoint = ray.GetPoint(distance);
            return true;
        }

        worldPoint = default;
        return false;
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

    private void EnsureSetup()
    {
        frontMeshFilter = EnsureChild(FrontChildName, out frontMeshRenderer);
        backMeshFilter = EnsureChild(BackChildName, out backMeshRenderer);
        shadowMeshFilter = EnsureChild(ShadowChildName, out shadowMeshRenderer);

        frontMesh = EnsureMesh(frontMeshFilter, frontMesh, "PeelStickerFrontMesh");
        backMesh = EnsureMesh(backMeshFilter, backMesh, "PeelStickerBackMesh");
        shadowMesh = EnsureMesh(shadowMeshFilter, shadowMesh, "PeelStickerShadowMesh");

        frontMaterial = EnsureMaterial(frontMeshRenderer, frontMaterial, "PeelStickerFrontMat");
        backMaterial = EnsureMaterial(backMeshRenderer, backMaterial, "PeelStickerBackMat");
        shadowMaterial = EnsureMaterial(shadowMeshRenderer, shadowMaterial, "PeelStickerShadowMat");

        ApplyMaterials();
    }

    private MeshFilter EnsureChild(string childName, out MeshRenderer meshRenderer)
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
            child = new GameObject(childName).transform;
            child.SetParent(transform, false);
        }

        MeshFilter meshFilter = child.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = child.gameObject.AddComponent<MeshFilter>();
        }

        meshRenderer = child.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = child.gameObject.AddComponent<MeshRenderer>();
        }

        child.localPosition = childName == ShadowChildName ? new Vector3(0f, 0f, -0.005f) : Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        return meshFilter;
    }

    private static Mesh EnsureMesh(MeshFilter filter, Mesh mesh, string meshName)
    {
        if (mesh == null)
        {
            mesh = new Mesh { name = meshName };
            mesh.MarkDynamic();
        }

        filter.sharedMesh = mesh;
        return mesh;
    }

    private static Material EnsureMaterial(MeshRenderer renderer, Material material, string materialName)
    {
        Shader shader = Shader.Find("Unlit/Transparent");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            return material;
        }

        if (material == null || material.shader != shader)
        {
            material = new Material(shader) { name = materialName };
        }

        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        return material;
    }

    private void ApplyMaterials()
    {
        if (frontMaterial != null)
        {
            frontMaterial.color = frontTint;
            frontMaterial.mainTexture = frontTexture;
        }

        if (backMaterial != null)
        {
            backMaterial.color = backTint;
            backMaterial.mainTexture = backTexture != null ? backTexture : frontTexture;
        }

        if (shadowMaterial != null)
        {
            Color shadowColor = shadowTint;
            shadowColor.a *= 1f - GetDetachProgress();
            shadowMaterial.color = shadowColor;
            shadowMaterial.mainTexture = null;
        }
    }

    private void RebuildGeometry()
    {
        if (frontMesh == null || backMesh == null || shadowMesh == null)
        {
            return;
        }

        BuildStickerMesh(frontMesh, false);
        BuildStickerMesh(backMesh, true);
        BuildShadowMesh(shadowMesh);
        ApplyMaterials();
    }

    private void BuildStickerMesh(Mesh mesh, bool reverseWinding)
    {
        int columns = segments + 1;
        Vector3[] vertices = new Vector3[columns * 2];
        Vector2[] uvs = new Vector2[columns * 2];
        int[] triangles = new int[segments * 6];

        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        float step = size.x / segments;
        float direction = peelSide == PeelSide.Right ? 1f : -1f;
        float detachProgress = GetDetachProgress();
        float attachedPeel = GetAttachedPeelAmount();
        float anchorX = -direction * halfWidth;
        Vector3 cursor = new(anchorX, 0f, 0f);
        Vector3 detachedOffset = GetDetachedOffset(direction, detachProgress);
        bool detached = detachProgress > 0f;

        for (int column = 0; column < columns; column++)
        {
            float progress = column / (float)segments;
            float curve = Mathf.Pow(progress, curlExponent);
            float attachedAngle = maxPeelAngle * attachedPeel * curve * direction;
            float freeAngle = detachedAngle * detachProgress * direction;
            float angle = attachedAngle + freeAngle;
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

            if (column > 0)
            {
                cursor += rotation * Vector3.right * (step * direction);
            }

            float attachedLift = Mathf.Sin(progress * Mathf.PI) * edgeLift * attachedPeel;
            float freeLift = detachProgress * detachedLift;
            Vector3 baseOffset = detached ? Vector3.Lerp(Vector3.zero, detachedOffset, detachProgress) : Vector3.zero;
            Vector3 verticalLift = new(0f, 0f, attachedLift + freeLift);
            Vector3 bottom = cursor + baseOffset + verticalLift + new Vector3(0f, -halfHeight, 0f);
            Vector3 top = cursor + baseOffset + verticalLift + new Vector3(0f, halfHeight, 0f);

            int vertexIndex = column * 2;
            vertices[vertexIndex] = bottom;
            vertices[vertexIndex + 1] = top;

            float u = peelSide == PeelSide.Right ? progress : 1f - progress;
            uvs[vertexIndex] = new Vector2(u, 0f);
            uvs[vertexIndex + 1] = new Vector2(u, 1f);
        }

        int triangleIndex = 0;
        for (int column = 0; column < segments; column++)
        {
            int root = column * 2;
            if (!reverseWinding)
            {
                triangles[triangleIndex++] = root;
                triangles[triangleIndex++] = root + 1;
                triangles[triangleIndex++] = root + 2;
                triangles[triangleIndex++] = root + 2;
                triangles[triangleIndex++] = root + 1;
                triangles[triangleIndex++] = root + 3;
            }
            else
            {
                triangles[triangleIndex++] = root + 2;
                triangles[triangleIndex++] = root + 1;
                triangles[triangleIndex++] = root;
                triangles[triangleIndex++] = root + 3;
                triangles[triangleIndex++] = root + 1;
                triangles[triangleIndex++] = root + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void BuildShadowMesh(Mesh mesh)
    {
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        float detachProgress = GetDetachProgress();
        float contractedWidth = Mathf.Lerp(size.x, size.x * 0.62f, peelAmount);
        contractedWidth = Mathf.Lerp(contractedWidth, size.x * 0.28f, detachProgress);
        float offsetX = peelSide == PeelSide.Right
            ? Mathf.Lerp(0f, -size.x * 0.1f, peelAmount)
            : Mathf.Lerp(0f, size.x * 0.1f, peelAmount);
        offsetX += peelSide == PeelSide.Right
            ? Mathf.Lerp(0f, -detachedTravel * 0.35f, detachProgress)
            : Mathf.Lerp(0f, detachedTravel * 0.35f, detachProgress);

        Vector3[] vertices =
        {
            new(offsetX - contractedWidth * 0.5f, -halfHeight * (1f - shadowFlatten), 0f),
            new(offsetX - contractedWidth * 0.5f, halfHeight * (1f - shadowFlatten), 0f),
            new(offsetX + contractedWidth * 0.5f, -halfHeight * (1f - shadowFlatten), 0f),
            new(offsetX + contractedWidth * 0.5f, halfHeight * (1f - shadowFlatten), 0f),
        };

        int[] triangles = { 0, 1, 2, 2, 1, 3 };
        Vector2[] uvs = { Vector2.zero, Vector2.up, Vector2.right, Vector2.one };

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }

    private float GetAttachedPeelAmount()
    {
        return detachStart <= 0f ? peelAmount : Mathf.Min(peelAmount, detachStart);
    }

    private float GetDetachProgress()
    {
        if (detachStart >= 1f)
        {
            return 0f;
        }

        return Mathf.Clamp01(Mathf.InverseLerp(detachStart, 1f, peelAmount));
    }

    private Vector3 GetDetachedOffset(float direction, float detachProgress)
    {
        float horizontal = detachedTravel * detachProgress * -direction;
        float depth = detachedLift * detachProgress;
        return new Vector3(horizontal, 0f, depth);
    }

    private static void DestroyGenerated(Object obj)
    {
        if (obj == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(obj);
        }
        else
        {
            DestroyImmediate(obj);
        }
    }
}

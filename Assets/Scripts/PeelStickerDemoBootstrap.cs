using UnityEngine;
using UnityEngine.SceneManagement;

public static class PeelStickerDemoBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateDemoIfSceneIsEmpty()
    {
        if (Object.FindAnyObjectByType<PeelSticker3D>() != null)
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (HasNonRuntimeRoots(scene))
        {
            return;
        }

        CreateCamera();
        CreateBackdrop();
        CreateSticker();
    }

    private static bool HasNonRuntimeRoots(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (rootObject.name != "Tap Sticker Placer")
            {
                return true;
            }
        }

        return false;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new("Main Camera");
        cameraObject.tag = "MainCamera";

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 3.2f;
        camera.backgroundColor = new Color(0.95f, 0.93f, 0.88f, 1f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void CreateBackdrop()
    {
        GameObject backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backdrop.name = "Desk";
        backdrop.transform.position = new Vector3(0f, 0f, 1.2f);
        backdrop.transform.localScale = new Vector3(12f, 8f, 1f);

        Renderer renderer = backdrop.GetComponent<Renderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Unlit/Color"));
        renderer.sharedMaterial.color = new Color(0.87f, 0.84f, 0.78f, 1f);
    }

    private static void CreateSticker()
    {
        GameObject stickerObject = new("Peel Sticker");
        PeelSticker3D sticker = stickerObject.AddComponent<PeelSticker3D>();

        Texture2D front = BuildStickerTexture(
            new Color(0.97f, 0.36f, 0.25f, 1f),
            new Color(1f, 0.95f, 0.92f, 1f),
            new Color(1f, 0.82f, 0.72f, 1f));

        Texture2D back = BuildStickerTexture(
            new Color(0.88f, 0.85f, 0.77f, 1f),
            new Color(0.93f, 0.91f, 0.85f, 1f),
            new Color(0.79f, 0.75f, 0.67f, 1f));

        sticker.SetTextures(front, back);
        sticker.transform.position = Vector3.zero;
    }

    private static Texture2D BuildStickerTexture(Color main, Color accent, Color border)
    {
        const int size = 256;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = "RuntimeStickerTexture"
        };

        Vector2 center = new(size * 0.5f, size * 0.52f);
        float radius = size * 0.33f;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                float dx = x - center.x;
                float dy = y - center.y;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                Color color = Color.white;
                if (distance < radius)
                {
                    color = main;
                }

                if (Mathf.Abs(distance - radius) < 10f)
                {
                    color = border;
                }

                if (Mathf.Abs(dx) < 12f || Mathf.Abs(dy) < 12f)
                {
                    color = accent;
                }

                pixels[index] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        return texture;
    }
}

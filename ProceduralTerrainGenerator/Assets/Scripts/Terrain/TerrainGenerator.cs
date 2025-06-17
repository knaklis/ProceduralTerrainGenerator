// Файл: TerrainGenerator.cs
using UnityEngine;
using System.IO;

[RequireComponent(typeof(Terrain))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Розмір карти")]
    public int width = 256;
    public int height = 256;

    [Header("Налаштування шуму")]
    public float scale = 15f;
    public float offsetX = 350f;
    public float offsetY = 780f;
    [Range(1, 10)]
    public int octaves = 6;
    [Range(0f, 1f)]
    public float persistence = 0.5f;
    public float lacunarity = 2.2f;
    public float heightMultiplier = 2.5f;

    [Header("Опції декору")]
    public bool addGrass = false;
    public bool addTrees = false;

    [Header("Налаштування трави")]
    public Texture2D grassTexture;  // ← перетягнеш у інспекторі

    [Header("Налаштування дерев")]
    public GameObject treePrefab;          // ← Префаб дерева
    public int treeCount = 1;            // ← Скільки дерев розміщати

    private Terrain terrain;

    void Start()
    {
        terrain = GetComponent<Terrain>();
    }

    public void GenerateTerrain()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();

        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);

        TerrainData terrainData = terrain.terrainData;
        int resolution = Mathf.Max(width, height);
        terrainData.heightmapResolution = resolution;
        terrainData.size = new Vector3(width, width / 4f, height);

        float[,] heights = new float[resolution, resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float n = GeneratePerlinNoise(x, y, resolution);
                heights[y, x] = n * heightMultiplier;
            }
        }
        terrainData.SetHeights(0, 0, heights);

        if (addGrass)
            AddGrass();

        if (addTrees)
            AddTrees();
    }

    private float GeneratePerlinNoise(int x, int y, int resolution)
    {
        float amp = 1f, freq = 1f, noiseH = 0f, maxAmp = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sx = (x / (float)resolution) * scale * freq + offsetX;
            float sy = (y / (float)resolution) * scale * freq + offsetY;
            noiseH += Mathf.PerlinNoise(sx, sy) * amp;
            maxAmp += amp;
            amp *= persistence;
            freq *= lacunarity;
        }

        return noiseH / maxAmp;
    }

    private void AddGrass()
    {
        if (grassTexture == null)
        {
            Debug.LogWarning("Не призначена текстура трави!");
            return;
        }

        TerrainData terrainData = terrain.terrainData;

        DetailPrototype grass = new DetailPrototype
        {
            prototypeTexture = grassTexture,
            renderMode = DetailRenderMode.GrassBillboard,
            usePrototypeMesh = false,
            minHeight = 0.5f,
            maxHeight = 1f,
            minWidth = 0.5f,
            maxWidth = 1f,
            healthyColor = Color.green,
            dryColor = Color.yellow
        };

        terrainData.detailPrototypes = new DetailPrototype[] { grass };

        int resolution = 256;
        terrainData.SetDetailResolution(resolution, 8);

        int[,] detailLayer = new int[resolution, resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                detailLayer[x, y] = 1;
            }
        }

        terrainData.SetDetailLayer(0, 0, 0, detailLayer);
    }


    private void AddTrees()
    {
        if (treePrefab == null)
        {
            Debug.LogWarning("Tree prefab не призначений!");
            return;
        }

        TerrainData terrainData = terrain.terrainData;

        // Призначаємо prefab як прототип дерева
        TreePrototype[] treePrototypes = new TreePrototype[1];
        treePrototypes[0] = new TreePrototype { prefab = treePrefab };
        terrainData.treePrototypes = treePrototypes;

        // Створюємо дерева на випадкових позиціях
        int treeCount = 100; // скільки дерев
        TreeInstance[] instances = new TreeInstance[treeCount];

        for (int i = 0; i < treeCount; i++)
        {
            float posX = Random.Range(0f, 1f);
            float posZ = Random.Range(0f, 1f);
            float height = terrainData.GetInterpolatedHeight(posX, posZ) / terrainData.size.y;

            TreeInstance tree = new TreeInstance
            {
                position = new Vector3(posX, height, posZ),
                prototypeIndex = 0,
                widthScale = 1f,
                heightScale = 1f,
                color = Color.white,
                lightmapColor = Color.white
            };

            instances[i] = tree;
        }

        terrainData.treeInstances = instances;
    }

    public void SaveTerrainToPNG()
    {
        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("Ландшафт або його дані не ініціалізовані!");
            return;
        }

        TerrainData tData = terrain.terrainData;
        int resolution = tData.heightmapResolution;
        float[,] heights = tData.GetHeights(0, 0, resolution, resolution);

        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
        Color[] colors = new Color[resolution * resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float h = heights[y, x];
                colors[y * resolution + x] = new Color(h, h, h);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        byte[] pngData = texture.EncodeToPNG();
        if (pngData == null)
        {
            Debug.LogError("Не вдалося закодувати текстуру в PNG.");
            return;
        }

        // === Створення директорії ===
        string exportFolder = Path.Combine(Application.persistentDataPath, "Exports", "Heightmaps");
        Directory.CreateDirectory(exportFolder);

        string fileName = $"terrain_heightmap_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        string path = Path.Combine(exportFolder, fileName);

        try
        {
            File.WriteAllBytes(path, pngData);
            Debug.Log($"Heightmap збережено у файл: {path}");

            // Робимо скріншот і відкриваємо папку з PNG
            CaptureScreenshot();
            OpenFolder(path);

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Помилка при збереженні heightmap: {e.Message}");
        }
        finally
        {
            DestroyImmediate(texture);
        }
    }


    /// <summary>
    /// Робить скріншот лише з активної камери без UI.
    /// </summary>
    public void CaptureScreenshot()
    {
        Camera cam = Camera.main; // Або вкажи конкретну камеру, якщо потрібно
        if (cam == null)
        {
            Debug.LogError("Основна камера не знайдена!");
            return;
        }

        int width = Screen.width;
        int height = Screen.height;

        // Створюємо рендер-таргет
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;

        // Рендеримо камеру в текстуру
        Texture2D screenImage = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenImage.Apply();

        // Очищаємо
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Кодуємо в PNG
        byte[] imageBytes = screenImage.EncodeToPNG();
        Destroy(screenImage);

        // Створюємо папку
        string screenshotFolder = Path.Combine(Application.persistentDataPath, "Exports", "Screenshots");
        Directory.CreateDirectory(screenshotFolder);

        // Зберігаємо файл
        string fileName = $"terrain_screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        string path = Path.Combine(screenshotFolder, fileName);
        File.WriteAllBytes(path, imageBytes);
        Debug.Log($"Скріншот камери збережено у файл: {path}");

        // Відкриває папку
        OpenFolder(path);

    }

    private void OpenFolder(string filePath)
    {
#if UNITY_STANDALONE_WIN
        // Відкриває провідник Windows і виділяє файл
        System.Diagnostics.Process.Start("explorer.exe", "/select," + filePath.Replace("/", "\\"));
#elif UNITY_STANDALONE_OSX
    // Відкриває Finder і виділяє файл
    System.Diagnostics.Process.Start("open", "-R " + filePath);
#else
    Debug.Log($"Автоматичне відкриття папки не підтримується на цій платформі.");
#endif
    }


}


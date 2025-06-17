// Файл: UIController.cs
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Залежності")]
    public TerrainGenerator terrainGen;
    public Button generateButton;
    public Button saveButton;

    [Header("Елементи UI")]
    public Slider scaleSlider;
    public Text scaleValueText;

    public InputField widthInput;
    public InputField heightInput;
    public InputField offsetXInput;
    public InputField offsetYInput;
    public InputField octavesInput;
    public InputField persistenceInput;
    public InputField lacunarityInput;
    public InputField heightMultiplierInput;

    [Header("Decoration Toggles")]
    public Toggle grassToggle;
    public Toggle treesToggle;

    [Header("Користувач")]
    public Text userLabel;                 // ← перетягнути Text у інспекторі

    /* ---------- Unity lifecycle ---------- */

    void Start()
    {
        // Показуємо ID, якщо вже авторизовані
        if (PlayerPrefs.HasKey("userId") && userLabel != null)
            userLabel.text = "ID " + PlayerPrefs.GetInt("userId");

        if (scaleSlider != null)
        {
            scaleSlider.onValueChanged.AddListener(OnScaleSliderChanged);
            OnScaleSliderChanged(scaleSlider.value);     // 1-ше оновлення
        }

        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateClick);

        if (saveButton != null) // ← ДОДАЙТЕ ЦЕЙ БЛОК
            saveButton.onClick.AddListener(OnSaveClick);
    }

    void OnDestroy()
    {
        if (scaleSlider != null)
            scaleSlider.onValueChanged.RemoveListener(OnScaleSliderChanged);

        if (generateButton != null)
            generateButton.onClick.RemoveListener(OnGenerateClick);

        if (saveButton != null) // ← ДОДАЙТЕ ЦЕЙ БЛОК
            saveButton.onClick.RemoveListener(OnSaveClick);
    }

    /* ---------- UI callbacks ---------- */

    void OnScaleSliderChanged(float value)
    {
        if (scaleValueText != null)
            scaleValueText.text = value.ToString("F1");
    }

    void OnSaveClick() // ← ДОДАЙТЕ ЦЕЙ МЕТОД
    {
        if (terrainGen != null)
        {
            terrainGen.SaveTerrainToPNG();
        }
        else
        {
            Debug.LogError("Посилання на TerrainGenerator не встановлено в UIController!");
        }
    }

    void OnGenerateClick()
    {
        if (terrainGen == null) return;

        // Читаємо параметри з полів
        if (int.TryParse(widthInput.text, out var w)) terrainGen.width = w;
        if (int.TryParse(heightInput.text, out var h)) terrainGen.height = h;
        if (float.TryParse(offsetXInput.text, out var offX)) terrainGen.offsetX = offX;
        if (float.TryParse(offsetYInput.text, out var offY)) terrainGen.offsetY = offY;
        if (int.TryParse(octavesInput.text, out var oct)) terrainGen.octaves = oct;
        if (float.TryParse(persistenceInput.text, out var per)) terrainGen.persistence = per;
        if (float.TryParse(lacunarityInput.text, out var lac)) terrainGen.lacunarity = lac;
        if (float.TryParse(heightMultiplierInput.text, out var hm)) terrainGen.heightMultiplier = hm;

        terrainGen.scale = (scaleSlider != null) ? scaleSlider.value : terrainGen.scale;
        terrainGen.addGrass = grassToggle != null && grassToggle.isOn;
        terrainGen.addTrees = treesToggle != null && treesToggle.isOn;

        terrainGen.GenerateTerrain();
    }
}

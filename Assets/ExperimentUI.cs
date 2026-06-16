using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the UI Canvas root.
/// Wire up all references in the Inspector.
///
/// Required UI hierarchy:
///   Canvas
///   ├── Panel (background)
///   │   ├── ObserveToggle        (Toggle)
///   │   ├── LabelObserved        (TMP_Text)
///   │   ├── SliderSeparation     (Slider  0.3 → 3.0)
///   │   ├── LabelSeparation      (TMP_Text)
///   │   ├── SliderWidth          (Slider  0.05 → 1.0)
///   │   ├── LabelWidth           (TMP_Text)
///   │   ├── SliderWavelength     (Slider  0.1 → 2.0)
///   │   ├── LabelWavelength      (TMP_Text)
///   │   ├── SliderRate           (Slider  1 → 100)
///   │   ├── LabelRate            (TMP_Text)
///   │   └── BtnClear             (Button)
///   └── LabelMode                (TMP_Text – big centre label)
/// </summary>
public class ExperimentUI : MonoBehaviour
{
    [Header("Manager")]
    public DoubleSlitManager manager;

    [Header("Observe Toggle")]
    public Toggle    observeToggle;
    public TMP_Text  labelObserved;

    [Header("Sliders")]
    public Slider   sliderSeparation;
    public TMP_Text labelSeparation;

    public Slider   sliderWidth;
    public TMP_Text labelWidth;

    public Slider   sliderWavelength;
    public TMP_Text labelWavelength;

    public Slider   sliderRate;
    public TMP_Text labelRate;

    [Header("Misc")]
    public Button   btnClear;
    public TMP_Text labelMode;   // big centre-screen label

    // ── colours ──────────────────────────────────────────────────────────
    static readonly Color ColorWave     = new Color(0.3f, 0.6f, 1f);
    static readonly Color ColorParticle = new Color(1f,   0.5f, 0.2f);

    void Start()
    {
        // Observe toggle
        observeToggle.onValueChanged.AddListener(OnObserveChanged);

        // Sliders
        sliderSeparation.onValueChanged.AddListener(v => {
            manager.OnSlitSeparationChanged(v);
            labelSeparation.text = $"Slit separation: {v:F2}";
        });
        sliderWidth.onValueChanged.AddListener(v => {
            manager.OnSlitWidthChanged(v);
            labelWidth.text = $"Slit width: {v:F2}";
        });
        sliderWavelength.onValueChanged.AddListener(v => {
            manager.OnWavelengthChanged(v);
            labelWavelength.text = $"Wavelength: {v:F2}";
        });
        sliderRate.onValueChanged.AddListener(v => {
            manager.OnEmitterRateChanged(v);
            labelRate.text = $"Photons/s: {(int)v}";
        });

        // Clear button
        btnClear.onClick.AddListener(() => manager.detectionScreen.Clear());

        // Sync initial values
        sliderSeparation.value = manager.slitSeparation;
        sliderWidth.value      = manager.slitWidth;
        sliderWavelength.value = manager.wavelength;
        sliderRate.value       = manager.emitterRate;
        observeToggle.isOn     = manager.isObserved;
        RefreshModeLabel(manager.isObserved);
    }

    void OnObserveChanged(bool observed)
    {
        manager.SetObserved(observed);
        RefreshModeLabel(observed);
    }

    void RefreshModeLabel(bool observed)
    {
        if (observed)
        {
            labelMode.text  = "OBSERVED\n<size=60%>Particle behaviour — two bands</size>";
            labelMode.color = ColorParticle;
            if (labelObserved) labelObserved.text = "Detector: ON";
        }
        else
        {
            labelMode.text  = "UNOBSERVED\n<size=60%>Wave behaviour — interference pattern</size>";
            labelMode.color = ColorWave;
            if (labelObserved) labelObserved.text = "Detector: OFF";
        }
    }
}

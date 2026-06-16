using UnityEngine;

/// <summary>
/// Main controller. Attach to an empty GameObject called "DoubleSlit".
/// Wire up all references in the Inspector.
/// </summary>
public class DoubleSlitManager : MonoBehaviour
{
    [Header("Experiment Settings")]
    public bool isObserved = false;
    [Range(0.1f, 2f)]  public float slitSeparation = 1.0f;
    [Range(0.05f, 1f)] public float slitWidth      = 0.3f;
    [Range(0.1f, 2f)]  public float wavelength     = 0.5f;
    [Range(1f, 100f)]  public float emitterRate    = 30f;

    [Header("References")]
    public ParticleEmitter    emitter;
    public DetectionScreen    detectionScreen;
    public WaveVisualizer     waveVisualizer;
    public BarrierController  barrier;

    // Called by UI toggle
    public void SetObserved(bool observed)
    {
        isObserved = observed;
        detectionScreen.Clear();
    }

    // Called by UI sliders (OnValueChanged)
    public void OnSlitSeparationChanged(float v) { slitSeparation = v; detectionScreen.Clear(); }
    public void OnSlitWidthChanged(float v)      { slitWidth      = v; detectionScreen.Clear(); }
    public void OnWavelengthChanged(float v)     { wavelength     = v; detectionScreen.Clear(); }
    public void OnEmitterRateChanged(float v)    { emitterRate    = v; }

    // ── private ──────────────────────────────────────────────────────────
    float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= 1f / emitterRate)
        {
            _timer = 0f;
            emitter.EmitParticle(isObserved, slitWidth, slitSeparation, wavelength);
        }
    }
}

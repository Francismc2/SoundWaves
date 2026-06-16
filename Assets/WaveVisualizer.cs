using UnityEngine;

/// <summary>
/// Attach to a large Quad that covers the whole scene (scale ~10 x 10),
/// placed slightly behind everything (z = 0.1).
/// The material must use the WaveShader.shader (Custom/DoubleSlitWave).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class WaveVisualizer : MonoBehaviour
{
    [Header("Fade Speed")]
    public float fadeSpeed = 3f;

    DoubleSlitManager _mgr;
    Material          _mat;
    float             _currentAlpha = 1f;

    static readonly int _PropTime      = Shader.PropertyToID("_WaveTime");
    static readonly int _PropSlitSep   = Shader.PropertyToID("_SlitSep");
    static readonly int _PropSlitWidth = Shader.PropertyToID("_SlitWidth");
    static readonly int _PropWavelength= Shader.PropertyToID("_Wavelength");
    static readonly int _PropAmplitude = Shader.PropertyToID("_Amplitude");

    void Awake()
    {
        _mgr = FindObjectOfType<DoubleSlitManager>();
        _mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Fade wave in/out based on observed state
        float targetAlpha = _mgr.isObserved ? 0f : 1f;
        _currentAlpha     = Mathf.Lerp(_currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        _mat.SetFloat(_PropTime,       Time.time);
        _mat.SetFloat(_PropSlitSep,    _mgr.slitSeparation);
        _mat.SetFloat(_PropSlitWidth,  _mgr.slitWidth);
        _mat.SetFloat(_PropWavelength, _mgr.wavelength);
        _mat.SetFloat(_PropAmplitude,  _currentAlpha);
    }
}

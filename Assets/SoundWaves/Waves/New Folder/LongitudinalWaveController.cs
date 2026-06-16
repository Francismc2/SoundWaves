using UnityEngine;

/// <summary>
/// Attach this to any GameObject that has a MeshRenderer using your
/// Longitudinal Wave Shader Graph material.
/// It forwards Inspector sliders → material properties every frame.
/// </summary>
[ExecuteAlways]
public class LongitudinalWaveController : MonoBehaviour
{
    [Header("Target Material (leave blank to use this object's material)")]
    public Material waveMaterial;

    [Header("Wave Parameters")]
    [Range(0.01f, 0.25f)] public float amplitude  = 0.08f;
    [Range(0.1f,  5f)]    public float frequency  = 2f;
    [Range(0.1f,  5f)]    public float waveSpeed  = 1f;
    [Range(1f,    8f)]    public float numWaves   = 3f;

    [Header("Grid")]
    [Range(5f,   80f)]    public float particleCols = 30f;
    [Range(1f,   20f)]    public float particleRows =  8f;

    [Header("Time")]
    public bool  useUnityTime = true;
    [Range(0f, 100f)] public float manualTime = 0f;

    // Cached property IDs for performance
    static readonly int _Amplitude     = Shader.PropertyToID("_Amplitude");
    static readonly int _Frequency     = Shader.PropertyToID("_Frequency");
    static readonly int _WaveSpeed     = Shader.PropertyToID("_WaveSpeed");
    static readonly int _NumWaves      = Shader.PropertyToID("_NumWaves");
    static readonly int _ParticleCols  = Shader.PropertyToID("_ParticleCols");
    static readonly int _ParticleRows  = Shader.PropertyToID("_ParticleRows");
    static readonly int _TimeOverride  = Shader.PropertyToID("_TimeOverride");

    Material _mat;

    void OnEnable()
    {
        _mat = waveMaterial != null
             ? waveMaterial
             : GetComponent<Renderer>()?.sharedMaterial;
    }

    void Update()
    {
        if (_mat == null) return;

        _mat.SetFloat(_Amplitude,    amplitude);
        _mat.SetFloat(_Frequency,    frequency);
        _mat.SetFloat(_WaveSpeed,    waveSpeed);
        _mat.SetFloat(_NumWaves,     numWaves);
        _mat.SetFloat(_ParticleCols, particleCols);
        _mat.SetFloat(_ParticleRows, particleRows);
        _mat.SetFloat(_TimeOverride, useUnityTime ? Time.time : manualTime);
    }
}

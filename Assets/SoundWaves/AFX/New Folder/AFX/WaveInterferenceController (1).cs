using UnityEngine;

/// <summary>
/// Attach this to any GameObject that has a MeshRenderer using the
/// Custom/WaveInterference shader.  All wave parameters are exposed in
/// the Inspector and pushed to the material every frame so you can
/// tweak them at runtime without touching the shader directly.
///
/// SETUP QUICK-START
/// -----------------
/// 1. Create a Plane (or any subdivided mesh – see tip below).
/// 2. Create a new Material → assign shader "Custom/WaveInterference".
/// 3. Attach this script to the same GameObject.
/// 4. Hit Play and adjust the sliders in the Inspector.
///
/// TIP: Unity's default Plane only has 11×11 verts.  For smooth vertex
/// displacement import a higher-resolution plane, or use the free
/// "ProBuilder" package to make one (e.g. 100×100 divisions).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class WaveInterferenceController : MonoBehaviour
{
    // ── Wave 1 ────────────────────────────────────────────────────────
    [Header("Wave 1")]
    [Range(0f, 2f)]  public float wave1Amplitude  = 0.30f;
    [Range(0f, 10f)] public float wave1Frequency  = 2.00f;
    [Range(0f, 10f)] public float wave1Speed      = 1.00f;
    [Range(-1f, 1f)] public float wave1DirectionX = 1.00f;
    [Range(-1f, 1f)] public float wave1DirectionY = 0.00f;

    // ── Wave 2 ────────────────────────────────────────────────────────
    [Header("Wave 2")]
    [Range(0f, 2f)]  public float wave2Amplitude  = 0.30f;
    [Range(0f, 10f)] public float wave2Frequency  = 2.00f;
    [Range(0f, 10f)] public float wave2Speed      = 1.00f;
    [Range(-1f, 1f)] public float wave2DirectionX = 0.707f;
    [Range(-1f, 1f)] public float wave2DirectionY = 0.707f;

    // ── Wave 3 (optional) ────────────────────────────────────────────
    [Header("Wave 3 (optional)")]
    public bool      wave3Enabled    = false;
    [Range(0f, 2f)]  public float wave3Amplitude  = 0.20f;
    [Range(0f, 10f)] public float wave3Frequency  = 3.00f;
    [Range(0f, 10f)] public float wave3Speed      = 0.80f;
    [Range(-1f, 1f)] public float wave3DirectionX = -1.00f;
    [Range(-1f, 1f)] public float wave3DirectionY =  0.00f;

    // ── Visuals ───────────────────────────────────────────────────────
    [Header("Visuals")]
    public Color colorTrough    = new Color(0.03f, 0.12f, 0.40f, 1f);
    public Color colorCrest     = new Color(0.20f, 0.65f, 1.00f, 1f);
    public Color foamColor      = new Color(0.90f, 0.97f, 1.00f, 1f);
    [Range(0f, 1f)] public float foamThreshold = 0.78f;
    [Range(0f, 1f)] public float transparency  = 0.88f;

    // ── Lighting ──────────────────────────────────────────────────────
    [Header("Lighting")]
    public Color specularColor = Color.white;
    [Range(1f, 256f)] public float shininess = 90f;

    // ── Geometry ──────────────────────────────────────────────────────
    [Header("Geometry")]
    [Range(0f, 1f)] public float vertexDisplacementScale = 0.25f;

    // ─────────────────────────────────────────────────────────────────
    // Cached shader property IDs (faster than string look-up every frame)
    // ─────────────────────────────────────────────────────────────────
    static readonly int ID_W1Amp   = Shader.PropertyToID("_Wave1Amplitude");
    static readonly int ID_W1Freq  = Shader.PropertyToID("_Wave1Frequency");
    static readonly int ID_W1Spd   = Shader.PropertyToID("_Wave1Speed");
    static readonly int ID_W1DirX  = Shader.PropertyToID("_Wave1DirectionX");
    static readonly int ID_W1DirY  = Shader.PropertyToID("_Wave1DirectionY");

    static readonly int ID_W2Amp   = Shader.PropertyToID("_Wave2Amplitude");
    static readonly int ID_W2Freq  = Shader.PropertyToID("_Wave2Frequency");
    static readonly int ID_W2Spd   = Shader.PropertyToID("_Wave2Speed");
    static readonly int ID_W2DirX  = Shader.PropertyToID("_Wave2DirectionX");
    static readonly int ID_W2DirY  = Shader.PropertyToID("_Wave2DirectionY");

    static readonly int ID_W3En    = Shader.PropertyToID("_Wave3Enabled");
    static readonly int ID_W3Amp   = Shader.PropertyToID("_Wave3Amplitude");
    static readonly int ID_W3Freq  = Shader.PropertyToID("_Wave3Frequency");
    static readonly int ID_W3Spd   = Shader.PropertyToID("_Wave3Speed");
    static readonly int ID_W3DirX  = Shader.PropertyToID("_Wave3DirectionX");
    static readonly int ID_W3DirY  = Shader.PropertyToID("_Wave3DirectionY");

    static readonly int ID_ColTro  = Shader.PropertyToID("_ColorTrough");
    static readonly int ID_ColCre  = Shader.PropertyToID("_ColorCrest");
    static readonly int ID_Foam    = Shader.PropertyToID("_FoamColor");
    static readonly int ID_FoamThr = Shader.PropertyToID("_FoamThreshold");
    static readonly int ID_Trans   = Shader.PropertyToID("_Transparency");

    static readonly int ID_Spec    = Shader.PropertyToID("_SpecColor");
    static readonly int ID_Shine   = Shader.PropertyToID("_Shininess");
    static readonly int ID_Disp    = Shader.PropertyToID("_DispScale");

    // ─────────────────────────────────────────────────────────────────
    Material _mat;

    void OnEnable()
    {
        var rend = GetComponent<Renderer>();
        // Use sharedMaterial in Edit-mode, material in Play-mode
        _mat = Application.isPlaying ? rend.material : rend.sharedMaterial;
        PushToMaterial();
    }

    void Update() => PushToMaterial();

    void PushToMaterial()
    {
        if (_mat == null) return;

        // Wave 1
        _mat.SetFloat(ID_W1Amp,  wave1Amplitude);
        _mat.SetFloat(ID_W1Freq, wave1Frequency);
        _mat.SetFloat(ID_W1Spd,  wave1Speed);
        _mat.SetFloat(ID_W1DirX, wave1DirectionX);
        _mat.SetFloat(ID_W1DirY, wave1DirectionY);

        // Wave 2
        _mat.SetFloat(ID_W2Amp,  wave2Amplitude);
        _mat.SetFloat(ID_W2Freq, wave2Frequency);
        _mat.SetFloat(ID_W2Spd,  wave2Speed);
        _mat.SetFloat(ID_W2DirX, wave2DirectionX);
        _mat.SetFloat(ID_W2DirY, wave2DirectionY);

        // Wave 3
        _mat.SetFloat(ID_W3En,   wave3Enabled ? 1f : 0f);
        _mat.SetFloat(ID_W3Amp,  wave3Amplitude);
        _mat.SetFloat(ID_W3Freq, wave3Frequency);
        _mat.SetFloat(ID_W3Spd,  wave3Speed);
        _mat.SetFloat(ID_W3DirX, wave3DirectionX);
        _mat.SetFloat(ID_W3DirY, wave3DirectionY);

        // Visuals
        _mat.SetColor(ID_ColTro,  colorTrough);
        _mat.SetColor(ID_ColCre,  colorCrest);
        _mat.SetColor(ID_Foam,    foamColor);
        _mat.SetFloat(ID_FoamThr, foamThreshold);
        _mat.SetFloat(ID_Trans,   transparency);

        // Lighting
        _mat.SetColor(ID_Spec,  specularColor);
        _mat.SetFloat(ID_Shine, shininess);

        // Geometry
        _mat.SetFloat(ID_Disp, vertexDisplacementScale);
    }
}

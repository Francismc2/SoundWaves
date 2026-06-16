using UnityEngine;

/// <summary>
/// Attach to a Quad (scale ~10 x 4) at z = +5.
/// Accumulates photon hits as a glow texture.
/// The material should use an Unlit/Texture shader.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class DetectionScreen : MonoBehaviour
{
    [Header("Texture")]
    public int   textureWidth     = 1024;
    public int   textureHeight    = 256;
    public float screenWorldWidth = 10f;

    [Header("Hit Appearance")]
    [Range(1, 20)] public int   hitRadius      = 5;
    [Range(0f, 1f)] public float hitBrightness = 0.12f;
    public Color hitColor = new Color(0.3f, 0.6f, 1f);

    // ── private ───────────────────────────────────────────────────────────
    Texture2D _tex;
    Color[]   _pixels;
    Renderer  _rend;
    bool      _dirty;

    static readonly Color BG = new Color(0.02f, 0.02f, 0.06f, 1f);

    void Awake()
    {
        _rend   = GetComponent<Renderer>();
        _tex    = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
                  { filterMode = FilterMode.Bilinear };
        _pixels = new Color[textureWidth * textureHeight];
        Clear();
    }

    void LateUpdate()
    {
        if (!_dirty) return;
        _tex.SetPixels(_pixels);
        _tex.Apply();
        _dirty = false;
    }

    /// <summary>Called by TravelingParticle when it reaches the screen.</summary>
    public void RegisterHit(float worldX)
    {
        // Map world X → pixel X
        int px = Mathf.RoundToInt((worldX / screenWorldWidth + 0.5f) * textureWidth);
        px = Mathf.Clamp(px, 0, textureWidth - 1);

        int   cy    = textureHeight / 2;
        float sigma = hitRadius * 0.5f;

        for (int dy = -hitRadius * 2; dy <= hitRadius * 2; dy++)
        {
            int py = cy + dy;
            if (py < 0 || py >= textureHeight) continue;
            float yFade = Mathf.Exp(-(dy * dy) / (2f * sigma * sigma * 4f));

            for (int dx = -hitRadius; dx <= hitRadius; dx++)
            {
                int tx = Mathf.Clamp(px + dx, 0, textureWidth - 1);
                float xFade = Mathf.Exp(-(dx * dx) / (2f * sigma * sigma));

                int idx = py * textureWidth + tx;
                Color add = hitColor * (hitBrightness * xFade * yFade);
                _pixels[idx] = ClampColor(_pixels[idx] + add);
            }
        }
        _dirty = true;
    }

    public void Clear()
    {
        for (int i = 0; i < _pixels.Length; i++)
            _pixels[i] = BG;

        if (_tex != null)
        {
            _tex.SetPixels(_pixels);
            _tex.Apply();
            _rend.material.mainTexture = _tex;
        }
        _dirty = false;
    }

    Color ClampColor(Color c) =>
        new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), 1f);
}

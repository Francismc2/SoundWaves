using UnityEngine;

/// <summary>
/// Attach to a Quad at z = 0 that acts as the physical barrier.
/// Dynamically updates a texture to draw the slits based on manager settings.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class BarrierController : MonoBehaviour
{
    [Header("Texture")]
    public int textureWidth      = 512;
    public int textureHeight     = 256;
    public float sceneWorldWidth = 10f;   // must match DetectionScreen.screenWorldWidth

    [Header("Colors")]
    public Color barrierColor = new Color(0.15f, 0.15f, 0.2f,  1f);
    public Color slitColor    = new Color(0f,    0f,    0f,     0f);  // transparent gap

    DoubleSlitManager _mgr;
    Texture2D         _tex;
    Color[]           _pixels;
    Renderer          _rend;

    float _lastSep = -1f, _lastW = -1f;

    void Awake()
    {
        _mgr    = FindObjectOfType<DoubleSlitManager>();
        _rend   = GetComponent<Renderer>();
        _tex    = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
                  { filterMode = FilterMode.Point };
        _pixels = new Color[textureWidth * textureHeight];
        Redraw();
    }

    void Update()
    {
        if (!Mathf.Approximately(_mgr.slitSeparation, _lastSep) ||
            !Mathf.Approximately(_mgr.slitWidth,      _lastW))
            Redraw();
    }

    void Redraw()
    {
        _lastSep = _mgr.slitSeparation;
        _lastW   = _mgr.slitWidth;

        // Fill barrier
        for (int i = 0; i < _pixels.Length; i++)
            _pixels[i] = barrierColor;

        // Cut slits
        float pxPerUnit = textureWidth / sceneWorldWidth;

        float slit1CenterWorld =  _mgr.slitSeparation * 0.5f;
        float slit2CenterWorld = -_mgr.slitSeparation * 0.5f;

        CutSlit(slit1CenterWorld, pxPerUnit);
        CutSlit(slit2CenterWorld, pxPerUnit);

        _tex.SetPixels(_pixels);
        _tex.Apply();
        _rend.material.mainTexture = _tex;
    }

    void CutSlit(float centerWorld, float pxPerUnit)
    {
        int cx   = Mathf.RoundToInt((centerWorld / sceneWorldWidth + 0.5f) * textureWidth);
        int half = Mathf.Max(1, Mathf.RoundToInt(_mgr.slitWidth * 0.5f * pxPerUnit));

        for (int y = 0; y < textureHeight; y++)
        {
            for (int dx = -half; dx <= half; dx++)
            {
                int px = Mathf.Clamp(cx + dx, 0, textureWidth - 1);
                _pixels[y * textureWidth + px] = slitColor;
            }
        }
    }
}

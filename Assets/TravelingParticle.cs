using UnityEngine;

/// <summary>
/// Attach to the TravelingParticle prefab.
/// The prefab should be a small sphere with an emissive/unlit material.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class TravelingParticle : MonoBehaviour
{
    [Header("Travel Settings")]
    public float travelDuration = 0.7f;
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // ── state ─────────────────────────────────────────────────────────────
    Vector3          _start, _end;
    float            _elapsed;
    bool             _initialized;
    DetectionScreen  _screen;
    TrailRenderer    _trail;

    public void Initialize(Vector3 start, Vector3 end)
    {
        _start       = start;
        _end         = end;
        _initialized = true;
        _screen      = FindObjectOfType<DetectionScreen>();
        _trail       = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (!_initialized) return;

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / travelDuration);
        float curved = speedCurve.Evaluate(t);

        transform.position = Vector3.Lerp(_start, _end, curved);

        // Scale down as it approaches the screen (feels like "collapsing")
        float scale = Mathf.Lerp(1f, 0.1f, t);
        transform.localScale = Vector3.one * scale;

        if (t >= 1f)
        {
            _screen.RegisterHit(_end.x);

            // Detach trail so it fades on its own
            if (_trail != null)
            {
                _trail.transform.SetParent(null);
                Destroy(_trail.gameObject, _trail.time + 0.1f);
            }

            Destroy(gameObject);
        }
    }
}

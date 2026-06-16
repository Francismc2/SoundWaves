using UnityEngine;

/// <summary>
/// Attach to an empty GameObject at z = -5 (the emitter position).
/// Spawns TravelingParticle prefab instances toward the detection screen.
/// </summary>
public class ParticleEmitter : MonoBehaviour
{
    [Header("References")]
    public GameObject travelingParticlePrefab;  // small glowing sphere prefab
    public Transform  detectionScreenTransform;
    public Transform  barrierTransform;

    // Called every tick by DoubleSlitManager
    public void EmitParticle(bool observed, float slitWidth, float slitSeparation, float wavelength)
    {
        float screenZ  = detectionScreenTransform.position.z;
        float barrierZ = barrierTransform.position.z;
        float L        = screenZ - barrierZ;   // distance barrier → screen

        float screenX;

        if (observed)
        {
            // PARTICLE MODE
            // Photon travels through exactly one slit → two Gaussian clusters on screen
            float slitCenter = (Random.value > 0.5f ? 1f : -1f) * slitSeparation * 0.5f;
            screenX = slitCenter + GaussianRandom(0f, slitWidth * 0.6f);
        }
        else
        {
            // WAVE MODE
            // Sample from full double-slit interference probability distribution
            screenX = SampleInterference(slitSeparation, wavelength, slitWidth, L);
        }

        Vector3 target = new Vector3(screenX, 0f, screenZ);

        GameObject p = Instantiate(travelingParticlePrefab, transform.position, Quaternion.identity);
        p.GetComponent<TravelingParticle>().Initialize(transform.position, target);
    }

    // ── Physics ──────────────────────────────────────────────────────────

    /// <summary>
    /// Rejection-sample from the double-slit intensity function I(x).
    /// </summary>
    float SampleInterference(float d, float lambda, float w, float L, int maxAttempts = 200)
    {
        float maxX = 6f;
        for (int i = 0; i < maxAttempts; i++)
        {
            float x         = Random.Range(-maxX, maxX);
            float intensity = DoubleSlitIntensity(x, d, lambda, w, L);
            if (Random.value < intensity)
                return x;
        }
        return 0f;
    }

    /// <summary>
    /// I(x) = sinc²(β) · cos²(δ)
    /// β = π w sinθ / λ   (single-slit envelope)
    /// δ = π d sinθ / λ   (double-slit interference)
    /// </summary>
    float DoubleSlitIntensity(float x, float d, float lambda, float w, float L)
    {
        float sinTheta = x / Mathf.Sqrt(x * x + L * L);

        float beta  = Mathf.PI * w * sinTheta / lambda;
        float delta = Mathf.PI * d * sinTheta / lambda;

        float sinc      = Mathf.Abs(beta) < 1e-6f ? 1f : Mathf.Sin(beta) / beta;
        float envelope  = sinc * sinc;
        float fringes   = Mathf.Cos(delta) * Mathf.Cos(delta);

        return Mathf.Clamp01(envelope * fringes);
    }

    /// <summary>Box-Muller gaussian sample.</summary>
    float GaussianRandom(float mean, float sigma)
    {
        float u1 = 1f - Random.value;
        float u2 = 1f - Random.value;
        float rand = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
        return mean + sigma * rand;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Longitudinal Wave Simulation
/// 
/// Setup Instructions:
/// 1. Create an empty GameObject and attach this script.
/// 2. Optionally assign a Particle Prefab (a sphere with a Renderer) — 
///    if left null, the script auto-creates spheres.
/// 3. Press Play and watch the waves bounce back and forth!
/// 
/// Inspector Controls:
///  - Num Particles      : number of particles in the line
///  - Particle Spacing   : rest distance between particles
///  - Amplitude          : how far each particle displaces
///  - Wave Speed         : how fast the wave travels
///  - Frequency          : oscillation frequency
///  - Num Waves          : how many wave sources bounce simultaneously
///  - Show Compression   : color particles by compression (red=compress, blue=rarefaction)
/// </summary>
public class LongitudinalWaveSimulation : MonoBehaviour
{
    [Header("Particle Settings")]
    public GameObject particlePrefab;       // Optional: assign a sphere prefab
    public int numParticles = 60;
    public float particleSpacing = 0.4f;
    public float particleScale = 0.25f;

    [Header("Wave Parameters")]
    public float amplitude = 0.3f;
    [Range(0.5f, 10f)]
    public float waveSpeed = 3f;
    [Range(0.1f, 5f)]
    public float frequency = 1f;
    [Range(1, 4)]
    public int numWaves = 2;               // Standing wave formed by this many reflections

    [Header("Visuals")]
    public bool showCompression = true;
    public Color compressionColor = new Color(1f, 0.2f, 0.2f);   // Red
    public Color normalColor = new Color(0.8f, 0.8f, 0.8f);       // Grey
    public Color rarefactionColor = new Color(0.2f, 0.4f, 1f);    // Blue

    [Header("Bounds (auto-set)")]
    public float leftBound = 0f;
    public float rightBound = 0f;

    // Internals
    private GameObject[] particles;
    private Vector3[] restPositions;
    private Renderer[] renderers;
    private float totalLength;
    private float wavelength;
    private float time;

    void Start()
    {
        SpawnParticles();
    }

    void SpawnParticles()
    {
        // Clean up existing
        if (particles != null)
            foreach (var p in particles) if (p) Destroy(p);

        particles = new GameObject[numParticles];
        renderers = new Renderer[numParticles];
        restPositions = new Vector3[numParticles];

        totalLength = (numParticles - 1) * particleSpacing;
        leftBound = transform.position.x;
        rightBound = leftBound + totalLength;
        wavelength = totalLength / numWaves;

        for (int i = 0; i < numParticles; i++)
        {
            Vector3 pos = transform.position + new Vector3(i * particleSpacing, 0f, 0f);
            restPositions[i] = pos;

            GameObject p;
            if (particlePrefab != null)
                p = Instantiate(particlePrefab, pos, Quaternion.identity, transform);
            else
            {
                p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.transform.SetParent(transform);
                p.transform.position = pos;
                Destroy(p.GetComponent<SphereCollider>());
            }

            p.transform.localScale = Vector3.one * particleScale;
            p.name = $"Particle_{i}";
            particles[i] = p;
            renderers[i] = p.GetComponent<Renderer>();
        }
    }

    void Update()
    {
        time += Time.deltaTime;

        float k = (2f * Mathf.PI) / wavelength;       // wave number
        float omega = 2f * Mathf.PI * frequency;       // angular frequency

        for (int i = 0; i < numParticles; i++)
        {
            float x = restPositions[i].x - leftBound; // position along wave [0, totalLength]

            // Standing longitudinal wave = superposition of two counter-propagating waves
            // u(x,t) = A * sin(kx - wt) + A * sin(kx + wt)  =  2A * sin(kx) * cos(wt)
            float displacement = amplitude * Mathf.Sin(k * x) * Mathf.Cos(omega * time);

            // Move particle horizontally (longitudinal = along direction of propagation)
            Vector3 newPos = restPositions[i];
            newPos.x += displacement;
            particles[i].transform.position = newPos;

            // Compression coloring: compression = -d(displacement)/dx
            if (showCompression && renderers[i] != null)
            {
                float compression = -amplitude * k * Mathf.Cos(k * x) * Mathf.Cos(omega * time);
                float t = Mathf.InverseLerp(-amplitude * k, amplitude * k, compression);
                Color col = Color.Lerp(rarefactionColor, compressionColor, t);
                // Blend toward normal at midpoint
                col = Color.Lerp(normalColor, col, Mathf.Abs(t * 2f - 1f));
                renderers[i].material.color = col;
            }
        }
    }

    // ── Editor helper to preview bounds ──────────────────────────────────────
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        float len = (numParticles - 1) * particleSpacing;
        Vector3 left = transform.position;
        Vector3 right = transform.position + new Vector3(len, 0f, 0f);
        Gizmos.DrawLine(left + Vector3.up * 0.6f, left - Vector3.up * 0.6f);
        Gizmos.DrawLine(right + Vector3.up * 0.6f, right - Vector3.up * 0.6f);
        Gizmos.DrawLine(left, right);
    }

    // ── Public API so you can hook buttons / UI sliders ───────────────────────
    public void Rebuild() => SpawnParticles();
    public void SetFrequency(float f) { frequency = f; }
    public void SetAmplitude(float a) { amplitude = a; }
    public void SetWaveSpeed(float s) { waveSpeed = s; }
    public void SetNumWaves(float n) { numWaves = Mathf.RoundToInt(n); SpawnParticles(); }
}
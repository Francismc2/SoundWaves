using UnityEngine;
using System.Collections.Generic;

public class waves : MonoBehaviour
{
    [Header("Wave Settings")]
    public int rayCount = 64;
    public int maxBounce = 3;
    public float maxDistance = 50f;
    public float spreadAngle = 45f;
    public float waveSpeed = 10f;
    public LayerMask reflectionMask;

    [Header("Prefab Settings")]
    public GameObject waveSegmentPrefab; // Assign your prefab here

    [Header("Wave Tail Settings")]
    public float waveTailLength = 2f;

    [Header("Emission Settings")]
    public float emissionInterval = 5f;
    public bool autoEmit = true;
    private float nextEmissionTime = 0f;

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color gizmoRayColor = Color.yellow;
    public Color gizmoHitColor = Color.red;
    public float gizmoHitRadius = 0.2f;

    List<WaveSegment> segments = new List<WaveSegment>();
    List<LineRenderer> lines = new List<LineRenderer>();

    class WaveSegment
    {
        public Vector3 start;
        public Vector3 end;
        public float startTime;
        public float length;
        public bool isHit;
        public bool isComplete;
    }

    void Start()
    {
        // Check if prefab is assigned
        if (waveSegmentPrefab == null)
        {
            Debug.LogError("Wave Segment Prefab is not assigned! Please assign a prefab with a LineRenderer component.");
            return;
        }

        // Check if prefab has LineRenderer
        if (waveSegmentPrefab.GetComponent<LineRenderer>() == null)
        {
            Debug.LogError("The assigned prefab doesn't have a LineRenderer component!");
            return;
        }

        if (autoEmit)
        {
            GenerateWave();
            nextEmissionTime = Time.time + emissionInterval;
        }
    }

    void Update()
    {
        if (autoEmit && Time.time >= nextEmissionTime)
        {
            GenerateWave();
            nextEmissionTime = Time.time + emissionInterval;
        }

        AnimateWave();
        CleanupCompletedWaves();
    }

    public void GenerateWave()
    {
        if (waveSegmentPrefab == null)
            return;

        float birthTime = Time.time;

        for (int i = 0; i < rayCount; i++)
        {
            float t = (float)i / rayCount;
            Vector3 direction = Quaternion.Euler(
                Mathf.Lerp(-spreadAngle, spreadAngle, t),
                Mathf.Lerp(-spreadAngle, spreadAngle, 1 - t),
                0
            ) * transform.forward;
            TraceRay(transform.position, direction, 0, birthTime);
        }
    }

    void TraceRay(Vector3 position, Vector3 direction, int bounce, float birthTime)
    {
        if (bounce > maxBounce)
            return;

        if (Physics.Raycast(position, direction, out RaycastHit hit,
            maxDistance, reflectionMask, QueryTriggerInteraction.Ignore))
        {
            AddSegment(position, hit.point, birthTime, true);
            Vector3 reflectDir = Vector3.Reflect(direction, hit.normal);
            TraceRay(
                hit.point + reflectDir * 0.01f,
                reflectDir,
                bounce + 1,
                birthTime);
        }
        else
        {
            AddSegment(position, position + direction * maxDistance, birthTime, false);
        }
    }

    void AddSegment(Vector3 a, Vector3 b, float time, bool isHit)
    {
        // Instantiate the prefab
        GameObject go = Instantiate(waveSegmentPrefab, transform);
        go.name = "WaveSegment";

        // Get the LineRenderer from the instantiated prefab
        LineRenderer lr = go.GetComponent<LineRenderer>();

        if (lr == null)
        {
            Debug.LogError("Instantiated prefab doesn't have a LineRenderer!");
            Destroy(go);
            return;
        }

        // Ensure it has 2 positions
        lr.positionCount = 2;

        segments.Add(new WaveSegment
        {
            start = a,
            end = b,
            startTime = time,
            length = Vector3.Distance(a, b),
            isHit = isHit,
            isComplete = false
        });
        lines.Add(lr);
    }

    void AnimateWave()
    {
        float currentTime = Time.time;

        for (int i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            var lr = lines[i];
            if (lr == null)
                continue;

            float travelled = (currentTime - seg.startTime) * waveSpeed;

            // Calculate the front of the wave
            float frontDistance = travelled;
            float frontT = Mathf.Clamp01(frontDistance / seg.length);
            Vector3 frontPos = Vector3.Lerp(seg.start, seg.end, frontT);

            // Calculate the back of the wave (the tail)
            float backDistance = travelled - waveTailLength;
            float backT = Mathf.Clamp01(backDistance / seg.length);
            Vector3 backPos = Vector3.Lerp(seg.start, seg.end, backT);

            // Set the line to only show the tail portion
            lr.SetPosition(0, backPos);
            lr.SetPosition(1, frontPos);

            // Mark as complete when the back of the wave reaches the end
            if (backT >= 1f)
            {
                seg.isComplete = true;
            }
        }
    }

    void CleanupCompletedWaves()
    {
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (segments[i].isComplete)
            {
                if (lines[i] != null)
                    Destroy(lines[i].gameObject);

                segments.RemoveAt(i);
                lines.RemoveAt(i);
            }
        }
    }

    void ClearWave()
    {
        foreach (var l in lines)
        {
            if (l != null)
                Destroy(l.gameObject);
        }
        segments.Clear();
        lines.Clear();
    }

    [ContextMenu("Emit Wave Now")]
    public void EmitWaveManually()
    {
        GenerateWave();
    }

    void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        foreach (var seg in segments)
        {
            Gizmos.color = gizmoRayColor;
            Gizmos.DrawLine(seg.start, seg.end);

            if (seg.isHit)
            {
                Gizmos.color = gizmoHitColor;
                Gizmos.DrawSphere(seg.end, gizmoHitRadius);
            }
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
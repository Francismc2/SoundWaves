using UnityEngine;

public class DoubleSlitExperiment : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform detectorScreen;
    [SerializeField] private Transform particleContainer;
    [SerializeField] private GameObject impactPrefab;

    [Header("Experiment")]
    [SerializeField] private float wavelength = 0.5f;
    [SerializeField] private float slitDistance = 0.5f;
    [SerializeField] private float slitWidth = 0.1f;

    [Header("Emission")]
    [SerializeField] private float particlesPerSecond = 50f;

    [SerializeField] private bool detectorEnabled = false;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 1f / particlesPerSecond)
        {
            timer = 0f;
            FireParticle();
        }
    }

    private void FireParticle()
    {
        float y = detectorEnabled
            ? SampleClassical()
            : SampleInterference();

        Vector3 hitPosition =
            detectorScreen.position +
            detectorScreen.up * y;

        Instantiate(
            impactPrefab,
            hitPosition,
            Quaternion.identity,
            particleContainer);
    }

    private float SampleInterference()
    {
        while (true)
        {
            float y = Random.Range(-3f, 3f);

            if (Random.value < Intensity(y))
                return y;
        }
    }

    private float SampleClassical()
    {
        if (Random.value < 0.5f)
            return Random.Range(-1.5f, -0.5f);

        return Random.Range(0.5f, 1.5f);
    }

    private float Intensity(float y)
    {
        float distanceToScreen = 7f;

        float theta = Mathf.Atan(y / distanceToScreen);

        float beta =
            Mathf.PI * slitWidth * Mathf.Sin(theta) / wavelength;

        float alpha =
            Mathf.PI * slitDistance * Mathf.Sin(theta) / wavelength;

        float diffraction =
            Mathf.Abs(beta) < 0.0001f
                ? 1f
                : Mathf.Pow(Mathf.Sin(beta) / beta, 2f);

        float interference =
            Mathf.Pow(Mathf.Cos(alpha), 2f);

        return diffraction * interference;
    }

    [ContextMenu("Clear Detector")]
    private void ClearDetector()
    {
        foreach (Transform child in particleContainer)
        {
            DestroyImmediate(child.gameObject);
        }
    }
}
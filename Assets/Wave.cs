using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshWave : MonoBehaviour
{
    Mesh mesh;
    Vector3[] baseVertices;
    Vector3[] vertices;

    [Header("Wave Settings")]
    public float amplitude = 0.5f;
    public float frequency = 1f;
    public float wavelength = 2f;
    public float speed = 1f;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
        vertices = new Vector3[baseVertices.Length];
    }

    void Update()
    {
        float time = Time.time * speed;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = baseVertices[i];

            float wave =
                amplitude *
                Mathf.Sin((v.x / wavelength) + (frequency * time));

            v.y = wave;
            vertices[i] = v;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
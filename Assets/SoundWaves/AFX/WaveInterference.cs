using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaveInterference : MonoBehaviour
{
    [Header("Mesh")]
    public int   resolution = 80;
    public float planeSize  = 10f;
    public Material surfaceMaterial;

    [Header("Wave A")]
    public float amplitudeA = 1f;
    public float frequencyA = 1f;
    public float speedA     = 5f;
    public float sourceAX   = -4f;

    [Header("Wave B")]
    public float amplitudeB = 1f;
    public float frequencyB = 1f;
    public float speedB     = 5f;
    public float sourceBX   = 4f;

    Mesh      _mesh;
    Vector3[] _baseVerts;
    Vector3[] _verts;
    float     _time;

    void Start()
    {
        BuildMesh();
        if (surfaceMaterial != null)
            GetComponent<MeshRenderer>().material = surfaceMaterial;
    }

    void Update()
    {
        _time += Time.deltaTime;
        AnimateMesh();
    }

    void BuildMesh()
    {
        _mesh = new Mesh { name = "WaveInterference" };
        _mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = _mesh;

        int     vCount = resolution * resolution;
        _baseVerts     = new Vector3[vCount];
        _verts         = new Vector3[vCount];
        Vector2[] uvs  = new Vector2[vCount];
        int[] tris     = new int[(resolution - 1) * (resolution - 1) * 6];

        float step = planeSize / (resolution - 1);
        float half = planeSize * 0.5f;

        for (int z = 0; z < resolution; z++)
        for (int x = 0; x < resolution; x++)
        {
            int i = z * resolution + x;
            _baseVerts[i] = new Vector3(x * step - half, 0f, z * step - half);
            uvs[i]        = new Vector2((float)x / (resolution - 1),
                                        (float)z / (resolution - 1));
        }

        int t = 0;
        for (int z = 0; z < resolution - 1; z++)
        for (int x = 0; x < resolution - 1; x++)
        {
            int i = z * resolution + x;
            tris[t++] = i;              tris[t++] = i + resolution;
            tris[t++] = i + 1;          tris[t++] = i + 1;
            tris[t++] = i + resolution; tris[t++] = i + resolution + 1;
        }

        _mesh.vertices  = _baseVerts;
        _mesh.uv        = uvs;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();
    }

    void AnimateMesh()
    {
        float wA = 2f * Mathf.PI * frequencyA;
        float wB = 2f * Mathf.PI * frequencyB;
        float kA = wA / speedA;
        float kB = wB / speedB;

        for (int i = 0; i < _baseVerts.Length; i++)
        {
            float x = _baseVerts[i].x;
            float z = _baseVerts[i].z;

            // Circular ripple from source A
            float rA = Mathf.Sqrt((x - sourceAX) * (x - sourceAX) + z * z);
            float yA = amplitudeA * Mathf.Sin(kA * rA - wA * _time)
                     / Mathf.Max(1f, rA * 0.4f);

            // Circular ripple from source B
            float rB = Mathf.Sqrt((x - sourceBX) * (x - sourceBX) + z * z);
            float yB = amplitudeB * Mathf.Sin(kB * rB - wB * _time)
                     / Mathf.Max(1f, rB * 0.4f);

            // Superposition
            _verts[i] = new Vector3(x, yA + yB, z);
        }

        _mesh.vertices = _verts;
        _mesh.RecalculateNormals();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + new Vector3(sourceAX, 0f, 0f), 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + new Vector3(sourceBX, 0f, 0f), 0.2f);
    }
}

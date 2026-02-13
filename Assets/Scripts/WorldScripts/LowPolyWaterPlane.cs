using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LowpolyWaterPlane : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    
    [Header("Water Settings")]
    public float waveSpeed = 1f;
    public float waveHeight = 0.1f;
    public float waveFrequency = 1f;
    
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] originalVertices;
    
    void Start()
    {
        GenerateLowpolyMesh();
    }
    
    void Update()
    {
        AnimateWater();
    }
    
    void GenerateLowpolyMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        
        // Создаем вершины для лоуполи сетки
        vertices = new Vector3[width * height];
        Vector2[] uv = new Vector2[vertices.Length];
        
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // Добавляем небольшую случайность для лоуполи вида
                float randomOffset = Random.Range(-0.1f, 0.1f);
                vertices[z * width + x] = new Vector3(
                    x * cellSize + randomOffset, 
                    0, 
                    z * cellSize + randomOffset
                );
                
                uv[z * width + x] = new Vector2((float)x / width, (float)z / height);
            }
        }
        
        // Создаем треугольники (каждые 4 вершины = 2 треугольника)
        int[] triangles = new int[(width - 1) * (height - 1) * 6];
        int triIndex = 0;
        
        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int bottomLeft = z * width + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = (z + 1) * width + x;
                int topRight = topLeft + 1;
                
                // Первый треугольник
                triangles[triIndex] = bottomLeft;
                triangles[triIndex + 1] = topLeft;
                triangles[triIndex + 2] = bottomRight;
                
                // Второй треугольник
                triangles[triIndex + 3] = bottomRight;
                triangles[triIndex + 4] = topLeft;
                triangles[triIndex + 5] = topRight;
                
                triIndex += 6;
            }
        }
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        // Сохраняем оригинальные вершины для анимации
        originalVertices = (Vector3[])vertices.Clone();
    }
    
    void AnimateWater()
    {
        Vector3[] currentVertices = new Vector3[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            
            // Простая волновая анимация
            float wave = Mathf.Sin(vertex.x * waveFrequency + Time.time * waveSpeed) *
                        Mathf.Sin(vertex.z * waveFrequency + Time.time * waveSpeed) *
                        waveHeight;
            
            currentVertices[i] = new Vector3(vertex.x, vertex.y + wave, vertex.z);
        }
        
        mesh.vertices = currentVertices;
        mesh.RecalculateNormals();
    }
}
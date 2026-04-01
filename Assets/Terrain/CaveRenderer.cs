using System.Collections.Generic;
using UnityEngine;

public class CaveRenderer : MonoBehaviour
{
    [Header("Rendering")]
    public Material caveMaterial; // Drag a cave/stone material here in the Inspector.
    public float textureScale = 0.1f; // How many times the texture tiles across the cave.

    // Generates a smooth cave mesh using the Marching Cubes algorithm.
    public void RenderCave(bool[,,] grid, int width, int height, int depth, int cellSize)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        // Vertex welding map: reuse any vertex that already sits at the same position,
        // so adjacent cubes share edge vertices and the mesh is watertight (no gaps).
        Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>();

        // Loop over every cube defined by 8 neighbouring grid corners.
        for(int x = 0; x < width - 1; x++)
        {
            for(int y = 0; y < height - 1; y++)
            {
                for(int z = 0; z < depth - 1; z++)
                {
                    MarchCube(grid, x, y, z, cellSize, vertices, triangles, vertexMap);
                }
            }
        }

        // Build the mesh from the collected vertices and triangles.
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Supports large caves.
        // Generate UVs from world position so the rock texture is visible.
        // Each surface normal decides which plane (XZ, XY, or YZ) to project onto,
        // then the results are blended — this is called triplanar mapping.
        Vector3[] verts = vertices.ToArray();
        mesh.vertices = verts;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        Vector3[] normals = mesh.normals;
        Vector2[] uvs = new Vector2[verts.Length];
        for(int i = 0; i < verts.Length; i++)
        {
            // Blend weight for each projection axis based on how much the normal faces that way.
            Vector3 n = new Vector3(Mathf.Abs(normals[i].x), Mathf.Abs(normals[i].y), Mathf.Abs(normals[i].z));
            float total = n.x + n.y + n.z + 0.0001f; // avoid divide-by-zero
            n /= total;

            Vector2 uvXZ = new Vector2(verts[i].x, verts[i].z) * textureScale; // floor/ceiling
            Vector2 uvXY = new Vector2(verts[i].x, verts[i].y) * textureScale; // front/back wall
            Vector2 uvYZ = new Vector2(verts[i].z, verts[i].y) * textureScale; // left/right wall

            uvs[i] = uvXZ * n.y + uvXY * n.z + uvYZ * n.x;
        }

        mesh.uv = uvs;

        // Attach mesh to a new GameObject.
        GameObject caveObj = new GameObject("Cave");
        caveObj.transform.position = this.transform.position;
        MeshFilter mf = caveObj.AddComponent<MeshFilter>();
        MeshRenderer mr = caveObj.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = caveMaterial != null ? caveMaterial : new Material(Shader.Find("Standard"));

        // Give the cave a subtle self-illumination so the interior is always visible.
        mr.material.EnableKeyword("_EMISSION");
        mr.material.SetColor("_EmissionColor", new Color(0.08f, 0.07f, 0.06f));

        // Add a mesh collider so the player can walk inside.
        MeshCollider mc = caveObj.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
    }

    // Processes a single cube at grid position (x, y, z).
    private void MarchCube(bool[,,] grid, int x, int y, int z, int cellSize,
                           List<Vector3> vertices, List<int> triangles,
                           Dictionary<Vector3, int> vertexMap)
    {
        // The 8 corners of this cube, in Marching Cubes winding order.
        Vector3Int[] corners = new Vector3Int[8]
        {
            new Vector3Int(x,     y,     z    ),
            new Vector3Int(x + 1, y,     z    ),
            new Vector3Int(x + 1, y,     z + 1),
            new Vector3Int(x,     y,     z + 1),
            new Vector3Int(x,     y + 1, z    ),
            new Vector3Int(x + 1, y + 1, z    ),
            new Vector3Int(x + 1, y + 1, z + 1),
            new Vector3Int(x,     y + 1, z + 1)
        };

        // Build the 8-bit index: each bit = 1 if that corner is ROCK (solid), 0 if AIR.
        int cubeIndex = 0;
        for(int i = 0; i < 8; i++)
        {
            if(grid[corners[i].x, corners[i].y, corners[i].z])
                cubeIndex |= (1 << i);
        }

        // Fully air (0) or fully solid (255) — no surface here.
        if(cubeIndex == 0 || cubeIndex == 255) return;
        if(cubeIndex >= MarchingCubesTables.TriTable.GetLength(0)) return;

        // The 12 possible edge midpoints for this cube.
        Vector3[] edgeVertices = new Vector3[12];
        edgeVertices[0]  = EdgeMidpoint(corners[0], corners[1], cellSize);
        edgeVertices[1]  = EdgeMidpoint(corners[1], corners[2], cellSize);
        edgeVertices[2]  = EdgeMidpoint(corners[2], corners[3], cellSize);
        edgeVertices[3]  = EdgeMidpoint(corners[3], corners[0], cellSize);
        edgeVertices[4]  = EdgeMidpoint(corners[4], corners[5], cellSize);
        edgeVertices[5]  = EdgeMidpoint(corners[5], corners[6], cellSize);
        edgeVertices[6]  = EdgeMidpoint(corners[6], corners[7], cellSize);
        edgeVertices[7]  = EdgeMidpoint(corners[7], corners[4], cellSize);
        edgeVertices[8]  = EdgeMidpoint(corners[0], corners[4], cellSize);
        edgeVertices[9]  = EdgeMidpoint(corners[1], corners[5], cellSize);
        edgeVertices[10] = EdgeMidpoint(corners[2], corners[6], cellSize);
        edgeVertices[11] = EdgeMidpoint(corners[3], corners[7], cellSize);

        // Add triangles from the triangle table — loop until the -1 sentinel.
        for(int i = 0; i < 16; i += 3)
        {
            if(MarchingCubesTables.TriTable[cubeIndex, i] == -1) break;

            for(int j = 0; j < 3; j++)
            {
                Vector3 v = edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i + j]];
                if(!vertexMap.TryGetValue(v, out int idx))
                {
                    idx = vertices.Count;
                    vertices.Add(v);
                    vertexMap[v] = idx;
                }
                triangles.Add(idx);
            }
        }
    }

    // Returns the world-space midpoint of the edge between two grid corners.
    private Vector3 EdgeMidpoint(Vector3Int a, Vector3Int b, int cellSize)
    {
        Vector3 worldA = new Vector3(a.x * cellSize, -a.y * cellSize, a.z * cellSize);
        Vector3 worldB = new Vector3(b.x * cellSize, -b.y * cellSize, b.z * cellSize);
        return (worldA + worldB) * 0.5f;
    }
}

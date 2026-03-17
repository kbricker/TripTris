using UnityEngine;

namespace HexTris
{
    public static class HexMesh
    {
        // Creates a hex GameObject with fill mesh and outline child
        public static GameObject CreateHex(float size, Material fillMaterial, Color outlineColor)
        {
            GameObject hex = new GameObject("Hex");

            // Outline: slightly larger dark hex behind
            GameObject outline = CreateHexMeshObject("Outline", size * 1.0f, outlineColor);
            outline.transform.SetParent(hex.transform, false);
            outline.transform.localPosition = new Vector3(0, 0, 0.01f); // slightly behind

            // Fill: the colored hex (use fill material directly if provided)
            GameObject fill = CreateHexMeshObject("Fill", size * 0.88f, Color.white, fillMaterial);
            fill.transform.SetParent(hex.transform, false);

            return hex;
        }

        // Creates a flat hex mesh GameObject with MeshFilter + MeshRenderer
        private static GameObject CreateHexMeshObject(string name, float size, Color color, Material overrideMaterial = null)
        {
            GameObject obj = new GameObject(name);
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();

            mf.mesh = GenerateHexMesh(size);

            if (overrideMaterial != null)
            {
                mr.material = overrideMaterial;
            }
            else
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = color;
                mr.material = mat;
            }
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            return obj;
        }

        // Generate a pointy-top hexagon mesh
        // Vertices: center + 6 corners, starting from top (90 degrees)
        public static Mesh GenerateHexMesh(float size)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[7];
            vertices[0] = Vector3.zero; // center

            for (int i = 0; i < 6; i++)
            {
                // Pointy-top: first vertex at 90 degrees (top), going clockwise
                float angle = (90f - 60f * i) * Mathf.Deg2Rad;
                vertices[i + 1] = new Vector3(size * Mathf.Cos(angle), size * Mathf.Sin(angle), 0f);
            }

            int[] triangles = new int[18]; // 6 triangles * 3 vertices
            for (int i = 0; i < 6; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i < 5) ? i + 2 : 1;
            }

            Vector3[] normals = new Vector3[7];
            for (int i = 0; i < 7; i++)
                normals[i] = -Vector3.forward;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;

            return mesh;
        }
    }
}

using System;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteRenderer3D : MonoBehaviour
{
    [Header("Assign a Sprite")]
    public Sprite baseMap;
    public Color baseColor;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    private static int index = 0;
    private Vector3[] currentQuadVertices = new Vector3[4];
    private static readonly Vector2[] originalQuadUVs = new Vector2[]
    {
        new Vector2(0f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 0f),
        new Vector2(1f, 1f)
    };

    private static readonly Vector3[] originalQuadVertices = new Vector3[]
    {
        new Vector3(-0.5f, -0.5f, 0f),
        new Vector3(-0.5f, 0.5f, 0f),
        new Vector3(0.5f, -0.5f, 0f),
        new Vector3(0.5f, 0.5f, 0f)
    };
    private static readonly int[] originalQuadTriangles = new int[]
    {
        0,
        1,
        2,
        2,
        1,
        3
    };
    private Vector2 size;
    private Vector2 pivot;
    private Vector2 center;
    private float pixelsPerUnit;

    private MaterialPropertyBlock materialPropertyBlock;

    private void OnEnable()
    {
        Initialize();
        UpdatePropertyBlock();
    }
    private void Initialize()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter && Application.isEditor && !Application.isPlaying)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshRenderer = GetComponent<MeshRenderer>();
        if (!meshRenderer && Application.isEditor && !Application.isPlaying)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        materialPropertyBlock = new MaterialPropertyBlock();
        originalQuadVertices.CopyTo(currentQuadVertices, 0);
        mesh = new Mesh();
        mesh.name = "SpriteMesh (Instance) " + index.ToString();
        index++;
        mesh.vertices = currentQuadVertices;
        mesh.triangles = originalQuadTriangles;
        mesh.uv = originalQuadUVs;
        mesh.MarkDynamic();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }

    public void UpdatePropertyBlock()
    {
        if (meshRenderer)
        {
            meshRenderer.GetPropertyBlock(materialPropertyBlock);
            SetTexture("_BaseMap", baseMap);
            materialPropertyBlock.SetColor("_BaseColor", baseColor);
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
    private void SetTexture(string name, Sprite textureSource)
    {
        if (textureSource != null)
        {
            materialPropertyBlock.SetTexture(name, textureSource.texture);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.3f, 0f);
        Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation, transform.localScale);
    }
    private void LateUpdate()
    {
        if ((meshRenderer == null || materialPropertyBlock == null || mesh == null)
            && Application.isEditor && !Application.isPlaying)
        {
            Initialize();
        }

        if (baseMap != null)
        {
            UpdateMeshForSprite();
        }

        UpdatePropertyBlock();
    }

    private void UpdateMeshForSprite()
    {
        // Get sprite dimensions and properties
        if (baseMap != null)
        {
            Rect spriteRect = baseMap.rect; // Pixel dimensions of the sprite
            pixelsPerUnit = baseMap.pixelsPerUnit;
            size = new Vector2(spriteRect.width / pixelsPerUnit, spriteRect.height / pixelsPerUnit);
            pivot = baseMap.pivot / pixelsPerUnit;

            // Center offset based on pivot
            center = new Vector2(size.x * 0.5f - pivot.x, size.y * 0.5f - pivot.y);

            // Update vertices
            currentQuadVertices[0] = new Vector3(-size.x * 0.5f + center.x, -size.y * 0.5f + center.y, 0f);
            currentQuadVertices[1] = new Vector3(-size.x * 0.5f + center.x, size.y * 0.5f + center.y, 0f);
            currentQuadVertices[2] = new Vector3(size.x * 0.5f + center.x, -size.y * 0.5f + center.y, 0f);
            currentQuadVertices[3] = new Vector3(size.x * 0.5f + center.x, size.y * 0.5f + center.y, 0f);

            mesh.vertices = currentQuadVertices;

            // Update UVs to match sprite texture
            Vector2 textureSize = new Vector2(baseMap.texture.width, baseMap.texture.height);
            Vector2 uvMin = spriteRect.min / textureSize;
            Vector2 uvMax = spriteRect.max / textureSize;

            mesh.uv = new Vector2[]
            {
            new Vector2(uvMin.x, uvMin.y),
            new Vector2(uvMin.x, uvMax.y),
            new Vector2(uvMax.x, uvMin.y),
            new Vector2(uvMax.x, uvMax.y)
            };

            mesh.RecalculateBounds();
            mesh.MarkDynamic();
        }
    }
}

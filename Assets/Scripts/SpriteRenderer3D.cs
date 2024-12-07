using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class SpriteRenderer3D : MonoBehaviour
{
    [Header("Assign a Sprite")]
    public Sprite sprite;

    private Renderer _renderer;
    private Material _material;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
            _material = _renderer.sharedMaterial;
    }

    void Update()
    {
        if (sprite != null && _material != null)
        {
            sprite.texture.alphaIsTransparency = true;

            _material.SetTexture("_BaseMap", sprite.texture);

            Vector4 uvData = new Vector4(
                sprite.textureRect.x / sprite.texture.width,      // U Offset
                sprite.textureRect.y / sprite.texture.height,     // V Offset
                sprite.textureRect.width / sprite.texture.width,  // U Scale
                sprite.textureRect.height / sprite.texture.height // V Scale
            );
            _material.SetVector("_BaseMapUV", uvData);
        }
    }
}

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
        // Ensure the Renderer and Material references are set
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
            _material = _renderer.sharedMaterial;
    }

    void Update()
    {
        if (sprite != null && _material != null)
        {
            // Assign the sprite texture to the shader
            _material.SetTexture("_BaseMap", sprite.texture);
        }
    }
}

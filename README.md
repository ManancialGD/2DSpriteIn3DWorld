# Integration of a 2D Sprite in a 3D world
---
## Objectives:
- Billboarding.
- Y Billboarding.
- Custom shadowmaps.
- Custom how sprite is lit.
---
# Billboarding
## What is billboarding?
Billboarding is the most common way of integrating sprites in a 3D World. Billboarding is taking a quad and make it always face the camera.

So, how can we do that? Whell, we could just simply make the quad face the camera by using the `LookAt` method from Unity, like this:

```csharp
private void Update()
{
  transform.LookAt(Camera.main.transform);
  transform.Rotate(0, 180, 0);
}
```
But now we have some problems like, what if the camera isn't the main camera?

Not just the cameras, but if we get close to the object this happens:

![NotBillboard](Images/notbillboard.png)

The quad isn't actually facing the camera like a sprite would, this is actually just **rotating** towards the camera position.

Instead, we can solve this problem by using a shader. In the vertex shader, we can use the following code:

```shader
            v2f vert (appdata v)
            {
                v2f o;
                float4 origin = float4(0,0,0,1);
                float4 world_origin = mul(UNITY_MATRIX_M, origin);
                float4 view_origin = mul(UNITY_MATRIX_V, world_origin);
                float4 world_to_view_translation = view_origin - world_origin;
                
                float4 world_pos = mul(UNITY_MATRIX_M, v.vertex);
                float4 view_pos = world_pos + world_to_view_translation;
                float4 clip_pos = mul(UNITY_MATRIX_P, view_pos);

                o.vertex = clip_pos;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
```

This shader fixes the issues by transforming the quad's vertices to always align with the camera's perspective. It calculates the quad's origin in model space and shifts the vertices in view space based on the camera's position. This ensures accurate alignment regardless of proximity or camera type, avoiding the perspective distortion that occurs with LookAt. By using the model, view, and projection matrices directly, the quad behaves like a proper 2D sprite integrated into the 3D world.

Now we get this:

![ThisIsBillboarding](Images/thisisbillboarding.png)

---
## References

Character art: The Adventurer - Male. sScary. Available at: https://sscary.itch.io/the-adventurer-male

"What is billboarding and can/should it be used in 3D games to create special effects?" Game Development Stack Exchange. Available at: https://gamedev.stackexchange.com/questions/54871


# Integration of a 2D Sprite in a 3D World

## Objectives:
- Billboarding.
- Y Billboarding.
- Custom shadowmaps.

---

# Billboarding
## What is billboarding?
Billboarding is the most common way of integrating sprites in a 3D world. Billboarding involves taking a quad and making it always face the camera.

So, how can we do that? Well, we could simply make the quad face the camera by using the `LookAt` method from Unity, like this:

```csharp
private void Update()
{
  transform.LookAt(Camera.main.transform);
  transform.Rotate(0, 180, 0);
}
```
But now we have some problems, like what if the camera isn't the main camera?

Not just the cameras, but if we get close to the object, this happens:

![NotBillboard](Images/notbillboard.png)

The quad isn't actually facing the camera like a sprite would, it is just **rotating** towards the camera position.

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
## Y Billboarding

I searched a lot on the internet but didn't find how to make Y Billboarding in a vertex shader...

I tried applying matrices in all axis but the y, didn't work.

Only a shader for GameMaker that I couldn't translate to Unity HLSL.
[Billboarding Shaders - 3D Games in GameMaker](https://www.youtube.com/watch?v=gMKMRkZzR9M&ab_channel=DragoniteSpam)

---

## Shadowcasting

So, now we can add a character to our plane, and a spotlight.

![WithoutShadow](Images/WhithoutShadow.png)

To add shadows, we can add another pass to our shader.
And also, add these tags:

```
Pass
{
  Name "ShadowCaster"
  Tags { "Queue"="Transparent" "LightMode"="ShadowCaster" }
  Blend SrcAlpha OneMinusSrcAlpha
  ZWrite On
  CGPROGRAM
```

Adding a simple vertex and fragment shader to it, we get this:

![WithShadow](Images/WithShadow.png)

But now we have a problem. If the light is 90° from the plane, we get this:

![WithoutShadowBillboarding](Images/WhithoutShadowBillboarding.png)

The shadow is just a line. But it makes sense; only the rendering to the camera is billboarding. We also need to add the billboarding to the vertex shader. And we get this:

![WithShaderBillboarding](Images/WithShaderBillboarding.png)

But now we have another problem. This method of billboarding for the shadows works fine for *Directional Light* and *Spot Light*. But when we use point light, this happens:

![PointLightProblem](Images/PointLightProblem.png)
![PointLightProblem2](Images/PointLightProblem2.png)

This actually makes sense when you consider how point lights render shadows.
See, a point light emits light in all directions, so it needs to render shadows for every direction as well. However, attempting to render shadows in all directions directly would be inefficient and computationally expensive. Instead, point light shadows are rendered using a cube map. This involves rendering the scene six times—once for each face of the cube—capturing the depth information from the light's perspective in every direction.

Since we are billboarding to the direction the light is rendering, and point light has 6, this is what happens.

Solution? I didn't actually find a solution to this...

### Alpha Texture based Shadow.

Now we want to make the shadow, not a quad, but follow the alpha on the texture.
We can do that by returning the alpha in the fragment shader, like this:

```
float frag(vertexOutput i) : SV_Target
{
  float4 texColor = tex2D(_BaseMap, i.uv);
        
  clip(texColor.a - 0.5);
  return texColor.a;
}
```

Now, the shadow looks like this:

![TextureBasedShadow](Images/TextureBasedShadow.png)

But we also have another problem.
At certain angles, we start to expect the shadow to be flipped, like this:

![ShadowUnflipped](Images/ShadowUnflipped.png)

We can see the character's backpack on the left, but in the shadow, it's on the right.

### Solution:
Based on the dot product of the light-to-object vector with the forward vector, we can check if the dot product is greater than 0 and flip the shadow.

This is all in theory; I couldn't implement this in practice.

---

## References

Character art: The Adventurer - Male. sScary. Available at: https://sscary.itch.io/the-adventurer-male

"What is billboarding and can/should it be used in 3D games to create special effects?" Game Development Stack Exchange. Available at: https://gamedev.stackexchange.com/questions/54871


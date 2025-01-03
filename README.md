# Integration of a 2D Sprite in a 3D World

## Summary
This report covers the integration of 2D sprites into a 3D world using various techniques such as billboarding, custom shadowmaps, and lighting. It includes detailed explanations, shader code examples, and visual results.

## Objectives:
- Billboarding.
- Y Billboarding.
- Custom shadowmaps.
- Lighting.

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

I attempted to implement Y Billboarding in Unity but encountered challenges in keeping the sprite upright while making it face the camera.
Despite trying various matrix manipulations, I couldn't achieve the desired effect.

I found this shader for GameMaker that accomplished this, but I struggled to translate it to Unity's HLSL. Here: [Billboarding Shaders - 3D Games in GameMaker](https://www.youtube.com/watch?v=gMKMRkZzR9M&ab_channel=DragoniteSpam)

For some reason, for the duration of this project, I found hard to use the camera in HLSL using the `_WorldSpaceCameraPos` varriable

---

## Shadowcasting

So, now we can add a character to our plane, and a spotlight.

![WithoutShadow](Images/WhithoutShadow.png)

To add shadows, we can add another pass to our shader.
And also, add these tags:

```cs
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


We also have this issue, the shadow is clipping into the wall.

![ShadowClip](Images/ShadowClipping.png)

---

## Lighting

To make our character respond to lighting in the scene, we need to calculate the lighting based on the normals of our vertices and the light's position and color.

### Adding Normals

First, we need to include the normal data in our vertex input and output structures. This allows us to calculate how light interacts with the surface of our sprite.

```cs
struct appdata_t
{
  float4 vertex : POSITION;
  float2 uv : TEXCOORD0;
  float3 normal : NORMAL; // Add normal data
};

struct v2f
{
  float2 uv : TEXCOORD0;
  float4 vertex : SV_POSITION;
  half3 worldNormal : NORMAL; // Add normal data
};
```

### Directional Light

Next, we need to calculate the lighting based on the direction of the light. We use the `_WorldSpaceLightPos0` variable to get the light's position.

```cs
float ComputeLighting(float3 normal)
{
  float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
  float NdotL = dot(normal, lightDir);
  return NdotL;
}
```

This function calculates the dot product of the normal and the light direction, giving us the intensity of the light on the surface.

### Handling Light from Different Angles

To avoid the character becoming completely black when the light hits from the side or behind, we can adjust the lighting calculation to ensure some light is always present.

```cs
float ComputeLighting(float3 normal)
{
  float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
  float realDot = dot(normal, lightDir);
  float NdotL = abs(realDot);

  if (realDot <= 0)  // Light comes from behind
  {
    NdotL += dot(normal, lightDir) * 0.8; // Darker than the front
  }

  NdotL += 0.1; // Ensure it's never pitch black
  return NdotL;
}
```

But we have a huge issue.
Let's say that the directional light is hitting it from the front.
When we rotate the camera 90°.
The light is hitting it from the side, but since we did not really rotate the sprite,
it's computing as it's hitting from the front.

to fix this, the normal should be the vector vertex.position -> camera, normalized.
When I tried to do it like this:

```cs
float3 cameraPos = float3(_WorldSpaceCameraPos.x, 0, _WorldSpaceCameraPos.z);
float lightInfo = ComputeLighting(normalize(cameraPos - i.vertex));
```


### Applying Light Color

Finally, we apply the light color to our fragment shader. We use `_LightColor0` to get the light's color and multiply it with the texture color and the computed lighting.

```cs
fixed4 frag(v2f i) : SV_Target
{
  // Sample the texture
  fixed4 texColor = tex2D(_BaseMap, i.uv);

  // Calculate lighting
  float lightInfo = ComputeLighting(i.worldNormal);

  // Apply lighting to the color
  float3 finalColor = lightInfo * texColor.rgb * _LightColor0.xyz;

  return fixed4(finalColor, texColor.a);
}
```

This ensures that our character is properly lit and tinted by the light color in the scene.

![LightTint](Images/LightTint.png)

Additionally we can also use the ambient light like this:

```cs
float3 finalColor = texColor.rgb * (unity_AmbientEquator + (lightInfo * _LightColor0.rgb));
```

This could be more fancy, like making really the gradient.

---

## References

"Character art: The Adventurer - Male", sScary. Available at: https://sscary.itch.io/the-adventurer-male

"What is billboarding and can/should it be used in 3D games to create special effects?" Game Development Stack Exchange. Available at: https://gamedev.stackexchange.com/questions/54871

"What are the fundamentals of a Quad Billboarding effect?", Game Development Stack Exchange. Available at: https://gamedev.stackexchange.com/questions/19037/what-are-the-fundamentals-of-a-quad-billboarding-effect?rq=1

"Custom Shadow Mapping in Unity", Shahriar Shahrabi. Available at: https://shahriyarshahrabi.medium.com/custom-shadow-mapping-in-unity-c42a81e1bbf8

"How to make unlit shader that casts shadow?", Khai3 in Unity Discussions. Available at: https://discussions.unity.com/t/how-to-make-unlit-shader-that-casts-shadow/736016

"Billboarding Shaders - 3D Games in GameMaker", DragoniteSpam. Available at: https://www.youtube.com/watch?v=gMKMRkZzR9M&ab_channel=DragoniteSpam

"Real time lighting in custom shaders", scottharber in Unity Discussions. Available at: https://discussions.unity.com/t/real-time-lighting-in-custom-shaders/658262/5

"How to turn this unlit shader into a lit shader? HELP", pwolf1897 in Unity Discussions. Available at: https://discussions.unity.com/t/how-to-turn-this-unlit-shader-into-a-lit-shader-help/931815

"Toon Shader From Scratch - Explained!", eleonora. Available at: https://www.youtube.com/watch?v=owwnUcmO3Lw&ab_channel=eleonora

"Built-in shader variables reference", Unity Manual. Available at: https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html

And of course: [Diogo Andrade on Youtube](https://www.youtube.com/@diogoandrade9588)

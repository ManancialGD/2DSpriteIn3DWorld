Shader "Custom/Custom_Sprite3D"
{
    Properties
    {
        _BaseMap ("BaseMap", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "Queue"="Transparent" "LightMode"="ShadowCaster" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            
            // Vertex shader
            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;

                // Calculate the origin in world and view space
                float4 origin = float4(0,0,0,1);
                float4 world_origin = mul(UNITY_MATRIX_M, origin);
                float4 view_origin = mul(UNITY_MATRIX_V, world_origin);
                float4 world_to_view_translation = view_origin - world_origin;

                // Transform vertex position to view space
                float4 world_pos = mul(UNITY_MATRIX_M, v.vertex);
                float4 view_pos = world_pos + world_to_view_translation;
                float4 clip_pos = mul(UNITY_MATRIX_P, view_pos);

                o.pos = clip_pos;
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            // Fragment shader
            float frag(vertexInput i) : SV_Target
            {
                float4 texColor = tex2D(_BaseMap, i.uv);
         
                // Discard fragments with alpha less than 0.5
                clip(texColor.a - 0.5);
                return texColor.a;
            }

            ENDCG
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 worldNormal : NORMAL;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST;

            // Compute lighting based on normal and light direction
            float ComputeLighting(float3 normal)
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                
                float realDot = dot(normal, lightDir);

                float NdotL = abs(realDot);

                if (realDot <= 0)  // if the light comes from behind.
                {
                    NdotL += dot(normal, lightDir) * 0.8; // then make it a little bit more dark then the front.
                }

                NdotL += 0.1; // make it a little bit less dark.

                return NdotL;
            }

            // Vertex shader
            v2f vert(appdata_t v)
            {
                v2f o;

                // Calculate the origin in world and view space
                float4 origin = float4(0,0,0,1);
                float4 world_origin = mul(UNITY_MATRIX_M, origin);
                float4 view_origin = mul(UNITY_MATRIX_V, world_origin);
                float4 world_to_view_translation = view_origin - world_origin;

                // Transform vertex position to view space
                float4 world_pos = mul(UNITY_MATRIX_M, v.vertex);
                float4 view_pos = world_pos + world_to_view_translation;
                float4 clip_pos = mul(UNITY_MATRIX_P, view_pos);

                o.vertex = clip_pos;
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            // Fragment shader
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 texColor = tex2D(_BaseMap, i.uv);

                // Calculate lighting
                float lightInfo = ComputeLighting(i.worldNormal);

                // Apply lighting to the color
                float3 finalColor = texColor.rgb * (unity_AmbientEquator + (lightInfo * _LightColor0.rgb));

                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return fixed4(finalColor, texColor.a);
            }
            ENDCG
        }
    }
}
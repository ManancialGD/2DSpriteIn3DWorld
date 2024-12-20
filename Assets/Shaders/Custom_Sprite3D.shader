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

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct vertexOutpot
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            
            vertexOutpot vert (vertexInput v)
            {
                vertexOutpot o;

                float4 origin = float4(0,0,0,1);
                float4 world_origin = mul(UNITY_MATRIX_M, origin);
                float4 view_origin = mul(UNITY_MATRIX_V, world_origin);
                float4 world_to_view_translation = view_origin - world_origin;

                float4 world_pos = mul(UNITY_MATRIX_M, v.vertex);
                float4 view_pos = world_pos + world_to_view_translation;
                float4 clip_pos = mul(UNITY_MATRIX_P, view_pos);

                o.pos = clip_pos;
                return o;
            }

            float4 frag(vertexOutpot i): SV_Target
            {
                fixed4 col = tex2D(_BaseMap, i.uv);
                return col;
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

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST;

            v2f vert(appdata_t v)
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

                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_BaseMap, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
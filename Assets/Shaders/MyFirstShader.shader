Shader "Introduction/MyFirstShader"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _SpecularMap ("Specular Map", 2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            sampler2D _BaseMap;
            float4 _BaseColor;
            sampler2D _NormalMap;
            sampler2D _SpecularMap;
            float4 _SpecColor;
            sampler2D _EmissionMap;
            float4 _EmissionColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the base map with UV coordinates
                float4 baseMapColor = tex2D(_BaseMap, i.uv) * _BaseColor;

                // Sample the normal map and unpack it to world space
                float3 normalWS = UnpackNormal(tex2D(_NormalMap, i.uv));

                // Sample the emission map
                float3 emission = tex2D(_EmissionMap, i.uv).rgb * _EmissionColor.rgb;

                // Combine base color and emission for demonstration
                float3 litColor = baseMapColor.rgb + emission;

                return float4(litColor, baseMapColor.a);
            }
            ENDCG
        }
    }
}

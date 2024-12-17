Shader "Custom/YBillboardShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION; // Position attribute
                float2 uv : TEXCOORD0;    // UV attribute
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1) // Fog coordinates
                float4 vertex : SV_POSITION; // Final position
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
            
                // Object space position
                float4 object_space_pos = v.vertex;
            
                // Camera position in world space
                float3 camera_pos = _WorldSpaceCameraPos.xyz;
                float3 obj_pos = object_space_pos.xyz;
            
                // Compute the direction from the object to the camera
                float3 to_camera = normalize(camera_pos - obj_pos);
            
                // The "up" vector is fixed to the Y-axis (world-space)
                float3 up = float3(0.0f, 1.0f, 0.0f);
                
                // Compute the right vector as the cross product of the up vector and the direction to the camera
                float3 right = normalize(cross(up, to_camera));
                
                // Recompute the forward direction as the cross product of the right and up vectors
                float3 forward = cross(right, up);
            
                // Now we need to transform the object’s local position by removing the Y component
                // from the world position, preserving only X and Z for billboarding.
                float3 billboarded_pos = obj_pos;
                billboarded_pos.x = dot(obj_pos, right);  // Project the position onto the right axis
                billboarded_pos.z = dot(obj_pos, forward); // Project the position onto the forward axis
            
                // Rebuild the object space position with the billboarded X and Z components
                object_space_pos = float4(billboarded_pos.x, object_space_pos.y, billboarded_pos.z, object_space_pos.w);
            
                // Transform the object position to world space and then to clip space
                float4 world_pos = mul(unity_ObjectToWorld, object_space_pos);
                o.vertex = mul(UNITY_MATRIX_P, world_pos);
            
                // Pass the UV coordinates for texture mapping
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            
                // Apply fog
                UNITY_TRANSFER_FOG(o, o.vertex);
            
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Fetch color from texture using the passed UV coordinates
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Apply fog effect
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}

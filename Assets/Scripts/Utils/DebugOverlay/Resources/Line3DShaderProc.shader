Shader "Debug/Line3DShaderProc"
{
    Properties
    {
    }

    SubShader
    {
        Pass
        {
            Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
            }

            ZWrite On
            ZTest Less
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5

            #include "UnityCG.cginc"

            float4 scales; // scale (x, y)

            struct instancedata
            {
                float4 start;
                float4 end;
                float4 color; 
            };

            StructuredBuffer<instancedata> positionBuffer;
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : TEXCOORD3;
            };

            v2f vert(uint vid : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                int instID = vid / 6;
                int vertID = vid - instID * 6;

                // Generates (0,0) (1,0) (1,1) (1,1) (0,1) (0,0) from vertID
                float4 v_pos = saturate(float4((2 - abs(vertID - 2)), (2 - abs(vertID - 3)), 0, 0));

                // Center around y
                v_pos.x -= 0.5;

                // Read instance data
                float4 start = positionBuffer[instID].start;
                float4 end = positionBuffer[instID].end;
                float4 color = positionBuffer[instID].color;

                float4 dir = end - start;
                float3 startDir = start - _WorldSpaceCameraPos;
                float3 endDir = end - _WorldSpaceCameraPos;

                float3 xOffsetUnit = v_pos.y * normalize(cross(dir, endDir)) * length(endDir) * 0.5 + (1 - v_pos.y) * normalize(cross(dir, startDir)) * length(startDir) * 0.5;

                float pointScale = 0.01;
                float4 p = (start + dir * v_pos.y) + float4(xOffsetUnit, 0) * v_pos.x * pointScale;

                p = UnityObjectToClipPos(p);

                v2f o;
                o.pos = p;
                o.color = color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return float4(i.color.rgb, 1);
            }

            ENDCG
        }
    }
}
Shader "Debug/LineShaderProc"
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
                "RenderType"="Transparent"
            }

            ZWrite Off
            ZTest Less
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5
            
            #include "UnityCG.cginc"

            float4 scales;

            struct instancedata
            {
                float4 pos;
                float4 color;
            };

            StructuredBuffer<instancedata> instanceBuffer;

            struct v2f
            {
                float4 color : TEXCOORD3;
                float4 pos : SV_POSITION;
            };
                        
            v2f vert (uint vid : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                int instID = vid / 6;
                int vertID = vid - instID * 6;

                // Generate v_pos, (0, 0) (1, 0) (1, 1) (1, 1) (0, 1) (0, 0)
                float2 v_pos = saturate(float2(2 - abs(vertID - 2), 2 - abs(vertID - 3)));

                // Read data
                float4 pos = instanceBuffer[instID].pos;
                float4 color = instanceBuffer[instID].color;

                // Generate position
                float2 dir = pos.zw - pos.xy;
                float2 pdir = normalize(float2(-dir.y, dir.x));
                float2 p = (pos + dir * v_pos.y) * scales.xy + pdir * v_pos.x * 3.0 * scales.zw;
                p = float2(-1, -1) + p * 2;

                // flip y
                p.y *= -1;

                v2f o;
                o.pos = float4(p, 1, 1);
                o.color = color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.color.rgb, 1);
            }

            ENDCG
        }
    }
}
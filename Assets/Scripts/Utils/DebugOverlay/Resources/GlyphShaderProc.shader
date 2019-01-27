Shader "Debug/GlyphShaderProc"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
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

            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct instancedata
            {
                float4 pos;
                float4 size;
                float4 color;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : TEXCOORD3;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 scales;
            StructuredBuffer<instancedata> instanceBuffer;

            v2f vert(uint vid : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                int instID = vid / 6;
                int vertID = vid - instID * 6;

                // Generate (0, 0) (1, 0) (1, 1) (1, 1) (0, 1) (0, 0) from vertID
                float2 v_pos = saturate(float2(2 - abs(vertID - 2), 2 - abs(vertID - 3)));

                // Read data
                float4 pos_uv = instanceBuffer[instID].pos;
                float2 scale = instanceBuffer[instID].size.xy;
                float4 color = instanceBuffer[instID].color;

                // Generate uv
                float2 uv = (pos_uv.zw + v_pos) * scales.zw;
                uv.y = 1 - uv.y;

                // Generate pos
                float2 pos = (pos_uv.xy + v_pos * scale) * scales.xy;
                pos = float2(-1, -1) + pos * 2;

                // Need to flip y for HD pipe for some reason
                pos.y *= -1;

                v2f o;
                o.uv = uv;
                o.color = color;
                o.pos = float4(pos, 1, 1);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 albedo = tex2D(_MainTex, i.uv);
                albedo = lerp(albedo, float4(1, 1, 1, 1), i.color.a);
                return albedo * float4(i.color.rgb, 1);
            }
            ENDCG
        }
    }
}

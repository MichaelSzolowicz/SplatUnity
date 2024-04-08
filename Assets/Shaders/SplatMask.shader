// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Splatmask"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "black" {}
        _InkColor("Painter Color", Color) = (1,0,0,1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float3 _SplatPos;
            float3 _Normal;
            float _Radius;
            float _Hardness;
            float _Strength;
            float4 _InkColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };


            // Vert to Frag. Data passed from vertx to fragment shader.
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                float4 uv = float4(0, 0, 0, 1);
                uv.xy = (v.uv.xy * 2 - 1) * float2(1, _ProjectionParams.x);
                o.vertex = uv;
                o.normal = v.normal;
                return o;
            }

            // Draw a circular brush stroke at pos
            float mask(float3 pos, float3 center, float radius, float hardness) {
                float m = distance(pos, center);
                return 1 - smoothstep(radius * hardness, radius, m);
            }

            float4 frag(v2f i) : SV_Target
            {  
                float4 col = tex2D(_MainTex, i.uv);
                float m = mask(i.worldPos, _SplatPos, _Radius, _Hardness);
                float edge = m * _Strength;
                col = lerp(col, _InkColor, edge);

                // Compare normals and don't draw on surfaces facing the opposite direction (underside of platform or ceiling above us.)
                float d = dot(i.normal, _Normal);
                d = d - .05;    // Give a little bit of leniance so it cuts off before 90 degrees.
                d = clamp(d, 0, 1);
                d = ceil(d);
                col = col * d;

                return col;
                
            }
            ENDCG
        }
    }
}

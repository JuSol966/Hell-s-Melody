Shader "Unlit/Outline"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width (px)", Range(0,4)) = 1
        _AlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.10
        _Cue ("Cue Strength", Range(0,1)) = 0        // força do efeito
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"
               "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 pos  : SV_POSITION;
                float2 uv   : TEXCOORD0;
                fixed4 col  : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float  _OutlineWidth;
            float  _AlphaThreshold;
            float  _Cue;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                o.col = v.color * _Color;
                return o;
            }

            inline float SampleA(float2 uv) { return tex2D(_MainTex, uv).a; }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = tex * i.col; // sprite normal (com tint/vertex color)

                // offsets em UV equivalentes a "N" pixels
                float2 texel = _MainTex_TexelSize.xy;
                float  r     = _OutlineWidth;
                float2 o     = float2(texel.x * r, texel.y * r);

                // maior alpha entre 8 vizinhos
                float na = 0.0;
                na = max(na, SampleA(i.uv + float2( o.x, 0)));
                na = max(na, SampleA(i.uv + float2(-o.x, 0)));
                na = max(na, SampleA(i.uv + float2(0,  o.y)));
                na = max(na, SampleA(i.uv + float2(0, -o.y)));
                na = max(na, SampleA(i.uv + float2( o.x,  o.y)));
                na = max(na, SampleA(i.uv + float2(-o.x,  o.y)));
                na = max(na, SampleA(i.uv + float2( o.x, -o.y)));
                na = max(na, SampleA(i.uv + float2(-o.x, -o.y)));

                // mask: onde o pixel atual é "vazio" mas vizinhos são "cheios"
                float outlineMask = step(_AlphaThreshold, na) * (1 - step(_AlphaThreshold, tex.a));

                // cor do outline modulada pela força (_Cue)
                float outlineA = saturate(_Cue) * _OutlineColor.a * outlineMask;
                float3 outlineRGB = _OutlineColor.rgb;

                // compor: outline atrás do sprite (não "lava" as partes opacas)
                float3 rgb = col.rgb + outlineRGB * outlineA * (1 - col.a);
                float  a   = saturate(col.a + outlineA * (1 - col.a));

                return float4(rgb, a);
            }
            ENDCG
        }
    }
}

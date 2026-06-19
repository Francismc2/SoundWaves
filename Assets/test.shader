Shader "Custom/test"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _SmokeColor ("Smoke Color", Color) = (1,1,1,1)
        _Opacity ("Opacity", Range(0,1)) = 0.15
        _NoiseScale ("Noise Scale", Float) = 1
        _ScrollSpeed ("Scroll Speed", Vector) = (0.05, 0.1, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _NoiseTex;
            float4 _SmokeColor;
            float _Opacity;
            float _NoiseScale;
            float4 _ScrollSpeed;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _NoiseScale;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv + _ScrollSpeed.xy * _Time.y;

                float noise = tex2D(_NoiseTex, uv).r;

                // Soft smoke appearance
                noise = smoothstep(0.2, 0.8, noise);

                fixed4 col = _SmokeColor;
                col.a = noise * _Opacity;

                return col;
            }
            ENDCG
        }
    }
}
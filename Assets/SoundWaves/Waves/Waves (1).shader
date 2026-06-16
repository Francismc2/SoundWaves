Shader "Custom/Waves"
{
    Properties
    {
        Color ("Color", Color) = (0.0, 0.8, 1.0, 1.0)
        Freq ("Frequency", Float) = 2.0
        Amp ("Amplitude", Float) = 0.2
        Speed ("Speed", Float) = 1.0
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

            fixed4 Color;
            float  Freq;
            float  Amp;
            float  Speed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;

                float wave = Amp * sin(Freq * v.vertex.x - Speed * _Time.y);
                v.vertex.y += wave;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return Color;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}

Shader "Custom/Waves"
{
    Properties
    {
        _Color ("Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _Freq ("Freq", Float) = 2.0
        _Amp ("Amp", Float) = 0.2
        _Speed ("Speed", Float) = 1.0
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

            fixed4 _Color;
            float  _Freq;
            float  _Amp;
            float  _Speed;

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

                float wave = _Amp * sin(_Freq * v.vertex.x - _Speed * _Time.y);
                v.vertex.y += wave;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}

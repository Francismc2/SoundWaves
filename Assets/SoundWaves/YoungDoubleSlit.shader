Shader "Custom/YoungDoubleSlit"
{
    Properties
    {
        _Wavelength ("Wavelength", Float) = 0.05
        _SlitDistance ("Slit Distance", Float) = 0.3
        _Brightness ("Brightness", Float) = 2.0
        _WaveSpeed ("Wave Speed", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define PI 3.14159265

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Wavelength;
            float _SlitDistance;
            float _Brightness;
            float _WaveSpeed;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Center UV coordinates
                float2 p = i.uv - 0.5;

                // Slit positions
                float2 slitA = float2(-_SlitDistance * 0.5, 0);
                float2 slitB = float2( _SlitDistance * 0.5, 0);

                // Distance from current pixel to each slit
                float dA = distance(p, slitA);
                float dB = distance(p, slitB);

                // Two coherent waves
                float waveA =
                    cos((2.0 * PI * dA / _Wavelength)
                    - (_Time.y * _WaveSpeed));

                float waveB =
                    cos((2.0 * PI * dB / _Wavelength)
                    - (_Time.y * _WaveSpeed));

                // Interference
                float amplitude = waveA + waveB;

                // Intensity
                float intensity = amplitude * amplitude;

                intensity *= 0.25;
                intensity *= _Brightness;

                return float4(
                    intensity,
                    intensity,
                    intensity,
                    1.0);
            }

            ENDCG
        }
    }
}
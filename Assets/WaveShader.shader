Shader "Custom/DoubleSlitWave"
{
    Properties
    {
        _SlitSep    ("Slit Separation", Float)  = 1.0
        _SlitWidth  ("Slit Width",      Float)  = 0.3
        _Wavelength ("Wavelength",      Float)  = 0.5
        _WaveTime   ("Time",            Float)  = 0.0
        _Amplitude  ("Amplitude",       Float)  = 1.0
        _BarrierPos ("Barrier Z (0-1)", Float)  = 0.4
        _WorldWidth ("World Width",     Float)  = 10.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _SlitSep, _SlitWidth, _Wavelength;
            float _WaveTime, _Amplitude, _BarrierPos, _WorldWidth;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos    : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;   // (0..1, 0..1)  →  x: horizontal, y: depth
                return o;
            }

            // One Huygens wavelet from source at (srcX, srcZ) toward (x, z)
            float wavelet(float x, float z, float srcX, float srcZ)
            {
                float dist = length(float2(x - srcX, z - srcZ));
                if (dist < 0.001) return 0;
                float k    = 6.2832 / _Wavelength;
                float fade = rsqrt(dist + 0.1);          // 2-D amplitude fall-off
                return fade * sin(k * dist - _WaveTime * 5.0);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Map UVs → world coords
                float x = (i.uv.x - 0.5) * _WorldWidth;   // horizontal, world units
                float z =  i.uv.y;                          // depth,      0..1

                float bz     = _BarrierPos;
                float slit1x =  _SlitSep * 0.5;
                float slit2x = -_SlitSep * 0.5;

                // ── Barrier pixels ──────────────────────────────────────
                float atBarrier = abs(z - bz) < 0.008 ? 1.0 : 0.0;
                if (atBarrier > 0.0)
                {
                    float inS1 = abs(x - slit1x) < _SlitWidth * 0.5 ? 1.0 : 0.0;
                    float inS2 = abs(x - slit2x) < _SlitWidth * 0.5 ? 1.0 : 0.0;
                    if (inS1 < 1.0 && inS2 < 1.0)
                        return fixed4(0.18, 0.18, 0.24, 1.0);   // solid barrier
                    // Gap in barrier: let wave through (fall through to wave calc)
                }

                // ── Wave amplitude ──────────────────────────────────────
                float wave = 0.0;

                if (z < bz)
                {
                    // Before barrier: single plane-ish wave from source at top-center
                    wave = wavelet(x, z, 0.0, 0.0);
                }
                else
                {
                    // After barrier: two Huygens point sources at slit openings
                    wave  = wavelet(x, z, slit1x, bz);
                    wave += wavelet(x, z, slit2x, bz);
                    wave *= 0.5;
                }

                // ── Colour ──────────────────────────────────────────────
                // Crest = blue, trough = orange, zero = transparent
                float pos = max( wave, 0.0);
                float neg = max(-wave, 0.0);

                fixed3 col = fixed3(0.1, 0.5, 1.0) * pos
                           + fixed3(1.0, 0.4, 0.1) * neg;

                float alpha = clamp(abs(wave) * 1.2, 0.0, 0.75) * _Amplitude;

                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
}

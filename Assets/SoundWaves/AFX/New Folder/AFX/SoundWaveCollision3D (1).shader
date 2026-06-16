Shader "Custom/SoundWaveCollision3D"
{
    Properties
    {
        [Header(WaveA_LeftSource)]
        _FreqA       ("WaveA Frequency",   Range(0.2, 8.0))  = 1.0
        _AmpA        ("WaveA Amplitude",   Range(0.0, 2.0))  = 1.0
        _SpeedA      ("WaveA Speed",       Range(0.1, 5.0))  = 1.0

        [Header(WaveB_RightSource)]
        _FreqB       ("WaveB Frequency",   Range(0.2, 8.0))  = 1.0
        _AmpB        ("WaveB Amplitude",   Range(0.0, 2.0))  = 1.0
        _SpeedB      ("WaveB Speed",       Range(0.1, 5.0))  = 1.0

        [Header(Global)]
        _Damping     ("Damping",           Range(0.0, 1.0))  = 0.3
        _Spread      ("Ripple Spread",     Range(0.1, 3.0))  = 1.0
        _WaveHeight  ("Wave Height",       Range(0.0, 3.0))  = 1.0

        [Header(SourcePositions)]
        _SrcAX       ("Source A X",        Range(-8.0, 8.0)) = -4.5
        _SrcBX       ("Source B X",        Range(-8.0, 8.0)) =  4.5

        [Header(Colours)]
        _ColorPeak   ("Peak Crest",        Color) = (0.0, 0.85, 1.0, 1.0)
        _ColorTrough ("Trough",            Color) = (1.0, 0.40, 0.20, 1.0)
        _ColorMid    ("Mid Base",          Color) = (0.03, 0.08, 0.18, 1.0)

        [Header(Surface)]
        _SpecColor2  ("Specular Color",    Color) = (0.4, 0.4, 0.4, 1.0)
        _Shininess   ("Shininess",         Range(1.0, 512.0)) = 128.0
        _NormalStr   ("Normal Strength",   Range(0.0, 3.0))   = 1.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 300

        // ================================================================
        //  PASS 1 - Ambient + first directional light (ForwardBase)
        // ================================================================
        Pass
        {
            Name "FORWARDBASE"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   4.5
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            // -- Uniforms -------------------------------------------------
            float  _FreqA;   float _AmpA;  float _SpeedA;
            float  _FreqB;   float _AmpB;  float _SpeedB;
            float  _Damping; float _Spread; float _WaveHeight;
            float  _SrcAX;   float _SrcBX;
            float4 _ColorPeak;
            float4 _ColorTrough;
            float4 _ColorMid;
            float4 _SpecColor2;
            float  _Shininess;
            float  _NormalStr;

            // -- Structs --------------------------------------------------
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float  dispSum  : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                SHADOW_COORDS(4)
            };

            // -- Core wave function ----------------------------------------
            // Blender exports planes flat on XY and displaces along Z.
            // Sources sit along the X axis at Y = 0.
            float Wave(float2 xy, float srcX,
                       float freq, float amp, float speed, float damp)
            {
                float2 delta = (xy - float2(srcX, 0.0)) * _Spread;
                float  dist  = length(delta);
                float  phase = dist * freq * UNITY_TWO_PI
                             - _Time.y * speed * UNITY_PI;
                return sin(phase) * amp * exp(-dist * damp);
            }

            // -- Vertex ---------------------------------------------------
            v2f vert(appdata v)
            {
                v2f o;

                // Read surface position from XY (Blender plane lies on XY)
                float2 xy = v.vertex.xy;
                float dA  = Wave(xy, _SrcAX, _FreqA, _AmpA, _SpeedA, _Damping);
                float dB  = Wave(xy, _SrcBX, _FreqB, _AmpB, _SpeedB, _Damping);
                float dS  = (dA + dB) * _WaveHeight;

                // Displace along Z - the axis pointing out of a Blender XY plane
                v.vertex.z   += dS;
                o.pos         = UnityObjectToClipPos(v.vertex);
                o.worldPos    = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv          = v.uv;
                o.dispSum     = dS;

                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            // -- Fragment -------------------------------------------------
            fixed4 frag(v2f i) : SV_Target
            {
                // Analytic normal from world position screen-space derivatives
                float3 dx   = ddx(i.worldPos);
                float3 dy   = ddy(i.worldPos);
                float3 geoN = normalize(cross(dy, dx));
                // Blend toward (0,0,-1) - the rest-facing normal of an XY plane
                float3 N    = normalize(lerp(float3(0, 0, -1), geoN, _NormalStr));

                // Colour from superposed displacement
                float  d   = i.dispSum;
                float3 col = _ColorMid.rgb;
                col = lerp(col, _ColorPeak.rgb,   saturate( d * 0.7));
                col = lerp(col, _ColorTrough.rgb,  saturate(-d * 0.7));

                // White flash at strong constructive peak
                col = lerp(col, float3(1, 1, 1),          saturate(( d - 1.6) * 2.5));
                // Deep void at strong destructive trough
                col = lerp(col, float3(0, 0.01, 0.04),    saturate((-d - 1.6) * 2.5));

                // Blinn-Phong lighting
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir  = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir  = normalize(lightDir + viewDir);

                float  diff     = max(0.0, dot(N, lightDir));
                float  spec     = pow(max(0.0, dot(N, halfDir)), _Shininess);

                float3 ambient  = UNITY_LIGHTMODEL_AMBIENT.rgb * col;
                float3 diffuse  = _LightColor0.rgb * col * diff;
                float3 specular = _LightColor0.rgb * _SpecColor2.rgb * spec;

                float  shadow   = SHADOW_ATTENUATION(i);
                float3 finalCol = ambient + (diffuse + specular) * shadow;

                // Edge vignette
                float2 e = abs(i.uv - 0.5) * 2.0;
                finalCol *= 1.0 - smoothstep(0.8, 1.0, max(e.x, e.y));

                fixed4 result = fixed4(finalCol, 1.0);
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;
            }
            ENDCG
        }

        // ================================================================
        //  PASS 2 - Additional pixel lights (ForwardAdd)
        // ================================================================
        Pass
        {
            Name "FORWARDADD"
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            ZWrite Off

            CGPROGRAM
            #pragma vertex   vertAdd
            #pragma fragment fragAdd
            #pragma target   4.5
            #pragma multi_compile_fwdadd_fullshadows

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            float  _FreqA;   float _AmpA;  float _SpeedA;
            float  _FreqB;   float _AmpB;  float _SpeedB;
            float  _Damping; float _Spread; float _WaveHeight;
            float  _SrcAX;   float _SrcBX;
            float4 _ColorPeak;
            float4 _ColorTrough;
            float4 _ColorMid;
            float4 _SpecColor2;
            float  _Shininess;
            float  _NormalStr;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float  dispSum  : TEXCOORD2;
                SHADOW_COORDS(3)
            };

            float Wave(float2 xy, float srcX,
                       float freq, float amp, float speed, float damp)
            {
                float2 delta = (xy - float2(srcX, 0.0)) * _Spread;
                float  dist  = length(delta);
                float  phase = dist * freq * UNITY_TWO_PI
                             - _Time.y * speed * UNITY_PI;
                return sin(phase) * amp * exp(-dist * damp);
            }

            v2f vertAdd(appdata v)
            {
                v2f o;
                float2 xy = v.vertex.xy;
                float  dS = (Wave(xy, _SrcAX, _FreqA, _AmpA, _SpeedA, _Damping)
                           + Wave(xy, _SrcBX, _FreqB, _AmpB, _SpeedB, _Damping))
                           * _WaveHeight;
                v.vertex.z  += dS;
                o.pos        = UnityObjectToClipPos(v.vertex);
                o.worldPos   = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv         = v.uv;
                o.dispSum    = dS;
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 fragAdd(v2f i) : SV_Target
            {
                float3 dx  = ddx(i.worldPos);
                float3 dy  = ddy(i.worldPos);
                float3 N   = normalize(lerp(float3(0, 0, -1),
                             normalize(cross(dy, dx)), _NormalStr));

                float  d   = i.dispSum;
                float3 col = _ColorMid.rgb;
                col = lerp(col, _ColorPeak.rgb,   saturate( d * 0.7));
                col = lerp(col, _ColorTrough.rgb,  saturate(-d * 0.7));

                float3 lightDir = normalize(
                    _WorldSpaceLightPos0.xyz
                  - _WorldSpaceLightPos0.w * i.worldPos);
                float3 viewDir  = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir  = normalize(lightDir + viewDir);

                float diff   = max(0.0, dot(N, lightDir));
                float spec   = pow(max(0.0, dot(N, halfDir)), _Shininess);
                float shadow = SHADOW_ATTENUATION(i);

                float3 result = _LightColor0.rgb
                              * (col * diff + _SpecColor2.rgb * spec)
                              * shadow;

                return fixed4(result, 1.0);
            }
            ENDCG
        }

        // ================================================================
        //  PASS 3 - Shadow caster
        // ================================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            CGPROGRAM
            #pragma vertex   vertShadow
            #pragma fragment fragShadow
            #pragma target   4.5
            #pragma multi_compile_shadowcaster

            #include "UnityCG.cginc"

            float  _FreqA;   float _AmpA;  float _SpeedA;
            float  _FreqB;   float _AmpB;  float _SpeedB;
            float  _Damping; float _Spread; float _WaveHeight;
            float  _SrcAX;   float _SrcBX;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f     { V2F_SHADOW_CASTER; };

            float Wave(float2 xy, float srcX,
                       float freq, float amp, float speed, float damp)
            {
                float dist  = length((xy - float2(srcX, 0.0)) * _Spread);
                float phase = dist * freq * UNITY_TWO_PI
                            - _Time.y * speed * UNITY_PI;
                return sin(phase) * amp * exp(-dist * damp);
            }

            v2f vertShadow(appdata v)
            {
                float2 xy  = v.vertex.xy;
                float  dS  = (Wave(xy, _SrcAX, _FreqA, _AmpA, _SpeedA, _Damping)
                            + Wave(xy, _SrcBX, _FreqB, _AmpB, _SpeedB, _Damping))
                            * _WaveHeight;
                v.vertex.z += dS;
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 fragShadow(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}

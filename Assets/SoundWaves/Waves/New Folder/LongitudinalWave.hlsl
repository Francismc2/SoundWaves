// LongitudinalWave.hlsl
// Drop this file anywhere in your Assets folder.
// In Shader Graph, add a Custom Function node, set Type = File,
// and point it at this file. Function name: LongitudinalWave
//
// INPUTS  (add these ports on the Custom Function node):
//   UV         : Vector2
//   Time       : Float
//   Amplitude  : Float   (default 0.08)
//   Frequency  : Float   (default 2.0)
//   WaveSpeed  : Float   (default 1.0)
//   NumWaves   : Float   (default 3.0)
//   ParticleRows: Float  (default 30.0)
//
// OUTPUTS:
//   Color      : Vector4   (plug into Base Color / Emission)
//   Brightness : Float     (plug into Emission Intensity or Alpha)

void LongitudinalWave_float(
    float2 UV,
    float  Time,
    float  Amplitude,
    float  Frequency,
    float  WaveSpeed,
    float  NumWaves,
    float  ParticleCols,
    float  ParticleRows,
    out float4 Color,
    out float  Brightness
)
{
    // ── Grid ─────────────────────────────────────────────────────────────────
    // Scale UV into a grid of ParticleCols x ParticleRows cells
    float2 grid     = float2(ParticleCols, ParticleRows);
    float2 cellUV   = UV * grid;               // [0, cols] x [0, rows]
    float2 cellID   = floor(cellUV);           // which cell we're in
    float2 localUV  = frac(cellUV);            // [0,1] inside the cell

    // Particle's rest position (0..1 along x axis)
    float restX = (cellID.x + 0.5) / ParticleCols;

    // ── Standing longitudinal wave ────────────────────────────────────────────
    // u(x,t) = A * sin(k*x) * cos(w*t)
    // This is the superposition of a left- and right-travelling wave.
    float k   = 2.0 * 3.14159265 * NumWaves;        // wave number (full waves across width)
    float w   = 2.0 * 3.14159265 * Frequency;
    float disp = Amplitude * sin(k * restX) * cos(w * Time * WaveSpeed);

    // ── Compression ───────────────────────────────────────────────────────────
    // compression = -du/dx  → tells us if particles are squeezed or spread
    float compression = -Amplitude * k * cos(k * restX) * cos(w * Time * WaveSpeed);
    // Normalise to [-1, 1]
    float maxC = max(Amplitude * k, 0.0001);
    float cNorm = clamp(compression / maxC, -1.0, 1.0);

    // ── Displaced particle centre ─────────────────────────────────────────────
    // Displace the particle's x position inside the cell (in UV space)
    float dispUV = disp * grid.x;                    // convert world disp → UV disp
    float2 center = float2(0.5 + dispUV, 0.5);       // shifted centre in local UV

    // ── SDF circle ───────────────────────────────────────────────────────────
    float aspect   = ParticleCols / max(ParticleRows, 0.001);
    float2 toCenter = (localUV - center) * float2(aspect, 1.0);
    float dist     = length(toCenter);

    float radius    = 0.35;                           // particle size (0–0.5)
    float edge      = 0.04;                           // soft edge width
    float circle    = 1.0 - smoothstep(radius - edge, radius, dist);

    // ── Colour by compression ─────────────────────────────────────────────────
    // cNorm > 0  → compressed   → red/warm
    // cNorm < 0  → rarefied     → blue/cool
    // cNorm = 0  → rest         → grey
    float3 compressCol   = float3(1.0, 0.25, 0.15);  // red
    float3 normalCol     = float3(0.75, 0.75, 0.80); // grey
    float3 rarefyCol     = float3(0.20, 0.50, 1.00); // blue

    float3 baseCol;
    if (cNorm >= 0.0)
        baseCol = lerp(normalCol, compressCol, cNorm);
    else
        baseCol = lerp(normalCol, rarefyCol, -cNorm);

    // Darken background, brighten particle
    float3 bgCol = float3(0.05, 0.05, 0.08);
    float3 finalCol = lerp(bgCol, baseCol, circle);

    // Subtle rim highlight
    float rim = smoothstep(radius - edge * 2.0, radius - edge, dist) * circle;
    finalCol += rim * 0.3;

    Color      = float4(finalCol, 1.0);
    Brightness = circle;
}

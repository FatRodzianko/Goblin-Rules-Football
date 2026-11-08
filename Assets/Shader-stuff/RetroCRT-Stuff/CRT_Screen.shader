// Full-screen CRT effect, driven by the Blit renderer feature on DefaultRenderer_Render_2d.
//
// Order inside the pass matters: everything between the linearise and the re-encode happens on
// light-like values, and the only clamp is the final one, so the mask survives in highlights.
//
// The project renders in Gamma colour space, so the source is sRGB-encoded and has to be
// linearised here by hand.
//
// The phosphor mask is anchored to output pixels (SV_POSITION), not to UVs, because a real mask
// sits on the glass and does not move with the picture. Everything else works in warped UV space.
Shader "Hidden/RetroCRT/CRT Screen"
{
    Properties
    {
        _MainTex ("Source (world)", 2D) = "white" {}
        // The UI is rendered by its own camera into its own texture and composited here, so the
        // effect covers it without it ever passing through the world camera's pipeline.
        _UITex ("Source (UI)", 2D) = "black" {}

        // Off still composites the two sources; only the effect itself is skipped.
        [Toggle] _EffectOn ("Effect on", Float) = 1

        // Whether the source arrives upside down depends on where the Blit feature runs. After
        // post-processing the camera target is the back buffer, so our pass draws into a temporary
        // render texture with a flipped projection and the copy back does not flip again, leaving
        // one net flip. Before post-processing both halves draw into render textures and cancel
        // out. Leave this on for the After Rendering Post Processing event, off for earlier ones.
        [Toggle] _FlipY ("Flip Y (on for the post-processing event)", Float) = 1

        [Header(Tube geometry)] [Space(4)]
        // Divisor, so larger = flatter. 0 disables the warp.
        _Curvature ("Barrel curvature (larger is flatter)", Range(0, 40)) = 10
        _VignetteWidth ("Frame vignette width (output px)", Range(0, 160)) = 30

        [Header(Horizontal colour bleed)] [Space(4)]
        _BleedAmount ("Bleed amount", Range(0, 1)) = 0.8
        // Exponential falloff in output pixels. The trail runs right, so the taps read left.
        _BleedTau ("Bleed falloff (output px)", Range(0.25, 12)) = 3
        // 1 = colour only (edges stay sharp, composite-chroma behaviour).
        // 0 = brightness smears too (beam slew and phosphor persistence).
        _BleedLumaKeep ("Keep luminance sharp", Range(0, 1)) = 0.6

        [Header(Scanlines)] [Space(4)]
        // Source lines, not output pixels. 432 is the pixel-perfect reference height.
        _ScanlineCount ("Scanline count (source lines)", Float) = 432
        _ScanlineStrength ("Scanline strength", Range(0, 1)) = 0.35
        _NoiseStrength ("Line noise", Range(0, 1)) = 0.15
        _NoiseSpeed ("Line noise speed", Range(0, 4)) = 1

        [Header(Phosphor mask)] [Space(4)]
        // Output pixels per R/G/B triad. Below 3 the pattern approaches Nyquist and shimmers.
        _MaskPitch ("Mask pitch (output px per triad)", Range(3, 12)) = 3
        _MaskStrength ("Mask strength", Range(0, 1)) = 0.5

        [Header(Beam glow)] [Space(4)]
        _GlowThreshold ("Glow threshold", Range(0, 1)) = 0.6
        _GlowStrength ("Glow strength", Range(0, 3)) = 0.6
        _GlowRadius ("Glow radius (output px)", Range(1, 16)) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "CRT Screen"
            Cull Off
            ZWrite Off
            ZTest Always
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Taps on each side of the kernels. Compile-time so the loops unroll.
            #define BLEED_TAPS 8
            #define GLOW_RING  1

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_UITex);
            SAMPLER(sampler_UITex);

            CBUFFER_START(UnityPerMaterial)
                float _EffectOn;
                float _FlipY;
                float _Curvature;
                float _VignetteWidth;
                float _BleedAmount;
                float _BleedTau;
                float _BleedLumaKeep;
                float _ScanlineCount;
                float _ScanlineStrength;
                float _NoiseStrength;
                float _NoiseSpeed;
                float _MaskPitch;
                float _MaskStrength;
                float _GlowThreshold;
                float _GlowStrength;
                float _GlowRadius;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // The project is in Gamma colour space, so do the encode/decode explicitly.
            float3 ToLinear(float3 c) { return pow(max(c, 0.0), 2.2); }
            float3 ToGamma (float3 c) { return pow(max(c, 0.0), 1.0 / 2.2); }

            float Luma(float3 c) { return dot(c, float3(0.299, 0.587, 0.114)); }

            // The UI texture carries premultiplied coverage: Pixel Perfect's stretch blends its
            // colour toward the transparent clear, which multiplies it by alpha already. So it is
            // added over the attenuated world rather than lerped, which would multiply twice.
            float3 Composite(float2 uv)
            {
                float3 world = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
                float4 ui    = SAMPLE_TEXTURE2D(_UITex, sampler_UITex, uv);
                return world * (1.0 - ui.a) + ui.rgb;
            }

            // Alpha has to survive this pass. If the effect ever runs over a camera that renders UI
            // into a texture - which happens if the old renderer feature is left switched on - then
            // writing a flat 1 here makes that texture opaque, and compositing it over the world
            // hides the world completely.
            float CompositeAlpha(float2 uv)
            {
                float world = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
                float ui    = SAMPLE_TEXTURE2D(_UITex, sampler_UITex, uv).a;
                return max(world, ui);
            }

            float3 SampleLinear(float2 uv)
            {
                return ToLinear(Composite(uv));
            }

            float Hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Both axes bend. k.x is driven by y and k.y by x, which is what makes it a barrel
            // rather than a one-axis stretch.
            float2 BarrelWarp(float2 uv, float curvature)
            {
                if (curvature <= 0.0) return uv;
                float2 c = uv * 2.0 - 1.0;
                float2 k = c.yx / curvature;
                c += c * (k * k);
                return c * 0.5 + 0.5;
            }

            // One-sided exponential kernel reading to the LEFT, so the smear trails right, the way
            // the beam does. Normalised, so average brightness is unchanged.
            float3 HorizontalBleed(float2 uv, float texelX)
            {
                float3 centre = SampleLinear(uv);
                if (_BleedAmount <= 0.0) return centre;

                float tau  = max(_BleedTau, 0.01);
                float3 acc = centre;
                float  wsum = 1.0;

                [unroll]
                for (int i = 1; i <= BLEED_TAPS; i++)
                {
                    float w = exp(-(float)i / tau);
                    acc  += SampleLinear(uv - float2((float)i * texelX, 0.0)) * w;
                    wsum += w;
                }

                float3 bled = acc / wsum;

                // Push the blurred result back to the original luminance. At 1 only the colour
                // runs and edges stay sharp; at 0 brightness smears as well.
                float3 chromaOnly = bled * (Luma(centre) / max(Luma(bled), 1e-4));
                bled = lerp(bled, chromaOnly, saturate(_BleedLumaKeep));

                return lerp(centre, bled, _BleedAmount);
            }

            // Bright-pass only, so dark areas stay dark. This is the beam growing with drive
            // voltage, which is why it keys off a threshold instead of blurring everything.
            float3 BeamGlow(float2 uv, float2 texel)
            {
                if (_GlowStrength <= 0.0) return float3(0.0, 0.0, 0.0);

                float3 acc  = float3(0.0, 0.0, 0.0);
                float  wsum = 0.0;

                [unroll]
                for (int y = -GLOW_RING; y <= GLOW_RING; y++)
                {
                    [unroll]
                    for (int x = -GLOW_RING; x <= GLOW_RING; x++)
                    {
                        float2 o = float2((float)x, (float)y) * _GlowRadius * texel;
                        float  w = exp(-(float)(x * x + y * y) * 0.5);
                        acc  += max(SampleLinear(uv + o) - _GlowThreshold, 0.0) * w;
                        wsum += w;
                    }
                }

                return (acc / wsum) * _GlowStrength;
            }

            // Tied to the source line count, not the output resolution, so the pattern stays
            // under Nyquist and resolves instead of turning into moire.
            float ScanlineWeight(float uvY, float noise)
            {
                float strength = _ScanlineStrength * lerp(1.0 - _NoiseStrength, 1.0, noise);
                float s = sin(frac(uvY * _ScanlineCount) * PI);
                s = s * s;
                // sin^2 averages 0.5, so divide by the mean to leave overall brightness alone.
                return lerp(1.0 - strength, 1.0, s) / max(1.0 - 0.5 * strength, 1e-4);
            }

            // Three raised cosines 120 degrees apart. They sum to a constant, so white stays
            // white and only the colour varies across the triad. Mean gain is 1 per channel.
            float3 PhosphorMask(float xPixels)
            {
                float t = TWO_PI * xPixels / max(_MaskPitch, 1.0);
                float3 m = 0.5 + 0.5 * float3(cos(t), cos(t - TWO_PI / 3.0), cos(t + TWO_PI / 3.0));
                return lerp(float3(1.0, 1.0, 1.0), m * 2.0, saturate(_MaskStrength));
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 texel = 1.0 / _ScreenParams.xy;

                // Flip before anything else, so the warp, the bleed and the scanlines all follow
                // the picture. The mask is not affected: it runs on output pixel x.
                float2 uv = float2(IN.uv.x, lerp(IN.uv.y, 1.0 - IN.uv.y, saturate(_FlipY)));

                // With the effect off this still has to run, because the UI only reaches the screen
                // through the composite.
                if (_EffectOn < 0.5)
                    return float4(Composite(uv), CompositeAlpha(uv));

                float2 uvW = BarrelWarp(uv, _Curvature);

                // The warp pushes the corners off the source, so black those out rather than
                // leaving it to whatever the sampler's wrap mode happens to be.
                float2 inside = step(0.0, uvW) * step(uvW, 1.0);
                float  onTube = inside.x * inside.y;

                float3 col = HorizontalBleed(uvW, texel.x);
                col += BeamGlow(uvW, texel);

                float lineIndex = floor(uvW.y * _ScanlineCount);
                float noise = Hash21(float2(lineIndex, floor(_Time.y * _NoiseSpeed * 24.0)));
                col *= ScanlineWeight(uvW.y, noise);

                col *= PhosphorMask(IN.positionCS.x);

                float2 edge = abs(uvW * 2.0 - 1.0);
                float2 w = max(_VignetteWidth / _ScreenParams.xy, 1e-5);
                float2 v = saturate(smoothstep(0.0, w, 1.0 - edge));
                col *= v.x * v.y * onTube;

                return float4(saturate(ToGamma(col)), CompositeAlpha(uvW) * onTube);
            }
            ENDHLSL
        }
    }

    Fallback Off
}

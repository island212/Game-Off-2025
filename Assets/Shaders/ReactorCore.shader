Shader "Custom/ReactorCore"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CoreColor ("Core Color", Color) = (0.2, 0.6, 1.0, 1.0)
        _EdgeColor ("Edge Color", Color) = (0.0, 0.3, 0.8, 1.0)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 3.0
        _RotationSpeed ("Rotation Speed", Range(-5, 5)) = 1.0
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0
        _NoiseSpeed ("Noise Speed", Range(0, 5)) = 1.0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.3
        _FresnelPower ("Fresnel Power", Range(0, 10)) = 3.0
        _Distortion ("Distortion Amount", Range(0, 0.5)) = 0.1
        _Instability ("Instability", Range(0, 2)) = 1.0
        _FlickerSpeed ("Flicker Speed", Range(0, 20)) = 10.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }
        
        LOD 200
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _CoreColor;
                float4 _EdgeColor;
                float _EmissionIntensity;
                float _RotationSpeed;
                float _NoiseScale;
                float _NoiseSpeed;
                float _PulseSpeed;
                float _PulseAmount;
                float _FresnelPower;
                float _Distortion;
                float _Instability;
                float _FlickerSpeed;
            CBUFFER_END
            
            // Hash function for noise
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }
            
            // Seamless 2D Noise function using modulo wrapping
            float noise(float2 p)
            {
                // Wrap coordinates to create seamless tiling
                float2 period = float2(1.0, 1.0);
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                // Sample with wrapped coordinates
                float2 i00 = fmod(i, period);
                float2 i10 = fmod(i + float2(1.0, 0.0), period);
                float2 i01 = fmod(i + float2(0.0, 1.0), period);
                float2 i11 = fmod(i + float2(1.0, 1.0), period);
                
                float a = hash(i00);
                float b = hash(i10);
                float c = hash(i01);
                float d = hash(i11);
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            // Fractal Brownian Motion for more complex noise
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for (int i = 0; i < 6; i++)
                {
                    value += amplitude * noise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }
            
            // Rotate UV coordinates
            float2 rotateUV(float2 uv, float rotation)
            {
                float2 center = float2(0.5, 0.5);
                float s = sin(rotation);
                float c = cos(rotation);
                
                uv -= center;
                float2 rotated = float2(
                    uv.x * c - uv.y * s,
                    uv.x * s + uv.y * c
                );
                rotated += center;
                
                return rotated;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);
                
                // Create rotating UVs
                float rotation = _Time.y * _RotationSpeed;
                float2 rotatedUV = rotateUV(input.uv, rotation);
                
                // Create multiple layers of rotating noise
                float2 noiseUV1 = rotatedUV * _NoiseScale + _Time.y * _NoiseSpeed * 0.1;
                float2 noiseUV2 = rotateUV(input.uv, -rotation * 0.7) * _NoiseScale * 1.3 + _Time.y * _NoiseSpeed * 0.15;
                float2 noiseUV3 = rotateUV(input.uv, rotation * 1.5) * _NoiseScale * 0.7 + _Time.y * _NoiseSpeed * 0.2;
                float2 noiseUV4 = rotateUV(input.uv, -rotation * 2.0) * _NoiseScale * 0.5 + _Time.y * _NoiseSpeed * 0.25;
                float2 noiseUV5 = rotatedUV * _NoiseScale * 1.8 - _Time.y * _NoiseSpeed * 0.12;
                
                float noise1 = fbm(noiseUV1);
                float noise2 = fbm(noiseUV2);
                float noise3 = fbm(noiseUV3);
                float noise4 = fbm(noiseUV4);
                float noise5 = fbm(noiseUV5);
                
                // Combine noise layers with more variation
                float combinedNoise = (noise1 + noise2 * 0.5 + noise3 * 0.3 + noise4 * 0.4 + noise5 * 0.25) / 2.45;
                
                // Add instability effects
                float erraticPulse = noise(float2(_Time.y * 2.3, _Time.y * 1.7)) * _Instability;
                float flicker = noise(float2(_Time.y * _FlickerSpeed, 0)) * 0.5 + 0.5;
                flicker = pow(flicker, 3.0) * _Instability;
                
                // Chaotic distortion spikes
                float spike = step(0.95, noise(float2(_Time.y * 0.5, 0))) * _Instability;
                float spikeIntensity = spike * (sin(_Time.y * 30.0) * 0.5 + 0.5) * 2.0;
                
                // Create radial gradient from center
                float2 centerUV = input.uv - 0.5;
                float radialDist = length(centerUV);
                float radialGradient = 1.0 - saturate(radialDist * 2.0);
                
                // Add distortion based on noise
                float distortion = (combinedNoise - 0.5) * _Distortion;
                radialGradient += distortion;
                radialGradient = saturate(radialGradient);
                
                // Pulsing effect with instability
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                pulse += erraticPulse * 0.3;
                pulse = saturate(pulse);
                float pulseEffect = lerp(1.0, 1.0 + _PulseAmount, pulse);
                pulseEffect += spikeIntensity;
                pulseEffect *= (1.0 - flicker * 0.4);
                
                // Mix core and edge colors based on radial distance and noise
                float colorMix = radialGradient * combinedNoise;
                colorMix += erraticPulse * 0.2;
                colorMix = saturate(colorMix);
                float3 reactorColor = lerp(_EdgeColor.rgb, _CoreColor.rgb, colorMix);
                
                // Add unstable color shifts
                float colorShift = noise(float2(_Time.y * 1.3, radialDist * 5.0)) * _Instability * 0.3;
                reactorColor += float3(colorShift, -colorShift * 0.5, colorShift * 0.3);
                
                // Apply pulsing to brightness
                reactorColor *= pulseEffect;
                
                // Fresnel effect for glowing edges
                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _FresnelPower);
                float3 fresnelGlow = _CoreColor.rgb * fresnel * 0.5;
                
                // Add animated energy lines
                float lines = sin((rotatedUV.x + rotatedUV.y) * 20.0 + _Time.y * 5.0) * 0.5 + 0.5;
                lines = pow(lines, 10.0) * combinedNoise;
                float3 lineColor = _CoreColor.rgb * lines * 2.0;
                
                // Combine all effects
                float3 finalColor = reactorColor + fresnelGlow + lineColor;
                finalColor *= _EmissionIntensity;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
        
        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Lit"
}

Shader "Custom/EnemySkin"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1, 0.2, 0.1, 0.8)
        _EmissionColor ("Emission Color", Color) = (1, 0.3, 0.15, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 1.5
        _Fresnel ("Fresnel Power", Range(0, 10)) = 3
        _FresnelColor ("Fresnel Color", Color) = (1, 0.4, 0.2, 1)
        _RimPower ("Rim Power", Range(0, 10)) = 4
        _RimColor ("Rim Color", Color) = (1, 0.5, 0.3, 1)
        _Distortion ("Surface Distortion", Range(0, 0.1)) = 0.02
        _NoiseScale ("Noise Scale", Range(0, 20)) = 5
        _TimeScale ("Animation Speed", Range(0, 2)) = 0.5
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
        ZWrite On
        Cull Back
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangentOS : TANGENT;
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
                float4 _Color;
                float4 _EmissionColor;
                float _EmissionIntensity;
                float _Fresnel;
                float4 _FresnelColor;
                float _RimPower;
                float4 _RimColor;
                float _Distortion;
                float _NoiseScale;
                float _TimeScale;
            CBUFFER_END
            
            // Simple noise function
            float noise(float3 pos)
            {
                return frac(sin(dot(pos, float3(12.9898, 78.233, 54.53))) * 43758.5453);
            }
            
            // 3D noise for surface distortion
            float noise3D(float3 pos)
            {
                float3 p = floor(pos);
                float3 f = frac(pos);
                f = f * f * (3.0 - 2.0 * f);
                
                return lerp(
                    lerp(lerp(noise(p + float3(0,0,0)), noise(p + float3(1,0,0)), f.x),
                         lerp(noise(p + float3(0,1,0)), noise(p + float3(1,1,0)), f.x), f.y),
                    lerp(lerp(noise(p + float3(0,0,1)), noise(p + float3(1,0,1)), f.x),
                         lerp(noise(p + float3(0,1,1)), noise(p + float3(1,1,1)), f.x), f.y), f.z);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                // Add subtle surface distortion based on time and position
                float3 worldPos = vertexInput.positionWS;
                float distortionNoise = noise3D(worldPos * _NoiseScale + _Time.y * _TimeScale);
                worldPos += normalInput.normalWS * distortionNoise * _Distortion;
                
                output.positionCS = TransformWorldToHClip(worldPos);
                output.positionWS = worldPos;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(worldPos);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                // Sample base texture
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Calculate lighting
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);
                
                // Basic diffuse lighting
                float NdotL = max(0, dot(normal, lightDir));
                float3 diffuse = mainLight.color * NdotL;
                
                // Fresnel effect for that glassy look
                float fresnel = pow(1.0 - max(0, dot(normal, viewDir)), _Fresnel);
                float3 fresnelColor = _FresnelColor.rgb * fresnel;
                
                // Rim lighting for the glowing edge effect
                float rim = 1.0 - max(0, dot(normal, viewDir));
                rim = pow(rim, _RimPower);
                float3 rimColor = _RimColor.rgb * rim;
                
                // Add some noise-based color variation for crystalline effect
                float3 worldPos = input.positionWS;
                float colorNoise = noise3D(worldPos * _NoiseScale * 0.5 + _Time.y * _TimeScale * 0.3);
                float3 noiseColor = lerp(_Color.rgb * 0.8, _Color.rgb * 1.2, colorNoise);
                
                // Combine all effects
                float3 baseColor = tex.rgb * noiseColor;
                float3 emission = _EmissionColor.rgb * _EmissionIntensity;
                
                // Final color composition
                float3 finalColor = baseColor + diffuse * 0.3 + fresnelColor + rimColor + emission;
                
                // Set to fully opaque
                float alpha = 1.0;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return float4(finalColor, alpha);
            }
            ENDHLSL
        }
        
        // Shadow pass for receiving shadows
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
        
        // Depth pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            
            ZWrite On
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Lit"
}

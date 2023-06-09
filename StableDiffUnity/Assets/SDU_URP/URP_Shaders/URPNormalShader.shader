Shader "Unlit/URPNormalShader"
{
    Properties
    { }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);
            float4x4 _ViewToWorld;

            half4 Frag(Varyings IN) : SV_Target
            {
                //float4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                float3 normals = SampleSceneNormals(IN.uv);
                float4 camNormals = float4(normals.xyz, 1);
                if (normals.x == 0 && normals.y == 0 && normals.z == 0) {
                    camNormals.z = 1;
                }
                else {
                    camNormals = mul(camNormals, _ViewToWorld);
                    float aLen = sqrt(camNormals.x * camNormals.x + camNormals.y * camNormals.y + camNormals.z * camNormals.z);
                    camNormals /= aLen;
                }

                half3 color = half3(camNormals.x, camNormals.y, camNormals.z) * 0.5 + 0.5; // *0.5 + 0.5;_ViewToWorld *

                return half4(color.b, color.g, color.r, 1);
            }

            ENDHLSL
        }
    }
}


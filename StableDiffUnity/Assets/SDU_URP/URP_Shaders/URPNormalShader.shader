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
                if (normals.z < 0)normals.z = -normals.z;
                if (normals.x == 0 && normals.y == 0 && normals.z == 0) {
                    normals.z = 1;
                }
                half4 color = 0;
                // IN.normal is a 3D vector. Each vector component has the range
                // -1..1. To show all vector elements as color, including the
                // negative values, compress each value into the range 0..1.
                color.rgb = half4(normals.z, normals.y, normals.x, 1) * 0.5 + 0.5;// *0.5 + 0.5;_ViewToWorld *

                return color;
            }

            ENDHLSL
        }
    }
}


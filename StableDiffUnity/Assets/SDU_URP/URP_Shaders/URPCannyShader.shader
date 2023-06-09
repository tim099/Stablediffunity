Shader "Unlit/URPCannyShader"
{
    Properties
    { 
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Dx("Dx", Float) = 1
		_Dy("Dy", Float) = 1
        _NormalWeight("NormalWeight", Float) = 1
        _DepthWeight("DepthWeight", Float) = 1
        _Threshold("Threshold", Float) = 0.5
        _Weight("Weight", Vector) = (1,1,1,1)
    }

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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);
float4x4 _ViewToWorld;
float _Dx;
float _Dy;
float _NormalWeight;
float _DepthWeight;
float _Threshold;
half4 _MainTex_TexelSize;
float4 _Weight;

half3 GetNormal(float2 uv)
{
    float3 normals = SampleSceneNormals(uv);
    float4 camNormals = float4(normals.xyz, 1);
    if (normals.x == 0 && normals.y == 0 && normals.z == 0)
    {
        camNormals.z = 1;
    }
    else
    {
        camNormals = mul(camNormals, _ViewToWorld);
        float aLen = sqrt(camNormals.x * camNormals.x + camNormals.y * camNormals.y + camNormals.z * camNormals.z);
        camNormals /= aLen;
    }
    camNormals = camNormals * 0.5 + 0.5;
    return camNormals.xyz;
}

half4 Frag(Varyings IN) : SV_Target
{
    float avgNormal = 0;
    
    {
        float3 c0 = GetNormal(IN.uv + half2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y));
        float3 c1 = GetNormal(IN.uv + half2(0, _MainTex_TexelSize.y));
        float3 c2 = GetNormal(IN.uv + half2(_MainTex_TexelSize.x, _MainTex_TexelSize.y));
        float3 c3 = GetNormal(IN.uv + half2(-_MainTex_TexelSize.x, 0));
        float3 c5 = GetNormal(IN.uv + half2(_MainTex_TexelSize.x, 0));
        float3 c6 = GetNormal(IN.uv + half2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
        float3 c7 = GetNormal(IN.uv + half2(0, -_MainTex_TexelSize.y));
        float3 c8 = GetNormal(IN.uv + half2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
    
        float3 GX = -_Dy * c0 + _Dy * c2 - _Dx * c3 + _Dx * c5 - _Dy * c6 + _Dy * c8;
        float3 GY = -_Dy * c0 + _Dy * c6 - _Dx * c1 + _Dx * c7 - _Dy * c2 + _Dy * c8;
        avgNormal = _Weight.x * sqrt(GX.x * GX.x + GY.x * GY.x) + _Weight.y * sqrt(GX.y * GX.y + GY.y * GY.y)
					+ _Weight.z * sqrt(GX.z * GX.z + GY.z * GY.z);
        avgNormal /= (_Weight.x + _Weight.y + _Weight.z);
    }
    float avgDepth = 0;
    {
        float c0 = SampleSceneDepth(IN.uv + half2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y));
        float c1 = SampleSceneDepth(IN.uv + half2(0, _MainTex_TexelSize.y));
        float c2 = SampleSceneDepth(IN.uv + half2(_MainTex_TexelSize.x, _MainTex_TexelSize.y));
        float c3 = SampleSceneDepth(IN.uv + half2(-_MainTex_TexelSize.x, 0));
        float c5 = SampleSceneDepth(IN.uv + half2(_MainTex_TexelSize.x, 0));
        float c6 = SampleSceneDepth(IN.uv + half2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
        float c7 = SampleSceneDepth(IN.uv + half2(0, -_MainTex_TexelSize.y));
        float c8 = SampleSceneDepth(IN.uv + half2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
    
        float GX = -_Dy * c0 + _Dy * c2 - _Dx * c3 + _Dx * c5 - _Dy * c6 + _Dy * c8;
        float GY = -_Dy * c0 + _Dy * c6 - _Dx * c1 + _Dx * c7 - _Dy * c2 + _Dy * c8;
        avgDepth = sqrt(GX.x * GX.x + GY.x * GY.x);
    }
    //float avg = (avgNormal * _NormalWeight + avgDepth * _DepthWeight);// / (_NormalWeight + _DepthWeight);
    float totalWeight = _NormalWeight + _DepthWeight;
    if (totalWeight <= 0)
        totalWeight = 0.00001;
    float avg = avgNormal + avgDepth / totalWeight;
    
    
    
    if (avg >= _Threshold)
    {
        avg = 1;
    }
    else
    {
        avg = 0;
    }
    return half4(avg, avg, avg, 1);
}



            ENDHLSL
        }
    }
}


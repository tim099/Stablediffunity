// This shader fills the mesh shape with a color predefined in the code.
Shader "Example/URPUnlitShaderBasic"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { }

        // The SubShader block containing the Shader code.
        SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
        // The positionOS variable contains the vertex positions in object
        // space.
        float4 positionOS   : POSITION;
    };

    struct Varyings
    {
        // The positions in this struct must have the SV_POSITION semantic.
        float4 positionHCS  : SV_POSITION;
    };

    // The vertex shader definition with properties defined in the Varyings
    // structure. The type of the vert function must match the type (struct)
    // that it returns.
    Varyings vert(Attributes IN)
    {
        // Declaring the output object (OUT) with the Varyings struct.
        Varyings OUT;
        // The TransformObjectToHClip function transforms vertex positions
        // from object space to homogenous clip space.
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        // Returning the output.
        return OUT;
    }

    // The fragment shader definition.
    half4 frag(Varyings IN) : SV_Target
    {
        float2 UV = IN.positionHCS.xy / _ScaledScreenParams.xy;
#if UNITY_REVERSED_Z
        real depth = SampleSceneDepth(UV);
#else
        // Adjust z to match NDC for OpenGL
        real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
#endif
        // Defining the color variable and returning it.
        //half4 customColor = half4(UV.x, UV.y, depth, 1);
        //half4 customColor = half4(depth, depth, depth, 1);
        //return customColor;
        float3 worldPos = ComputeWorldSpacePosition(UV, depth, UNITY_MATRIX_I_VP);

        uint scale = 10;
        uint3 worldIntPos = uint3(abs(worldPos.xyz * scale));
        bool white = (worldIntPos.x & 1) ^ (worldIntPos.y & 1) ^ (worldIntPos.z & 1);
        half4 color = white ? half4(1, 1, 1, 1) : half4(0, 0, 0, 1);

        return color;
    }
    ENDHLSL
}
    }
}
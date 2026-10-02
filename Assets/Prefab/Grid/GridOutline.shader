Shader "Custom/GridOutline"
{
    Properties
    {
        [HDR]_OutlineColor(
            "Outline Color",
            Color) = (1, 1, 0, 1)

        _OutlineWidth(
            "Outline Width",
            Range(0.001, 0.25)) = 0.06

        _EdgeSoftness(
            "Edge Softness",
            Range(0.001, 0.05)) = 0.005
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "GridOutline"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)

            float4 _OutlineColor;
            float _OutlineWidth;
            float _EdgeSoftness;

            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(
                        input.positionOS.xyz);

                output.positionCS =
                    positionInputs.positionCS;

                output.uv = input.uv;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float leftDistance =
                    input.uv.x;

                float rightDistance =
                    1.0 - input.uv.x;

                float bottomDistance =
                    input.uv.y;

                float topDistance =
                    1.0 - input.uv.y;

                float horizontalDistance =
                    min(
                        leftDistance,
                        rightDistance);

                float verticalDistance =
                    min(
                        bottomDistance,
                        topDistance);

                float nearestEdge =
                    min(
                        horizontalDistance,
                        verticalDistance);

                float outlineMask =
                    1.0 -
                    smoothstep(
                        _OutlineWidth,
                        _OutlineWidth +
                        _EdgeSoftness,
                        nearestEdge);

                return half4(
                    _OutlineColor.rgb,
                    _OutlineColor.a *
                    outlineMask);
            }

            ENDHLSL
        }
    }
}
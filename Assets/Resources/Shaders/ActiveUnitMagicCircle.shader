Shader "Dungeon/ActiveUnitMagicCircle"
{
    Properties
    {
        [HDR]_Color(
            "Color",
            Color) = (1.8, 0.72, 0.08, 1)

        _RotationSpeed(
            "Rotation Speed",
            Range(-2, 2)) = 0.12

        _PulseSpeed(
            "Pulse Speed",
            Range(0, 10)) = 2.5

        _LineWidth(
            "Line Width",
            Range(0.002, 0.08)) = 0.018

        _Softness(
            "Softness",
            Range(0.001, 0.05)) = 0.008

        _Intensity(
            "Intensity",
            Range(0, 4)) = 1.4

        [HideInInspector]_StateIntensity(
            "State Intensity",
            Range(0, 3)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent+20"
        }

        Pass
        {
            Name "ActiveUnitMagicCircle"

            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define TWO_PI 6.28318530718

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

            float4 _Color;
            float _RotationSpeed;
            float _PulseSpeed;
            float _LineWidth;
            float _Softness;
            float _Intensity;
            float _StateIntensity;

            CBUFFER_END

            float Ring(
                float distanceFromCenter,
                float radius,
                float width,
                float softness)
            {
                return 1.0 - smoothstep(
                    width,
                    width + softness,
                    abs(distanceFromCenter - radius));
            }

            float RadialBand(
                float distanceFromCenter,
                float innerRadius,
                float outerRadius,
                float softness)
            {
                float innerMask = smoothstep(
                    innerRadius,
                    innerRadius + softness,
                    distanceFromCenter);

                float outerMask = 1.0 - smoothstep(
                    outerRadius,
                    outerRadius + softness,
                    distanceFromCenter);

                return innerMask * outerMask;
            }

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
                float2 centeredUV =
                    (input.uv - 0.5) * 2.0;

                float distanceFromCenter =
                    length(centeredUV);

                float angle = atan2(
                    centeredUV.y,
                    centeredUV.x);

                float time = _Time.y;
                float rotation =
                    time * _RotationSpeed;

                float outerRing = Ring(
                    distanceFromCenter,
                    0.88,
                    _LineWidth,
                    _Softness);

                float middleRing = Ring(
                    distanceFromCenter,
                    0.69,
                    _LineWidth * 0.8,
                    _Softness);

                float innerRing = Ring(
                    distanceFromCenter,
                    0.30,
                    _LineWidth * 0.75,
                    _Softness);

                float outerTicks =
                    1.0 - smoothstep(
                        0.04,
                        0.18,
                        abs(sin(
                            (angle + rotation) *
                            12.0)));

                outerTicks *= RadialBand(
                    distanceFromCenter,
                    0.73,
                    0.84,
                    _Softness);

                float innerSpokes =
                    1.0 - smoothstep(
                        0.025,
                        0.12,
                        abs(sin(
                            (angle - rotation * 0.75) *
                            3.0)));

                innerSpokes *= RadialBand(
                    distanceFromCenter,
                    0.34,
                    0.63,
                    _Softness);

                float hexagonRadius =
                    0.49 +
                    0.055 * cos(
                        (angle - rotation * 0.55) *
                        6.0);

                float rotatingHexagon = Ring(
                    distanceFromCenter,
                    hexagonRadius,
                    _LineWidth * 0.7,
                    _Softness);

                float centerGlow =
                    (1.0 - smoothstep(
                        0.0,
                        0.28,
                        distanceFromCenter)) *
                    0.16;

                float magicCircleMask = saturate(
                    outerRing +
                    middleRing +
                    innerRing +
                    outerTicks +
                    innerSpokes +
                    rotatingHexagon +
                    centerGlow);

                float outerFade =
                    1.0 - smoothstep(
                        0.92,
                        1.0,
                        distanceFromCenter);

                magicCircleMask *= outerFade;

                float pulse =
                    0.78 +
                    sin(time * _PulseSpeed) *
                    0.22;

                float finalAlpha =
                    magicCircleMask *
                    pulse *
                    _Color.a *
                    _StateIntensity;

                float3 finalColor =
                    _Color.rgb *
                    _Intensity *
                    _StateIntensity;

                return half4(
                    finalColor,
                    saturate(finalAlpha));
            }

            ENDHLSL
        }
    }
}

Shader "Bastiao/Original Sprite Palette"
{
    Properties
    {
        _MainTex ("Original Sprite", 2D) = "white" {}
        _PaletteMul ("Original multiply", Vector) = (1,1,1,1)
        _PaletteAdd ("Original additive", Vector) = (0,0,0,0)
        _Color ("Tint", Color) = (1,1,1,1)
        _NeutralBeret ("Preserve neutral beret", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            struct Attributes { COMMON_2D_INPUTS half4 color : COLOR; UNITY_SKINNED_VERTEX_INPUTS };
            struct Varyings { COMMON_2D_OUTPUTS half4 color : COLOR; };
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _PaletteMul;
                half4 _PaletteAdd;
                half4 _Color;
                half _NeutralBeret;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 texel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                #ifndef UNITY_COLORSPACE_GAMMA
                texel.rgb = LinearToSRGB(texel.rgb);
                #endif
                half3 original = texel.rgb;
                texel.rgb = saturate(texel.rgb * _PaletteMul.rgb + _PaletteAdd.rgb);
                texel.rgb = lerp(texel.rgb, original * .65h, _NeutralBeret * smoothstep(.42h, .60h, input.uv.y));
                #ifndef UNITY_COLORSPACE_GAMMA
                texel.rgb = SRGBToLinear(texel.rgb);
                #endif
                return texel * input.color;
            }
            ENDHLSL
        }
    }
}

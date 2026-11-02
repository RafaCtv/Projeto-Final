Shader "Bastiao/Street Surface"
{
    Properties { _MainTex("Original pavement", 2D)="white"{} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            struct A { float4 position:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct V { float4 position:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            V Vert(A i) { V o; o.position=TransformObjectToHClip(i.position.xyz);o.uv=i.uv;o.color=i.color;return o; }
            half4 Frag(V i):SV_Target { return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color; }
            ENDHLSL
        }
    }
}

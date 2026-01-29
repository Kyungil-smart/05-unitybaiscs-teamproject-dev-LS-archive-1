Shader "Unlit/SkyBox"
{
   Properties
    {
        _CubeA ("Skybox A", CUBE) = "" {}
        _CubeB ("Skybox B", CUBE) = "" {}
        _Blend ("Blend", Range(0,1)) = 0
        _Exposure ("Exposure", Range(0,8)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _CubeA;
            samplerCUBE _CubeB;
            float _Blend;
            float _Exposure;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert (float4 v : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v);
                o.dir = v.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 colA = texCUBE(_CubeA, i.dir);
                fixed4 colB = texCUBE(_CubeB, i.dir);
                return lerp(colA, colB, _Blend) * _Exposure;
            }
            ENDCG
        }
    }
}

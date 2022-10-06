Shader "Custom/RenderFeature/Pixelate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            float _Intensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                // Divide resolution by intensity to get square pixels
                float intensity = _Intensity / (_ScreenParams.x / _ScreenParams.y);
                float2 d = float2(intensity, (_MainTex_TexelSize.y / _MainTex_TexelSize.x) * intensity);

                // Step uv to get pixelation effect
                float2 uv = d * floor(input.uv / d);

                return tex2D(_MainTex, uv);
            }

            ENDCG
        }
    }
}
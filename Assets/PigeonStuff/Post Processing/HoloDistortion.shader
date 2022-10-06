Shader "Custom/RenderFeature/HoloDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("(X Intensity) (Y Speed) (ZY Aberration)", Vector) = (1, 1, 1, 1)
        _Damage ("Damage", Range(0, 1)) = 1
        _EdgeLineIntensity ("Edge Line Intensity", Range(0, 1)) = 1
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
            // sampler2D _CameraOpaqueTexture;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            float4 _Intensity;
            float _Damage;
            float _EdgeLineIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            uint hashUint(uint state)
            {
                state ^= 2747636419u;
                state *= 2654435769u;
                state ^= state >> 16;
                state *= 2654435769u;
                state ^= state >> 16;
                state *= 2654435769u;
                return state;
            }

            float hash(uint state)
            {
                state ^= 2747636419u;
                state *= 2654435769u;
                state ^= state >> 16;
                state *= 2654435769u;
                state ^= state >> 16;
                state *= 2654435769u;
                return state / 4294967295.0;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                float2 res = _MainTex_TexelSize.xy;
                //float2 uv = input.uv;

                //float k = 1.0 * 0.9;
                //float kcube = 0.5;
                //float offset = 0.1 * 0.5;

                //float2 uv = computeUV(input.uv, _Curve.x, _Curve.y);
                float2 uv = input.uv;//glitchUV(input.uv);

                // Jiggle uv randomly over time. Intensity.y is the frequency and Intensity.x is the amplitude
                uv.x += cos(_Time.y * hash(uv.y * 1000) * _Intensity.y) * (_Intensity.x * _Damage);
                uv.y += cos(_Time.y * hash(uv.x * 1000) * _Intensity.y) * (_Intensity.x * _Damage);

                // Curve parabola
                //uv.y += _Curve * (-2 * uv.y + 1) * uv.x * (uv.x - 1);

                // Scroll y lines over time
                float y = (uv.y + _Time.y * 0.005);

                // Get distance from center (so that y lines are more visible on the edges)
                float t = abs(uv.x - 0.5);

                // Blend between x uv and random value, by a random value
                uv.x = lerp(uv.x, hash(y * 2500), (hash(y * 2000) < _Damage ? t * t : 0) * _EdgeLineIntensity);

                // Sample with chromatic aberration
                fixed4 col = fixed4(tex2D(_MainTex, uv - _Intensity.zw * _Damage).r, tex2D(_MainTex, uv).g, tex2D(_MainTex, uv + _Intensity.zw * _Damage).b, 1);
                //float3 r = tex2D(_MainTex, uv - _Intensity.zw * _Damage);
                //r.gb *= 0.4;
                //float3 g = tex2D(_MainTex, uv);
                //g.rb *= 0.4;
                //float3 b = tex2D(_MainTex, uv + _Intensity.zw * _Damage);
                //b.rg *= 0.4;
                //fixed4 col = fixed4((r + b + g), 1);
                return col;
            }

            ENDCG
        }
    }
}
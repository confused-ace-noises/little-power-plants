Shader "Custom/HologramTint"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (0.2, 0.6, 1.0, 0.7)
        _TintIntense ("Intensity Tint", Range(0,1)) = 0.7 
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;
            float _TintIntense;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                float lum = dot(texColor.rgb, fixed3(0.299, 0.587, 0.114));

                // increase contrast
                lum = pow(lum, 100.0);

                fixed3 tinted = lum * _TintColor.rgb * 2.0;

                // blend with original texture
                fixed3 result = lerp(tinted, texColor.rgb, _TintIntense);

                return fixed4(result, texColor.a * _TintColor.a);
            }
            ENDCG
        }
    }
}
Shader "Custom/WhiteWipeOutShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1) // 기존 스프라이트 색상 (알파값 포함)
        _WipeAmount ("Wipe Amount", Range(0,1)) = 0 // 사라지는 정도 (0: 모두 보임, 1: 모두 사라짐)
        _FadeAmount ("Fade Amount", Range(0,1)) = 0 // 하얗게 변하는 정도 (0: 원래 색, 1: 흰색)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 알파 블렌딩 활성화

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // 버텍스 컬러를 받기 위함
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR; // 프래그먼트 쉐이더로 버텍스 컬러 전달
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _WipeAmount;
            float _FadeAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color; // 버텍스 컬러와 _Color 프로퍼티를 곱함
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color; // 텍스처 색상에 버텍스 컬러/틴트 적용

                // 하얗게 변하는 효과
                fixed3 whiteColor = lerp(col.rgb, fixed3(1,1,1), _FadeAmount);
                col.rgb = whiteColor;

                // 위에서 아래로 사라지는 효과 (y-axis 기준)
                // i.uv.y는 0 (아래)에서 1 (위)까지의 값을 가집니다.
                // _WipeAmount가 0이면 모두 보이고, 1이면 모두 사라지도록 합니다.
                // 예를 들어 _WipeAmount가 0.5일 때, y가 0.5보다 작으면 사라집니다.
                float currentAlpha = col.a;
                if (i.uv.y < _WipeAmount) // y-좌표가 WipeAmount보다 작으면 (아래쪽부터) 사라짐
                {
                    currentAlpha = 0; // 완전히 투명하게
                }
                
                col.a = currentAlpha; // 최종 알파값 적용
                
                return col;
            }
            ENDCG
        }
    }
}
Shader "Custom/WhiteWipeOutShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1) // ���� ��������Ʈ ���� (���İ� ����)
        _WipeAmount ("Wipe Amount", Range(0,1)) = 0 // ������� ���� (0: ��� ����, 1: ��� �����)
        _FadeAmount ("Fade Amount", Range(0,1)) = 0 // �Ͼ�� ���ϴ� ���� (0: ���� ��, 1: ���)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // ���� ���� Ȱ��ȭ

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // ���ؽ� �÷��� �ޱ� ����
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR; // �����׸�Ʈ ���̴��� ���ؽ� �÷� ����
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
                o.color = v.color * _Color; // ���ؽ� �÷��� _Color ������Ƽ�� ����
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color; // �ؽ�ó ���� ���ؽ� �÷�/ƾƮ ����

                // �Ͼ�� ���ϴ� ȿ��
                fixed3 whiteColor = lerp(col.rgb, fixed3(1,1,1), _FadeAmount);
                col.rgb = whiteColor;

                // ������ �Ʒ��� ������� ȿ�� (y-axis ����)
                // i.uv.y�� 0 (�Ʒ�)���� 1 (��)������ ���� �����ϴ�.
                // _WipeAmount�� 0�̸� ��� ���̰�, 1�̸� ��� ��������� �մϴ�.
                // ���� ��� _WipeAmount�� 0.5�� ��, y�� 0.5���� ������ ������ϴ�.
                float currentAlpha = col.a;
                if (i.uv.y < _WipeAmount) // y-��ǥ�� WipeAmount���� ������ (�Ʒ��ʺ���) �����
                {
                    currentAlpha = 0; // ������ �����ϰ�
                }
                
                col.a = currentAlpha; // ���� ���İ� ����
                
                return col;
            }
            ENDCG
        }
    }
}
Shader "Skybox/Space"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_StarThresh ("Star threshold", Range(0,1)) = 0.5
		_Sky ("Color color", Color) = (0,0,0,0)
		_StarA ("Star color A", Color) = (1,1,1,1)
		_StarB ("Star color B", Color) = (1,0,0,0)
		_TimeMult ("time mult", Float) = 0.1
		_ColorStartY ("Color start y", Float) = 1
		_ColorEndY ("Color end y", Float) = 1
    }
    SubShader
    {
        // No culling or depth
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off

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
				float3 eye : EYE;
            };

			fixed4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = v.uv;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.eye= normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
                return o;
            }

            sampler2D _MainTex;
			fixed _StarThresh;
			fixed4 _Sky;
			fixed4 _StarA;
			fixed4 _StarB;
			fixed _TimeMult;
			fixed _ColorStartY;
			fixed _ColorEndY;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv+fixed2(1,1)*_Time.x*_TimeMult);
				fixed4 starCol = lerp(_StarA,_StarB,i.uv.x);
				fixed eye = i.eye.y;
				starCol=lerp(_Sky,starCol,smoothstep(_ColorStartY,_ColorEndY,eye));
				col=lerp(starCol,_Sky,1-step(_StarThresh,col.r));
                return col;
            }
            ENDCG
        }
    }
}

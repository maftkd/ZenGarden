Shader "Unlit/ProgressGlow"
{
    Properties
    {
		_Color ("Main Color", Color) = (1,1,1,1)
		_IntenseColor ("Intense Color", Color) = (1,1,1,1)
		_Progress ("Progress", Range(0,1)) = 1
		_Width ("Glow Width", Range(0,1)) = 0.5
		_Intensity ("Intensity", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent""Queue"="Transparent" }
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            fixed4 _IntenseColor;
			fixed _Progress;
			fixed _Width;
			fixed _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed fudge=step(i.uv.x,0.01);
				clip(_Progress-i.uv.x-fudge*0.00001);
				fixed glow=abs(i.uv.y-0.5)*2;
				fixed glowAmount=1-smoothstep(_Width,1,glow);
				fixed intensity=(sin(_Time.y*1.75)+1)*0.5;
                fixed4 col = lerp(_Color,_IntenseColor,_Intensity*glowAmount*intensity);
				col.a=glowAmount;
                return col;
            }
            ENDCG
        }
    }
}

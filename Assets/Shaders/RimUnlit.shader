Shader "Unlit/RimUnlit"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_RimColor ("Rim Color", Color) = (0,0,0,0)
		_RimAmount ("Rim Amount", Vector) = (0.5,0.7,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
			fixed4 _RimColor;
			fixed4 _RimAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv=v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed rim = 1-smoothstep(_RimAmount.x,_RimAmount.y,i.uv.y);
				fixed4 col = rim*_RimColor+(1-rim)*_Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

Shader "Custom/ArpBall"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ColorB ("Color B", Color) = (1,1,1,1)
		_EmissionColor ("Emisison color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_PoleRadius ("Pole Radius", Range(0,0.4)) = 0.1
		_InnerRadius ("Inner Radius", Range(0,0.4)) = 0.05
		_NoiseReduction ("Noise reduction", Range(0,1)) = 0.5
		_LineWidth ("Line width", Float) = 0.1
		_Emission ("Emission", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 localPos;
        };

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.localPos = v.vertex.xyz;
		}

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _ColorB;
		fixed _PoleRadius;
		fixed _NoiseReduction;
		fixed _LineWidth;
		fixed _InnerRadius;
		fixed4 _EmissionColor;
		fixed4 _Emission;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed n = tex2D (_MainTex, IN.uv_MainTex).r;
			n=lerp(n,1,_NoiseReduction);
            //o.Albedo = c.rgb;
			fixed2 xDiff=fixed2(IN.localPos.z,IN.localPos.y);
			fixed2 yDiff=fixed2(IN.localPos.x,IN.localPos.z);
			fixed2 zDiff=fixed2(IN.localPos.x,IN.localPos.y);
			fixed xSqrDiff=dot(xDiff,xDiff);
			fixed ySqrDiff=dot(yDiff,yDiff);
			fixed zSqrDiff=dot(zDiff,zDiff);
			fixed xPole=step(xSqrDiff,_PoleRadius*_PoleRadius)*step(_InnerRadius*_InnerRadius,xSqrDiff);
			fixed yPole=step(ySqrDiff,_PoleRadius*_PoleRadius)*step(_InnerRadius*_InnerRadius,ySqrDiff);
			fixed zPole=step(zSqrDiff,_PoleRadius*_PoleRadius)*step(_InnerRadius*_InnerRadius,zSqrDiff);
			fixed pole=xPole+yPole+zPole;

			fixed xLine=step(abs(IN.localPos.x),_LineWidth);
			fixed yLine=step(abs(IN.localPos.y),_LineWidth);
			fixed zLine=step(abs(IN.localPos.z),_LineWidth);
			pole+=xLine+yLine+zLine;


			pole=saturate(pole);

			o.Albedo=lerp(_Color.rgb,_ColorB.rgb,pole)*n;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness*n;
			o.Emission=_EmissionColor.rgb*(xPole*_Emission.x+yPole*_Emission.y+zPole*_Emission.z);
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

Shader "Custom/CrackGlow"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_GlowColor ("Glow color", Color) = (1,1,1,1)
		_FillAmount ("Fill amount", Range(0,1)) = 0.5
		_GlowParams ("Glow params", Vector) = (0,1,1,1)
		_IntenseColor ("Intense Color", Color) = (1,1,1,1)
		_Intensity ("Intensity", Range(0,1)) = 0.5
		_GlowIntensity ("Glow intensity", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed4 _GlowColor;
		fixed _FillAmount;
		fixed4 _GlowParams;
		fixed4 _IntenseColor;
		fixed _Intensity;
		fixed _GlowIntensity;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			fixed noise = tex2D(_MainTex,IN.uv_MainTex-fixed2(1,1)*_Time.x*_GlowParams.z).r;
			fixed r = 0.5;
			fixed2 center = fixed2(0.5,0.5);
			fixed2 diff = IN.uv_MainTex-center;
			fixed theta = atan2(diff.y,diff.x);
			fixed fAngle=((_FillAmount)-0.5)*3.14152*2;
			fixed fLerp = 1-step(fAngle,theta);
			fLerp*=noise;
			fLerp*=smoothstep(_GlowParams.x,_GlowParams.y,IN.worldPos.y);
            // Albedo comes from a texture tinted by color
			fixed intensity=(sin(_Time.y*_GlowParams.w)+1)*0.5;
			fixed4 col = lerp(_GlowColor,_IntenseColor,_Intensity*intensity);
            o.Albedo = lerp(_Color.rgb,col.rgb,fLerp);
			o.Emission=fLerp*col.rgb*_GlowIntensity;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

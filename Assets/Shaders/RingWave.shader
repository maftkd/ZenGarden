Shader "Custom/RingWave"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_RingParams ("Ring params (x,y,thick,.)", Vector) = (0.5,0.5,0.1,0)
		_Radius ("Ring Radius", Range(0,1)) = 0.5
		_NoiseCutoff ("Noise cutoff", Range(0,1)) = 0.1
		_NoiseMult ("Noise mult", Float) = 1
		_NoiseSpeed ("Noise speed", Float) = 0.5
		_WaveFreq ("Wave frequency", Float) = 2
		_WaveAmp ("Wave amplitude", Range(0,1)) = 0.1
		_WaveWave ("Wave wave", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

		fixed4 _RingParams;
        fixed4 _Color;
		fixed _Radius;
		fixed _NoiseMult;
		fixed _NoiseCutoff;
		fixed _NoiseSpeed;
		fixed _WaveFreq;
		fixed _WaveAmp;
		fixed _WaveWave;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed n = tex2D (_MainTex, (IN.uv_MainTex+fixed2(_Time.x,_Time.x)*_NoiseSpeed)*_NoiseMult).r;
			fixed4 c = _Color;
			fixed2 center = fixed2(_RingParams.x,_RingParams.y);
			fixed2 diff = center-IN.uv_MainTex;
			fixed dSqr=dot(diff,diff);
			fixed t = atan2(diff.y,diff.x);
			fixed r = _Radius+sin(t*_WaveFreq)*_WaveAmp*sin(_Time.z*_WaveWave);

			fixed dDiff=abs(r-sqrt(dSqr));
			fixed ringThickness=_RingParams.z;
			fixed inRing=step(0,ringThickness-dDiff);
			//fixed metal = n*
			//clip(inRing-0.9-n*_NoiseCutoff);
			clip(ringThickness-dDiff-n*_NoiseCutoff);

            o.Albedo = c.rgb;
			//o.Emission=c.rgb*_EmissionMult;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0.1;
            o.Smoothness = 0.1;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

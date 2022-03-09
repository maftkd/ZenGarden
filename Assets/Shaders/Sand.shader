Shader "Custom/Sand"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_SandA ("Sand color A", Color) = (1,1,1,1)
		_SandB ("Sand Color B", Color) = (0,0,0,0)
		_DropVec ("Drop Vector (x,y,r,w)", Vector) = (0.5,0.5,0.5,0.1)
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _Noise ("Noise ", 2D) = "white" {}
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
        sampler2D _Noise;

        struct Input
        {
            float2 uv_MainTex;
			float3 localPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed4 _SandA;
		fixed4 _SandB;
		fixed4 _DropVec;
		fixed4 _RingColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.localPos = v.vertex.xyz;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Smoothness = _Glossiness*c.r;
            o.Metallic = _Metallic*c.r;
			fixed col=step(1,IN.uv_MainTex.x);
			c*=lerp(_SandA,_SandB,col);
            o.Albedo = c.rgb;
			//drop vec
			fixed ringN = tex2D (_Noise, IN.localPos.xz*0.25+fixed2(1,1)*_Time.x).r; 
			ringN=step(0.5,ringN);
			fixed2 diff = _DropVec.xy-IN.localPos.xz;
			fixed dist = length(diff);
			fixed inRing=step(dist,_DropVec.z);
			fixed onEdge=step(abs(dist-_DropVec.z),_DropVec.w);
			fixed ring = smoothstep(0,_DropVec.z,dist)*inRing*0.25;
			fixed wtf=onEdge*ringN;
			o.Emission=_RingColor.rgb*wtf;
            // Metallic and smoothness come from slider variables
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

Shader "Hidden/Glow"
{
	Properties
	{
		_ColorTint("Color Tint", Color) = (0,0,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" ()
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimPower("Rim Power", Range(1.0,10.0)) = 3.0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			#pragma surface surf lambert

			struct Input
			{
				float4 Color : Color;
				float2 uv_MainTex;
				float2 uv_BumpMap;
				float3 viewDir;
			};

		float4 _ColorTint;
		sampler2D _MainTes;
		sampler2D _BumpMap;
		float4 _RimColor;
		float _RimPower;

		void surf(Input IN, inout SurfaceOutput o)
		{
			IN.color = _ColorTint;
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * IN.color;
			o.Normal = UnpackNormal(tex2d(_BumpMap, IN.uv_BumpMap));

			half rim = 1.0 - saturate(dot(normalize(IN.viewDir, o.Normal)));
			0.Emission = _RimColor.rgb * pow(rim, _RimPower);
		}


			ENDCG
		}
			fallback "Diffuse";
	}
}

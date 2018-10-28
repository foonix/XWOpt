﻿Shader "XwOptUnity/TextureAtlas"
{
	Properties
	{
		_MainTex("Albido", 2D) = "white" {}
		_MainTex2("Subtexture Bottom Left", 2D) = "white" {}
		_MainTex3("Subtexture Top RIght", 2D) = "white" {}
		_EmissionMap("Emission Map", 2D) = "black" {}
		_EmissionBrightness("Emission Brightness", float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		CGPROGRAM
		#pragma surface surf Standard

		struct Input {
		  float2 uv_MainTex : TEXCOORD0;
		  float2 uv2_MainTex2;
		  float2 uv3_MainTex3;
	    };
		sampler2D _MainTex;
		sampler2D _EmissionMap;
		float _EmissionBrightness;
		void surf(Input IN, inout SurfaceOutputStandard o) {
			float x = (frac(IN.uv_MainTex.x) * IN.uv3_MainTex3.x) + IN.uv2_MainTex2.x;
			float y = (frac(IN.uv_MainTex.y) * IN.uv3_MainTex3.y) + IN.uv2_MainTex2.y;
			o.Albedo = tex2D(_MainTex, float2(x, y));
			o.Emission = tex2D(_EmissionMap, float2(x, y)) * _EmissionBrightness;
		}
		ENDCG
	}
}

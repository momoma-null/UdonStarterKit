// Copyright (c) 2022 momoma
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

Shader "MomomaShader/Fragment/TransitionMirror"
{
	Properties
	{
		_MainTex ("Transition Map", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		[PerRendererData] _ReflectionTex0("", 2D) = "white" {}
		[PerRendererData] _ReflectionTex1("", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }
		Pass
		{
			AlphaToMask On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			UNITY_DECLARE_TEX2D(_MainTex);
			float4 _MainTex_ST;
			half4 _Color;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_ReflectionTex0);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_ReflectionTex1);

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 refl : TEXCOORD1;
				nointerpolation float scaleZ : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.refl = ComputeNonStereoScreenPos(o.pos);
				float3 scale = float3(length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz));
				o.uv = v.vertex.xy / scale.xy;
				o.uv = TRANSFORM_TEX(o.uv, _MainTex);
				o.scaleZ = 1.0 / scale.z;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				float alphaClip = saturate(i.scaleZ - 1);
				float alpha = saturate(UNITY_SAMPLE_TEX2D(_MainTex, i.uv).r) * 0.95 + 0.05 - alphaClip;
				float4 refl = UNITY_PROJ_COORD(i.refl);
				float2 reflUV = refl.xy / refl.w;
				float4 c = unity_StereoEyeIndex == 0 ? UNITY_SAMPLE_TEX2D_SAMPLER(_ReflectionTex0, _MainTex, reflUV) : UNITY_SAMPLE_TEX2D_SAMPLER(_ReflectionTex1, _MainTex, reflUV);
				c.a = saturate(alpha / max(fwidth(saturate(alpha + 1.0 - alphaClip)), 0.0001) + 0.5);
				c.rgb = lerp(_Color.rgb, c.rgb, saturate(alpha * 20.0));
				return c;
			}
			ENDCG
		}
	}
}
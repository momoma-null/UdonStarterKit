// Copyright (c) 2021 momoma
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

Shader "MomomaShader/Sprite/MSDF"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_BackgroundColor ("Background Color", Color) = (0,0,0,0)
		_PixelRange ("Pixel Range", Range(0.01, 1.0)) = 0.5
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile_local _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			#include "UnitySprites.cginc"

			half4 _BackgroundColor;
			half _PixelRange;
			float4 _MainTex_TexelSize;

			inline float median(float r, float g, float b)
			{
				return max(min(r, g), min(max(r, g), b));
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 uv = IN.texcoord - 0.5;
				float2 uvTex = uv * 1.61;
				float3 sample = tex2D(_MainTex, uvTex + 0.5);
				float dist = median(sample.r, sample.g, sample.b) - 0.5;
				half4 color = IN.color;
				color = lerp(_BackgroundColor, color, saturate(dist * max(1.0, 0.5 * dot(_MainTex_TexelSize.xy / _PixelRange, 1.0 / fwidth(uvTex))) + 0.5 - (abs(uvTex.x) > 0.5) - (abs(uvTex.y) > 0.5)));
				float rr = dot(uv, uv);
				color.a *= saturate((0.23 - rr) * 50.0);
				#if ETC1_EXTERNAL_ALPHA
					fixed alpha = tex2D(_AlphaTex, IN.texcoord).r;
					color.a = lerp(color.a, alpha, _EnableExternalAlpha);
				#endif
				color.rgb *= color.a;
				return color;				
			}
			ENDCG
		}
	}
}

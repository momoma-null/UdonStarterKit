// Copyright (c) 2022 momoma
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

Shader "MomomaShader/Fragment/Marble"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_SecondColor ("2nd Color", Color) = (0,0,0,1)
		_NoiseScale ("Noise Scale", Float) = 5.0
	}
	SubShader
	{
		Tags { "DisableBatching" = "True" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 oPos : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			half4 _Color, _SecondColor;
			half _NoiseScale;

			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.oPos = v.vertex.xyz;
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			float snoise(float3 uv, float res)
			{
				const float3 s = float3(1e0, 1e2, 1e3);

				uv *= res;

				float3 uv0 = floor(fmod(uv, res)) * s;
				float3 uv1 = floor(fmod(uv + 1.0, res)) * s;

				float3 f = frac(uv);
				f = f * f * (3.0 - 2.0 * f);

				float4 v = float4(
					uv0.x+uv0.y+uv0.z,
					uv1.x+uv0.y+uv0.z,
					uv0.x+uv1.y+uv0.z,
					uv1.x+uv1.y+uv0.z);

				float4 r0 = frac(sin(v * 1e-1) * 1e3);
				float4 r1 = frac(sin((v + uv1.z - uv0.z) * 1e-1) * 1e3);
				float v0 = lerp(lerp(r0.x, r0.y, f.x), lerp(r0.z, r0.w, f.x), f.y);
				float v1 = lerp(lerp(r1.x, r1.y, f.x), lerp(r1.z, r1.w, f.x), f.y);

				return lerp(v0, v1, f.z);
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float f = snoise(IN.oPos + _Time.x / _NoiseScale, _NoiseScale) * 0.5;
				f += snoise(IN.oPos + 2.3 * _SinTime.z / _NoiseScale, _NoiseScale * 2) * 0.25;
				f += snoise(IN.oPos + 1.7 * _CosTime.z / _NoiseScale, _NoiseScale * 4) * 0.25;
				float3 color = lerp(_SecondColor.rgb, _Color.rgb, f);
				return float4(color, 1.0);
			}
			ENDCG
		}
	}
}

// Copyright (c) 2022 momoma
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

Shader "MomomaShader/Fragment/FakeSphere"
{
	Properties
	{
		_RimPower ("Rim Power", Range(0.005, 1.0)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" "DisableBatching" = "True" }
		Pass
		{
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 objPos : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			half _RimPower;
			
			v2f vert (appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.objPos = v.vertex;
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float2 sphereDist(float3 r0, float3 rd, float3 c, float r)
			{
				float3 v = c - r0;
				float b = dot(v, rd);
				float d = b * b + r * r - dot(v, v);
				float h = sqrt(max(d, 0.0));
				return d < 0 ? -1.0 : float2(b - h, b + h);
			}
			
			// --------------------------------
			// main image
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 c = 1.0;
				float3 r0 = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
				float3 rd = normalize(i.objPos - r0);

				float2 sd = sphereDist(r0, rd, 0.0, 0.495);
				c.a = !(sd.y < 0.0);
				c.a *= saturate(1.0 - pow(sd.y - sd.x, _RimPower));

				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}
			ENDCG
		}
	}
}

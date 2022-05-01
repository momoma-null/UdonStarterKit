Shader "StarterKit/RoundedTrail"
{
	Properties
	{
		_Color ("Solid Color", Color) = (1,1,1,1)
		_Invisible ("Invisible Length", Float) = 1.0
		_Width ("Width", Float) = 0.03
	}
	SubShader
	{
		Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
		Cull Off
		AlphaToMask On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct g2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float4 _Color;
			float _Width;
			float _Invisible;

			appdata vert (appdata v)
			{
				return v;
			}

			[maxvertexcount(10)]
			void geom(triangle appdata IN[3], inout TriangleStream<g2f> stream)
			{
				UNITY_SETUP_INSTANCE_ID(IN[0]);
				g2f o;
				UNITY_INITIALIZE_OUTPUT(g2f, o);
				UNITY_TRANSFER_INSTANCE_ID(IN[0], o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.color = (IN[0].color + IN[1].color + IN[2].color) / 3.0;
				if(IN[0].uv.x + IN[2].uv.x > IN[1].uv.x * 2) return;

				float4 vp1 = UnityObjectToClipPos(IN[0].vertex);
				float4 vp2 = UnityObjectToClipPos(IN[1].vertex);
				float2 vd = vp1.xy / vp1.w - vp2.xy / vp2.w;
				vd /= float2(UNITY_MATRIX_P[0][0], UNITY_MATRIX_P[1][1]);
				float d = length(vd);
				vd = d < 0.0001 ? float2(1,0) : vd / d;
				float2 vz = _Width * float2(-UNITY_MATRIX_P[0][0], UNITY_MATRIX_P[1][1]);
				float2 vn = vd.yx * vz;

				if(length(IN[1].vertex.xyz - IN[0].vertex.xyz) < _Invisible)
				{
					o.uv = float2(0,-1);
					o.vertex = vp1+float4(+vn,0,0);
					stream.Append(o);
					o.uv = float2(0,1);
					o.vertex = vp1+float4(-vn,0,0);
					stream.Append(o);
					o.uv = float2(0,-1);
					o.vertex = vp2+float4(+vn,0,0);
					stream.Append(o);
					o.uv = float2(0,1);
					o.vertex = vp2+float4(-vn,0,0);
					stream.Append(o);
					stream.RestartStrip();
				}

				if(IN[1].uv.x >= 0.999999)
				{
					o.uv = float2(0,1);
					o.vertex = vp2+float4(o.uv*vz,0,0);
					stream.Append(o);
					o.uv = float2(-0.9,-0.5);
					o.vertex = vp2+float4(o.uv*vz,0,0);
					stream.Append(o);
					o.uv = float2(0.9,-0.5);
					o.vertex = vp2+float4(o.uv*vz,0,0);
					stream.Append(o);
					stream.RestartStrip();
				}

				o.uv = float2(0,1);
				o.vertex = vp1+float4(o.uv*vz,0,0);
				stream.Append(o);
				o.uv = float2(-0.9,-0.5);
				o.vertex = vp1+float4(o.uv*vz,0,0);
				stream.Append(o);
				o.uv = float2(0.9,-0.5);
				o.vertex = vp1+float4(o.uv*vz,0,0);
				stream.Append(o);
				stream.RestartStrip();
			}

			fixed4 frag (g2f i) : SV_Target
			{
				float l = length(i.uv);
				float alpha = saturate(1 - l);
				alpha = saturate((alpha - 0.5) / max(fwidth(alpha), 1e-3) + 0.5);
				return float4(_Color.rgb * i.color.rgb, alpha);
			}
			ENDCG
		}
	}
}

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ZucconiShader"
{
	Properties
	{
		_Centre("Center", Vector) = (0,0,0)
		_Radius("Radius", Float) = 1
		_MinDistance("Minimum Distance", Float) = 1
		_Steps("Resolution", Int) = 5
		_Color("Color", Color) = (1,0,0,1)
		_SpecularPower("Specular", Float) = 1
		_Gloss("Gloss", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			float3 _Centre;
			float _Radius;
			float _MinDistance;
			float _SpecularPower;
			float _Gloss;
			int _Steps;
			fixed4 _Color;

			struct v2f {
				float4 pos : SV_POSITION;	// Clip space
				float3 wPos : TEXCOORD1;	// World position
			};

			// Vertex function
			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}

			float sdf_smin(float a, float b, float k = 32)
			{
				float res = exp(-k*a) + exp(-k*b);
				return -log(max(0.0001, res)) / k;
			}

			float sdf_blend(float d1, float d2, float a)
			{
				return a * d1 + (1 - a) * d2;
			}

			float vmax(float3 v)
			{
				return max(max(v.x, v.y), v.z);
			}

			float sdf_cone(float3 p, float radius, float height) {
				float2 q = float2(length(p.xz), p.y);
				float2 tip = q - (0, height);
				float2 mantleDir = normalize((height, radius));
				float mantle = dot(tip, mantleDir);
				float d = max(mantle, -q.y);
				float projected = dot(tip, (mantleDir.y, -mantleDir.x));

				// distance to tip
				if ((q.y > height) && (projected < 0)) {
					d = max(d, length(tip));
				}

				// distance to base ring
				if ((q.x > radius) && (projected > length(float2(height, radius)))) {
					d = max(d, length(q - float2(radius, 0)));
				}
				return d;
			}

			float sdf_box(float3 p, float3 c, float3 s)
			{
				return vmax(abs(p - c) - s);
			}

			float sdf_sphere(float3 p, float3 c, float r)
			{
				return distance(p, c) - r;
			}
			
			float map(float3 p)
			{
				//return distance(p, _Centre) - _Radius;
				return sdf_blend
					(
						sdf_sphere(p, 0, 3),
						sdf_box(p, 0, 1),
						(_SinTime[3] + 1.) / 2.
						);
			}

			fixed4 simpleLambert(fixed3 normal, float3 viewDirection) {
				fixed3 lightDir = _WorldSpaceLightPos0.xyz;	// Light direction
				fixed3 lightCol = _LightColor0.rgb;		// Light color

				fixed NdotL = max(dot(normal, lightDir), 0);
				fixed4 c;
				// Specular
				fixed3 h = (lightDir - viewDirection) / 2.;
				fixed s = pow(dot(normal, h), _SpecularPower) * _Gloss;
				c.rgb = _Color * lightCol * NdotL + s;
				c.a = 1;
				//c.rgb = _Color * lightCol * NdotL;
				//c.a = 1;
				return c;
			}

			float3 normal(float3 p)
			{
				const float eps = 0.01;

				return normalize
					(float3
						(map(p + float3(eps, 0, 0)) - map(p - float3(eps, 0, 0)),
							map(p + float3(0, eps, 0)) - map(p - float3(0, eps, 0)),
							map(p + float3(0, 0, eps)) - map(p - float3(0, 0, eps))
							)
						);
			}
			fixed4 renderSurface(float3 p, float3 d)
			{
				float3 n = normal(p);
				return simpleLambert(n, d);
			}

			



			fixed4 raymarch(float3 position, float3 direction)
			{
				for (int i = 1; i < _Steps+1; i++)
				{
					float distance = map(position);
					if (distance < _MinDistance)
						return renderSurface(position, direction);

					position += distance * direction;
				}
				return fixed4(1, 1, 1, 1);
			}

			// Fragment function
			fixed4 frag(v2f i) : SV_Target
			{
				float3 worldPosition = i.wPos;
				float3 viewDirection = normalize(i.wPos - _WorldSpaceCameraPos);
				return raymarch(worldPosition, viewDirection);
			}

			
			ENDCG
		}
	}
}

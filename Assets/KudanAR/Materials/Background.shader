Shader "Kudan/Background"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Background" }
		LOD 100
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			float2 ScreenParams;

			float2 ScaleZoomToFit(float targetWidth, float targetHeight, float sourceWidth, float sourceHeight)
			{
				float sourceAspect = sourceWidth / sourceHeight;
				float targetAspect = targetWidth / targetHeight;

				float2 scale;

				if (targetAspect > 1.0)
				{
					scale = float2(1.0, targetAspect / sourceAspect);
				}

				else
				{
					scale = float2(1.0 / (sourceAspect * targetAspect), 1.0);
				}

				return scale;
			}
			
			v2f vert (appdata v)
			{
				v2f o;

				float2 scale = ScaleZoomToFit(_ScreenParams.x, _ScreenParams.y, _MainTex_TexelSize.z, _MainTex_TexelSize.w);
				
				float2 pos = (v.vertex * scale * 2.0);		
				o.vertex = float4(pos, 0, 1);

				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = v.uv;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
//				return float4(i.uv.xy, 0, 1) * col;
				return col;
			}
			ENDCG
		}
	}
}

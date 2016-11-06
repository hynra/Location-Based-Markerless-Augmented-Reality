Shader "Kudan/YpCbCrShader" {
	Properties {
		Yp("Yp", 2D) = "black" {}
		CbCr("CbCr", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Background" }
		LOD 100
		Cull Off
		ZWrite Off
		ZTest Always
		
		Pass {
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D Yp;
			sampler2D CbCr;
			float4 Yp_ST;
			float4 Yp_TexelSize;

			float2 ScreenParams;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float2 scl_aspect_fit(float2 src, float2 dst)
			{
				float As = src.x / src.y;
				float Ad = dst.x / dst.y;
				if (As > Ad)
					// Fit width
					return float2(1.0, Ad / As);
				else
					// Fit height
					return float2(As / Ad, 1.0);
			}

			float2 scl_aspect_fill(float2 src, float2 dst)
			{
				float As = src.x / src.y;
				float Ad = dst.x / dst.y;
				if (Ad < 1.0)
					// Fit height
					return float2(1.0 / (Ad * As), 1.0);
				else
					// Fit width
					return float2(1.0, Ad / As);
			}
			
			v2f vert(appdata v) {
				v2f o;

				float2 scl = scl_aspect_fill(Yp_TexelSize.zw, _ScreenParams.xy);
				float2 pos = (v.vertex * scl * 2.0);
				o.vertex = float4(pos, 0.0, 1.0);
				o.uv = TRANSFORM_TEX(v.uv, Yp);
				
				return o;
			}

//			const float3 ycbcr_offs = float3(-0.0625, -0.5, -0.5);
//			const float3x3 ycbcr_xfrm = float3x3(
//				float3(1.164,  0.0,    1.793),
//				float3(1.164, -0.213, -0.533),
//				float3(1.164,  2.112,  0.0)
//			);

			// src: http://scc.qibebt.cas.cn/docs/compiler/intel/2011/ipp/ipp_manual/IPPI/ippi_ch6/functn_YCbCrToRGB.htm#functn_YCbCrToRGB
			const float3 ycbcr_offs = float3(-0.0625, -0.5, -0.5);
			const float3x3 ycbcr_xfrm = float3x3(
				float3(1.164,  0.0,    1.596),
				float3(1.164, -0.392, -0.813),
				float3(1.164,  2.017,  0.0)
			);

			float3 ycbcr2rgb(float3 yuv)
			{
				return mul(ycbcr_xfrm, yuv + ycbcr_offs);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float y = tex2D(Yp, i.uv);
				float4 uv = tex2D(CbCr, i.uv);
				#if SHADER_API_METAL
				float3 ycbcr = float3(y, uv.rg);
				#else
				float3 ycbcr = float3(y, uv.ra);
				#endif
				float3 col = ycbcr2rgb(ycbcr);
				return fixed4(fixed3(col), 1);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}

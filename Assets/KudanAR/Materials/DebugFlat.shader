Shader "Kudan/DebugFlat"
{
	SubShader
	{
		Tags {"Queue" = "Geometry" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 100
		Cull Off
		ZWrite Off
		ZTest Always
 		Lighting Off
 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 colour : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 colour : TEXCOORD0;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.colour = v.colour;
					
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.colour;
			}
			ENDCG
		}
	}
}

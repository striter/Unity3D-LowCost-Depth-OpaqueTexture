Shader "Unlit/OpaqueTexture"
{
	Properties
	{
		_Distort("Distort",Vector)=(1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent-1" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float2 screenUV:TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float4 _Distort;
			sampler2D _CameraOpaqueTexture;
			float4 _CameraOpaqueTexture_TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(o.vertex);
				o.screenUV = screenPos.xy / screenPos.w+_Distort.xy*sin(_Time.y)* _CameraOpaqueTexture_TexelSize;
				return o;
			}
			

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_CameraOpaqueTexture, i.screenUV);
				return col;
			}
			ENDCG
		}
	}
}

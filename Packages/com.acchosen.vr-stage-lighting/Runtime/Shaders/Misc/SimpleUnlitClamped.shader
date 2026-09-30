// Shader template is made by OwenTheProgrammer

Shader "VRSL/SimpleUnlitClamped"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"Queue"="Geometry"
		}

		Cull Off
		//ZWrite Off
		//ZClip False

		Pass
		{
			Name "VRSL/SimpleUnlitClamped"
			CGPROGRAM
			//#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			struct inputData
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;

				//float2 screenPos : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			/*
			float2 VR_ComputeScreenPos(float4 clip)
			{
				float flip = _ProjectionParams.x;
				float2 ndc = clip.xy * float2(0.5, 0.5 * flip);
				return TransformStereoScreenSpaceTex(ndc + 0.5 * clip.w, clip.w);
			}

			float VR_LinearEyeDepth(float2 screenPos, float depth)
			{
				float4 ndc = float4(screenPos, depth, 1);

				#ifdef UNITY_REVERSED_Z
					ndc.xyz = ndc.xyz * float3(2,2,-2) + float3(-1,-1,1);
				#else
					ndc.xyz = ndc.xyz * 2 - 1;
				#endif //UNITY_REVERSED_Z

				float4 clip = mul(unity_CameraInvProjection, ndc);
				return -clip.z / clip.w;
			}
			*/

			v2f vert(inputData i)
			{
				v2f o;

				
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_TRANSFER_INSTANCE_ID(i, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				

				o.vertex = UnityObjectToClipPos(i.vertex);

				o.uv = i.uv;
				//o.uv = TRANSFORM_TEX(i.uv, _MainTex);

				//o.screenPos = VR_ComputeScreenPos(o.vertex);

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half4 tex = tex2D(_MainTex, i.uv);
				

				return half4(clamp(tex.r, 0.0, 1.0), clamp(tex.g, 0.0, 1.0), clamp(tex.b, 0.0, 1.0), 1);

				//return 0;
			}
			ENDCG
		}
	}
}
Shader "LUTBeam/VRSL Spotlight AudioLink"
{
    Properties
    {
        [NoScaleOffset] _GoboTex ("Gobo Texture", 2DArray) = "white" {}
        [NoScaleOffset] _GoboLUT ("LUT Texture", 2DArray) = "white" {}

        [Header(Shape)]
        _Zoom ("_Zoom", Range(0, 2.0)) = 0.1
        _Offset ("_Offset", Range(-1,1)) = 0.25
        _NearRadiusX ("_NearRadiusX", Range(0,1)) = 0.1
        _NearRadiusY ("_NearRadiusY", Range(0,1)) = 0.1
        _FarZ ("_FarZ", Float) = 25
        _Gobo ("Gobo Index", Integer) = 0
        _GoboSpin ("Gobo Spin", Range(0, 1)) = 0
            
        [Header(Color)]
        _Emission ("Emission Color", Color) = (1, 1, 1, 1)
        _BeamIntensity ("_BeamIntensity", Range(0, 8.0)) = 1
        _BeamFalloff ("_BeamFalloff", Range(0, 3.0)) = 1
        _GoboIntensity ("_GoboIntensity", Range(0, 8.0)) = 1

		[Header(Audio Section)]
        [Toggle]_EnableAudioLink("Enable Audio Link", Float) = 0
        [Toggle] _EnableColorChord ("Enable Color Chord Tinting", Int) = 0
        _Band("Band", Float) = 0
        _BandMultiplier("Band Multiplier", Range(1, 15)) = 1
        _Delay("Delay", Float) = 0
        _NumBands("Num Bands", Float) = 4
        _AudioSpectrum("AudioSpectrum", 2D) = "black" {}
		[Toggle] _EnableThemeColorSampling ("Enable Theme Color Sampling", Int) = 0
		 _ThemeColorTarget ("Choose Theme Color", Int) = 0
    }
    SubShader
    {
        Tags {"RenderType"="Transparent" "Queue"="Transparent+303" }

        LOD 100

        Cull Back
        ZTest LEqual
        ZWrite Off

        Pass
        {
            Name "LUTBeam"
            Blend One One
            CGPROGRAM
            
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            // we use camera depth texture defined by LUTBeam.cginc
            #define CAMERA_DEPTH_TEXTURE
        
			#define GEOMETRY
			#define FIXTURE_EMIT
			#define VRSL_AUDIOLINK
			#ifndef UNITY_PASS_FORWARDBASE
			#define UNITY_PASS_FORWARDBASE
			#endif

            #include "Packages/com.llealloo.audiolink/Runtime/Shaders/AudioLink.cginc"
		    #include "Packages/com.acchosen.vr-stage-lighting/Runtime/Shaders/Shared/VRSL-Defines.cginc"
		    #include "Packages/com.acchosen.vr-stage-lighting/Runtime/Shaders/AudioLink/Shared/VRSL-AudioLink-Functions.cginc"

            Texture2DArray _GoboTex;
            Texture2DArray _GoboLUT;
            float _Offset;
            float _NearRadiusX;
            float _NearRadiusY;
            float _FarZ;
            float _Zoom;
            float _Gobo;

            float _GoboSpin;
                
            float _GoboIntensity;
            float _BeamIntensity;
            float _BeamFalloff;

            inline half getGobo()
            {
                return _Gobo;
            }

            #define LUTBEAM_CALLBACK_PROJECTION 1
            float3 LUTBeamCallbackProjection(SamplerState samp, float2 uv)
            {
                return _GoboTex.SampleLevel(samp, float3(uv, getGobo()), 0).rrr;
            }
            #define LUTBEAM_CALLBACK_VOLUME 1
            float3 LUTBeamCallbackVolume(SamplerState samp, float2 uv)
            {
                return _GoboLUT.SampleLevel(samp, float3(uv, getGobo()), 0).rrr;
            }
            
            // Example from LUTBeam.cginc
            // (Kotovsky) thank your for a nice example, it helped a lot! ^^
            #define LUTBEAM_CALLBACK_TRANSFORM 1
            float3x3 LUTBeamCallbackTransform(float3 vertex, inout float3 offset)
            {
                float goboSpin = _GoboSpin * _Time.g; // TODO: gobo spin timer instead
                float tilt = .0;
                float pan = .0;
                    
                float3x3 spinMatrix3 = float3x3(
                    cos(goboSpin), -sin(goboSpin), 0,
                    sin(goboSpin),  cos(goboSpin), 0,
                    0,              0,             1
                );

                float3x3 tiltMatrix3 = float3x3(
                    1, 0,           0,
                    0, cos(tilt), -sin(tilt),
                    0, sin(tilt),  cos(tilt)
                );

                float3x3 panMatrix3 = float3x3(
                    cos(pan), -sin(pan), 0,
                    sin(pan),  cos(pan), 0,
                    0,         0,        1
                );

                float3x3 combined = mul(spinMatrix3, mul(tiltMatrix3, panMatrix3));

                //offset = float3(cos(_Time.g), 0, sin(_Time.g)) * 5;

                return combined;
            }

            #include "Assets/LUTBeam/LUTBeam.cginc"
        
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                BeamData beam;
                float4 audioGlobalFinalConeIntensity : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


                float gi = getGlobalIntensity();
                float fi = getFinalIntensity();
                float amp = GetAudioReactAmplitude();
                //float coneWidth = getConeWidth();

                float4 color = getEmissionColor() * gi * fi * amp;
                half zoom = _Zoom;

                // simulate dimming that happens when the gobo is zoomed out
                float zoomFade = lerp(1, 0.1, 1-pow(1-saturate(zoom*0.5), 5));
                
                // make sure you feed in v.vertex from the unity default cube here directly without modifying it
                // otherwise things may go wroooonngggg :)
                o.beam = LUTBeamVert(v.vertex, zoom, zoom, _FarZ, _NearRadiusX, _NearRadiusY, _Offset, color * zoomFade, _BeamIntensity, _GoboIntensity, _BeamFalloff);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                float3 col = LUTBeamFrag(i.beam, _BeamFalloff);
                return float4(col, 0);
            }
            ENDCG
        }
    }
}

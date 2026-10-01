Shader "LUTBeam/VRSL Spotlight"
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
        _Color ("Color", Color) = (1, 1, 1, 1)
        _BeamIntensity ("_BeamIntensity", Range(0, 8.0)) = 1
        _BeamFalloff ("_BeamFalloff", Range(0, 3.0)) = 1
        _GoboIntensity ("_GoboIntensity", Range(0, 8.0)) = 1

        // VRSL stuff
        [Header(VRSL)]
		[Toggle] _EnableStrobe ("Enable Strobe", Int) = 0
		[Toggle] _EnableDMX ("Enable Stream DMX/DMX Control", Int) = 0
		_DMXChannel ("Starting DMX Channel", Int) = 0
		_MaxMinPanAngle("Max/Min Pan Angle (-x, x)", Float) = 180
		_MaxMinTiltAngle("Max/Min Tilt Angle (-y, y)", Float) = 180
		[HideInInspector][Toggle] _PanInvert ("Invert Mover Pan", Int) = 0
		[HideInInspector][Toggle] _TiltInvert ("Invert Mover Tilt", Int) = 0
		[HideInInspector]_FixtureBaseRotationY("Mover Pan Offset", Range(-540,540)) = 0
		[HideInInspector]_FixtureRotationX("Mover Tilt Offset", Range(-180,180)) = 0
		_FixtureRotationOrigin("Fixture Pivot Origin", Float) = (0, 0.014709, -1.02868, 0)
        
		_ConeWidth("Cone Width", Range(0,5.5)) = 0
		_FixtureMaxIntensity ("Maximum Cone Intensity",Range (0,1)) = 1
		_RedMultiplier ("Red Channel Multiplier", Range(0, 5)) = 1
		_GreenMultiplier ("Green Channel Multiplier", Range(0, 5)) = 1
		_BlueMultiplier ("Blue Channel Multiplier", Range(0,5)) = 1
        
        [Header(Stencil)]
        [IntRange] _StencilRef ("Ref", Range(0, 255)) = 142
        [IntRange] _StencilReadMask ("Read Mask", Range(0, 255)) = 255
        [IntRange] _StencilWriteMask ("Write Mask", Range(0, 255)) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Compare Function", Float) = 6
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("ZFail Op", Float) = 0
    }
    SubShader
    {
        Tags {"RenderType"="Transparent" "Queue"="Transparent+303" }

        Stencil
        {
            Ref [_StencilRef]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            Comp [_StencilCompareFunction]
            Pass [_StencilPassOp]
            Fail [_StencilFailOp]
            ZFail [_StencilZFailOp]
        }
        
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

            // for VRSL
            #ifndef UNITY_PASS_FORWARDBASE
            #define UNITY_PASS_FORWARDBASE
            #endif

            #define FIXTURE_EMIT
            #define VRSL_DMX
            #define PROJECTION_YES

            // we use camera depth texture defined by LUTBeam.cginc
            #define CAMERA_DEPTH_TEXTURE
            
            #include "Packages/com.acchosen.vr-stage-lighting/Runtime/Shaders/Shared/VRSL-Defines.cginc"
            #include "Packages/com.acchosen.vr-stage-lighting/Runtime/Shaders/Shared/VRSL-DMXFunctions.cginc"
			#include "Packages/com.acchosen.vr-stage-lighting/Runtime/Shaders/Shared/VRSL-Helpers.cginc"
            
            // LUTBeamSimple.shader
            Texture2DArray _GoboTex;
            Texture2DArray _GoboLUT;
            float _Offset;
            float _NearRadiusX;
            float _NearRadiusY;
            float _FarZ;
            float _Zoom;
            float _Gobo;

            float _GoboIntensity;
            float _BeamIntensity;
            float _BeamFalloff;

            inline half getGobo()
            {
                half dmxAlive = isDMX();
                return (dmxAlive * getDMXGoboSelection(getDMXChannel()) - 1) + ((1 - dmxAlive) * _Gobo);
            }
            

            #define LUTBEAM_CALLBACK_PROJECTION LUTBeamCallbackProjection
            float3 LUTBeamCallbackProjection(SamplerState samp, float2 uv, float mip)
            {
                return _GoboTex.SampleLevel(samp, float3(uv, getGobo()), mip).rrr;
            }
            #define LUTBEAM_CALLBACK_VOLUME LUTBeamCallbackVolume
            float3 LUTBeamCallbackVolume(SamplerState samp, float2 uv, float mip)
            {
                return _GoboLUT.SampleLevel(samp, float3(uv, getGobo()), mip).rrr;
            }
            
            // Example from LUTBeam.cginc
            // (Kotovsky) thank you for a nice example, it helped a lot! ^^
            #define LUTBEAM_CALLBACK_VERTEX LUTBeamCallbackTransform
            float3 LUTBeamCallbackTransform(float3 vertex)
            {
                uint dmx = getDMXChannel();
                float goboSpin = getGoboSpinSpeed(dmx);
                float tilt = GetTiltValue(dmx) - 90;
                float pan = GetPanValue(dmx);
                
                return calculateRotations(float4(0,0,1,0), float4(vertex,0), 0, pan, tilt).xyz;
            }

            #include "Packages/com.torvid21.lutbeam/Runtime/LUTBeam.cginc"
        
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
                
                BeamSettings settings = DefaultBeamSettings();
                //settings.zoomX = _Zoom;
                //settings.zoomY = _Zoom;
                settings.farz = _FarZ;
                settings.nearSizeX = _NearRadiusX;
                settings.nearSizeY = _NearRadiusY;
                //settings.color = _Color;
                settings.brightnessVolume = _BeamIntensity;
                settings.brightnessGobo = _GoboIntensity;
                settings.beamFalloff = _BeamFalloff;

                uint dmx = getDMXChannel();
                half zoom = isDMX() ? getValueAtCoords(dmx + 4, _Udon_DMXGridRenderTexture) : _Zoom;
                half dimmer = getValueAtCoords(dmx + 5, _Udon_DMXGridRenderTexture);
                float4 color = isDMX() ? GetDMXColor(dmx) * dimmer : 1.;
                
                // simulate dimming that happens when the gobo is zoomed out
                float zoomFade = lerp(1, 0.1, 1-pow(1-saturate(zoom*0.5), 5));
                
                
                // LUTBeamSimple.shader
                //float ex = settings.nearSizeX*10 + tan(radians(max(settings.zoomX/2, 1))) * settings.farz;
                //float ey = settings.nearSizeY*10 + tan(radians(max(settings.zoomY/2, 1))) * settings.farz;
                //float minWidth = 0.05;
                //settings.color *= 2 / pow(ex * ey + minWidth, 0.7);
                
                settings.zoomX = zoom;
                settings.zoomY = zoom;
                settings.color = color * zoomFade;
                
                // make sure you feed in v.vertex from the unity default cube here directly without modifying it
                // otherwise things may go wroooonngggg :)
                o.beam = LUTBeamVert(v.vertex, settings);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                float3 col = LUTBeamFrag(i.beam);
                return float4(col, 0);
            }
            ENDCG
        }
    }
}

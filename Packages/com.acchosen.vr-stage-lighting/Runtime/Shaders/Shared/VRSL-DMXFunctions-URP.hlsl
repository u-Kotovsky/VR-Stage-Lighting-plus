#ifndef VRSL_DMX_FUNCTIONS
#define VRSL_DMX_FUNCTIONS

#if defined(UNIVERSAL_RENDER_PIPELINE)

#ifndef VRSL_DMX
#define VRSL_DMX
#endif
#include "VRSL-Defines-URP.hlsl"

#define IF(a, b, c) lerp(b, c, step((float) (a), 0.0))

uint getDMXChannel()
{
    return (uint) round(UNITY_ACCESS_INSTANCED_PROP(Props, _DMXChannel));  
}

#ifndef LASER
uint checkPanInvertY()
{
    return (uint) UNITY_ACCESS_INSTANCED_PROP(Props, _PanInvert);
}
uint checkTiltInvertZ()
{
    return (uint) UNITY_ACCESS_INSTANCED_PROP(Props, _TiltInvert);
}
#endif

half getValueAtCoords(uint DMXChannel, TEXTURE2D_PARAM(tex, _DecodedTexture))
{
    uint channelIndex = DMXChannel;// - 1;

    if (channelIndex >= 16384) // 128*128 = 16384
        return 0.0h;

    uint x = channelIndex % 128;
    uint y = channelIndex / 128;

    float u = (x + 0.5) / 128.0; // center of pixel
    float v = (y + 0.5) / 128.0;
    
    float4 decoded = SAMPLE_TEXTURE2D_LOD(tex, _DecodedTexture, float4(u, v, 0, 0), 0);
    return decoded.r; // r is either fallback or new dmx data
}
half getValueAtCoordsRaw(uint DMXChannel, TEXTURE2D_PARAM(tex, _DecodedTexture))
{
    return getValueAtCoords(DMXChannel, TEXTURE2D_PARAM(tex, _DecodedTexture))
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(VOLUMETRIC_YES) || defined(PROJECTION_YES) || defined(FIXTURE_EMIT) || defined(FIXTURE_SHADOWCAST) || defined(VRSL_SURFACE) || defined(VRSL_FLARE)
    half getMinMaxPan()
    {
        return UNITY_ACCESS_INSTANCED_PROP(Props,_MaxMinPanAngle);
    }
    half getMinMaxTilt()
    {
        return UNITY_ACCESS_INSTANCED_PROP(Props,_MaxMinTiltAngle);
    }
#endif

uint isDMX()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_EnableDMX);
}
#ifndef LASER
uint isStrobe()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_EnableStrobe);
}

uint instancedGOBOSelection()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_ProjectionSelection);
}

half getOffsetX()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_FixtureRotationX);
}

half getOffsetY()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_FixtureBaseRotationY);
}

half getStrobeFreq()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_StrobeFreq);
}
#endif
half4 getEmissionColor()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_Emission);
}
#ifndef LASER
half getConeWidth()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_ConeWidth) - 1.0;
}

uint isGOBOSpin()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_EnableSpin);
}

half getConeLength()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _ConeLength);
}
half getMaxConeLength(uint DMXChannel)
{
    #ifdef VOLUMETRIC_YES
    half mcl = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxConeLength);
    return isDMX() == 1 && _EnableExtraChannels == 1 ? mcl + (getValueAtCoords(DMXChannel+1, TEXTURE2D_ARGS(_Udon_DMXGridRenderTexture, sampler_Udon_DMXGridRenderTexture)) * 4) : mcl;
    #else
    return UNITY_ACCESS_INSTANCED_PROP(Props, _MaxConeLength);
    #endif
}
#endif
half getGlobalIntensity()
{
    return lerp(1.0,UNITY_ACCESS_INSTANCED_PROP(Props, _GlobalIntensity), UNITY_ACCESS_INSTANCED_PROP(Props, _GlobalIntensityBlend));
}

half getFinalIntensity()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _FinalIntensity);
}
#ifndef LASER
half GetStrobeOutput(uint DMXChannel)
{
    half strobe = getValueAtCoords(DMXChannel + 6, TEXTURE2D_ARGS(_Udon_DMXGridStrobeOutput, sampler_Udon_DMXGridStrobeOutput));

    //check if we should even be strobing at all.
    strobe = IF(isDMX() == 1, strobe, 1);
    strobe = IF(isStrobe() == 1, strobe, 1);
    
    return strobe;
}

half GetStrobeOutputFiveCH(uint DMXChannel)
{
    half strobe = getValueAtCoords(DMXChannel + 4, TEXTURE2D_ARGS(_Udon_DMXGridStrobeOutput, sampler_Udon_DMXGridStrobeOutput));

    //check if we should even be strobing at all.
    strobe = IF(isDMX() == 1, strobe, 1);
    strobe = IF(isStrobe() == 1, strobe, 1);
    
    return strobe;
}

half getDMXGoboSelection(uint DMXChannel)
{
    half goboSelect = 30.0;

    #if defined(PROJECTION_MOVER) || defined (VOLUMETRIC_YES) 
        goboSelect = IF(UNITY_ACCESS_INSTANCED_PROP(Props, _LegacyGoboRange) > 0, 42.5, goboSelect);
    #endif

    uint value = round(((getValueAtCoords(DMXChannel + 11, TEXTURE2D_ARGS(_Udon_DMXGridRenderTexture, sampler_Udon_DMXGridRenderTexture)))*255)/goboSelect);
    value = isDMX() > 0.0 ? value : instancedGOBOSelection();
    return clamp(value, 1, 8) -0.1;
}

half getGoboSpinSpeed (uint DMXChannel)
{
    #if defined(PROJECTION_YES) || defined(VOLUMETRIC_YES)
        half status = getValueAtCoords(DMXChannel + 10, TEXTURE2D_ARGS(_Udon_DMXGridRenderTexture, sampler_Udon_DMXGridRenderTexture));
        half phase = getValueAtCoordsRaw(DMXChannel + 10, TEXTURE2D_ARGS(_Udon_DMXGridSpinTimer, sampler_Udon_DMXGridSpinTimer));
        phase = checkPanInvertY() == 1 ? -phase : phase;
        return status > 0.5 ? -phase * 4 : phase * 4;
    #endif
    return 0.0;
}

//function for getting the Intensity Value (Channel 6)
half GetDMXIntensity(uint DMXChannel, half multiplier)
{
    return getValueAtCoords(DMXChannel + 5, TEXTURE2D_ARGS(_Udon_DMXGridRenderTexture, sampler_Udon_DMXGridRenderTexture)) * multiplier;
}

half GetDMXChannel(uint DMXChannel)
{
    return getValueAtCoords(DMXChannel, TEXTURE2D_ARGS(_Udon_DMXGridRenderTexture, sampler_Udon_DMXGridRenderTexture));
}

//function for getting the Pan Value (Channel 2)
half GetFinePanValue(uint DMXChannel)
{
    return getValueAtCoords(DMXChannel+1, TEXTURE2D_ARGS(_Udon_DMXGridRenderTextureMovement, sampler_Udon_DMXGridRenderTextureMovement));
}
half GetPanValue(uint DMXChannel)
{
    half inputValue = getValueAtCoords(DMXChannel, TEXTURE2D_ARGS(_Udon_DMXGridRenderTextureMovement, sampler_Udon_DMXGridRenderTextureMovement));
    #if defined(VOLUMETRIC_YES) || defined(PROJECTION_YES) || defined(FIXTURE_EMIT) || defined(FIXTURE_SHADOWCAST) || defined(VRSL_SURFACE) || defined(VRSL_FLARE)
        return IF(isDMX() == 1, ((getMinMaxPan() * 2) * (inputValue)) - getMinMaxPan(), 0.0);
    #else
        return IF(isDMX() == 1, ((_MaxMinPanAngle * 2) * (inputValue)) - _MaxMinPanAngle, 0.0);
    #endif
}

half GetFineTiltValue(uint DMXChannel)
{
    return getValueAtCoords(DMXChannel+3, TEXTURE2D_ARGS(_Udon_DMXGridRenderTextureMovement, sampler_Udon_DMXGridRenderTextureMovement));
}

//function for getting the Tilt Value (Channel 3)
half GetTiltValue(uint DMXChannel)
{
    half inputValue = getValueAtCoords(DMXChannel + 2, TEXTURE2D_ARGS(_Udon_DMXGridRenderTextureMovement, sampler_Udon_DMXGridRenderTextureMovement));
    #if defined(VOLUMETRIC_YES) || defined(PROJECTION_YES) || defined(FIXTURE_EMIT) || defined(FIXTURE_SHADOWCAST) || defined(VRSL_SURFACE) || defined(VRSL_FLARE)
        return IF(isDMX() == 1, ((getMinMaxTilt() * 2) * (inputValue)) - getMinMaxTilt(), 0.0);
    #else
        return IF(isDMX() == 1, ((_MaxMinTiltAngle * 2) * (inputValue)) - _MaxMinTiltAngle, 0.0);
    #endif
}

//Function for getting the RGB Color Value (Channels 4, 5, and 6)
half4 GetDMXColor(uint DMXChannel)
{
    half redchannel = getValueAtCoords(DMXChannel + 7, TEXTURE2D_ARGS(_Udon_DMXGridRenderTexture, sampler_Udon_DMXGridRenderTexture));
    half greenchannel = getValueAtCoords(DMXChannel + 8, TEXTURE2D_ARGS(_Udon_DMXGridRenderTexture, sampler_Udon_DMXGridRenderTexture));
    half bluechannel = getValueAtCoords(DMXChannel + 9, TEXTURE2D_ARGS(_Udon_DMXGridRenderTexture, sampler_Udon_DMXGridRenderTexture));

    #if defined(PROJECTION_YES)
        redchannel = redchannel * _RedMultiplier;
        bluechannel = bluechannel * _BlueMultiplier;
        greenchannel = greenchannel * _GreenMultiplier;
    #endif

    return lerp(float4(0,0,0,1), half4(redchannel,greenchannel,bluechannel,1), GetDMXIntensity(DMXChannel, _FixtureMaxIntensity));
}

half getDMXConeWidth(uint DMXChannel) //Motor Speed Channel// CHANNEL 5
{
    half inputvalue = getValueAtCoords(DMXChannel + 4, TEXTURE2D_ARGS(_Udon_DMXGridRenderTexture, sampler_Udon_DMXGridRenderTexture));
    half DMXWidth = lerp(0, 5.5, inputvalue) - 1.5;
    return IF(isDMX() == 1, DMXWidth, getConeWidth());
}
#endif

#endif
#endif
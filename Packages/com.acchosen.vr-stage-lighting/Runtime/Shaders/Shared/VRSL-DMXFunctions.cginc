#define IF(a, b, c) lerp(b, c, step((fixed) (a), 0));

// Returns instanced _DMXChannel
uint getDMXChannel()
{
    return (uint) round(UNITY_ACCESS_INSTANCED_PROP(Props, _DMXChannel));  
}

#ifdef DMXTranslate
    uint getDMXTranslateChannel()
    {
        return (uint) round(UNITY_ACCESS_INSTANCED_PROP(Props, _DMXTranslateChannel));  
    }
    uint isDMXTranslateChannel()
    {
        return (uint) UNITY_ACCESS_INSTANCED_PROP(Props, _EnableDMXTranslateChannel);
    }
#endif

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

// Returns DMX value of pixel in Range(0, 1) ex.: 0, 0.2 .. 1
half getValueAtCoords(uint DMXChannel, sampler2D _DecodedTexture)
{
    uint channelIndex = DMXChannel;// - 1;

    if (channelIndex >= 16384) // 128*128 = 16384
        return 0.0h;

    uint x = channelIndex % 128;
    uint y = channelIndex / 128;

    float u = (x + 0.5) / 128.0; // center of pixel
    float v = (y + 0.5) / 128.0;
    
    float4 decoded = tex2Dlod(_DecodedTexture, float4(u, v, 0, 0));
    return decoded.r; // r is either fallback or new dmx data
}

// Returns DMX value of pixel in Range(0, 1) ex.: 0, 0.2 .. 1
half getValueAtCoordsRaw(uint DMXChannel, sampler2D _Tex)
{
    return getValueAtCoords(DMXChannel, _Tex);
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
    half getStartDMXChannel(int offset)
    {
        return UNITY_ACCESS_INSTANCED_PROP(Props, _DMXChannel) + offset;
    }
#endif

uint isDMX()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _EnableDMX);
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
        return isDMX() == 1 && _EnableExtraChannels == 1 ? mcl + (getValueAtCoords(DMXChannel+1, _Udon_DMXGridRenderTexture) * 4) : mcl;
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
        //half phase = getValueAtCoordsRaw(DMXChannel + 6, _Udon_DMXGridStrobeTimer);
        //half status = getValueAtCoords(DMXChannel + 6, _Udon_DMXGridRenderTexture);

        half strobe = getValueAtCoords(DMXChannel + 6, _Udon_DMXGridStrobeOutput);
        //half strobe = (sin(phase));//Get sin wave
        //strobe = IF(strobe > 0.0, 1.0, 0.0);//turn to square wave
        //strobe = saturate(strobe);

        //strobe = IF(status > 0.2, strobe, 1); //minimum channel threshold set
        
        //check if we should even be strobing at all.
        strobe = IF(isDMX() == 1, strobe, 1);
        strobe = IF(isStrobe() == 1, strobe, 1);
        
        return strobe;
    }

    half GetStrobeOutputFiveCH(uint DMXChannel)
    {
        // half phase = getValueAtCoordsRaw(DMXChannel + 4, _Udon_DMXGridStrobeTimer);
        // half status = getValueAtCoords(DMXChannel + 4, _Udon_DMXGridRenderTexture);

        half strobe = getValueAtCoords(DMXChannel + 4, _Udon_DMXGridStrobeOutput);
        // strobe = IF(strobe > 0.0, 1.0, 0.0);//turn to square wave
        //strobe = saturate(strobe);

        // strobe = IF(status > 0.2, strobe, 1); //minimum channel threshold set
        
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

        uint value = round(((getValueAtCoords(DMXChannel + 11, _Udon_DMXGridRenderTexture))*255)/goboSelect);
        value = isDMX() > 0.0 ? value : instancedGOBOSelection();
        return clamp(value, 1, 8) -0.1;
    }

    half getGoboSpinSpeed (uint DMXChannel)
    {
        #if defined(PROJECTION_YES) || defined(VOLUMETRIC_YES)
            half status = getValueAtCoords(DMXChannel + 10, _Udon_DMXGridRenderTexture);
            half phase = getValueAtCoordsRaw(DMXChannel + 10, _Udon_DMXGridSpinTimer);
            phase = checkPanInvertY() == 1 ? -phase : phase;
            return status > 0.5 ? -phase * 4 : phase * 4;
        #endif
        return 0.0;
    }

    //function for getting the Intensity Value (Channel 6)
    half GetDMXIntensity(uint DMXChannel, half multiplier)
    {
        return getValueAtCoords(DMXChannel + 5, _Udon_DMXGridRenderTexture) * multiplier;
    }

    half GetDMXChannel(uint DMXChannel)
    {
        return getValueAtCoords(DMXChannel, _Udon_DMXGridRenderTexture);
    }

    // Function for getting value from coarse and fine
    half GetValue(half coarse, half fine)
    {
        return coarse + (fine / 255.0);
    }

    //function for getting the Pan Value (Channel 2)
    half GetFinePanValue(uint DMXChannel)
    {
        return getValueAtCoords(DMXChannel + 1, _Udon_DMXGridRenderTextureMovement);
    }

    half GetPanValue(uint DMXChannel)
    {
        half coarse = getValueAtCoords(DMXChannel, _Udon_DMXGridRenderTextureMovement);
        half fine = GetFinePanValue(DMXChannel);
        half inputValue = GetValue(coarse, fine);

        #if defined(VOLUMETRIC_YES) || defined(PROJECTION_YES) || defined(FIXTURE_EMIT) || defined(FIXTURE_SHADOWCAST) || defined(VRSL_SURFACE) || defined(VRSL_FLARE)
            return IF(isDMX() == 1, ((getMinMaxPan() * 2) * (inputValue)) - getMinMaxPan(), 0.0);
        #else
            return IF(isDMX() == 1, ((_MaxMinPanAngle * 2) * (inputValue)) - _MaxMinPanAngle, 0.0);
        #endif
    }

    half GetFineTiltValue(uint DMXChannel)
    {
        return getValueAtCoords(DMXChannel + 3, _Udon_DMXGridRenderTextureMovement);
    }

    //function for getting the Tilt Value (Channel 3)
    half GetTiltValue(uint DMXChannel)
    {
        half coarse = getValueAtCoords(DMXChannel + 2, _Udon_DMXGridRenderTextureMovement);
        half fine = GetFineTiltValue(DMXChannel);
        half inputValue = GetValue(coarse, fine);
        #if defined(VOLUMETRIC_YES) || defined(PROJECTION_YES) || defined(FIXTURE_EMIT) || defined(FIXTURE_SHADOWCAST) || defined(VRSL_SURFACE) || defined(VRSL_FLARE)
            return IF(isDMX() == 1, ((getMinMaxTilt() * 2) * (inputValue)) - getMinMaxTilt(), 0.0);
        #else
            return IF(isDMX() == 1, ((_MaxMinTiltAngle * 2) * (inputValue)) - _MaxMinTiltAngle, 0.0);
        #endif
    }

    //Function for getting the RGB Color Value (Channels 4, 5, and 6)
    half4 GetDMXColor(uint DMXChannel)
    {
        half redchannel = getValueAtCoords(DMXChannel + 7, _Udon_DMXGridRenderTexture);
        half greenchannel = getValueAtCoords(DMXChannel + 8, _Udon_DMXGridRenderTexture);
        half bluechannel = getValueAtCoords(DMXChannel + 9, _Udon_DMXGridRenderTexture);

        #if defined(PROJECTION_YES)
            redchannel = redchannel * _RedMultiplier;
            bluechannel = bluechannel * _BlueMultiplier;
            greenchannel = greenchannel * _GreenMultiplier;
        #endif

        //return IF(isDMX() == 1,lerp(fixed4(0,0,0,1), half4(redchannel,greenchannel,bluechannel,1), GetDMXIntensity(DMXChannel, _FixtureMaxIntensity)), half4(redchannel,greenchannel,bluechannel,1) * GetDMXIntensity(DMXChannel, _FixtureMaxIntensity));
        return lerp(fixed4(0,0,0,1), half4(redchannel,greenchannel,bluechannel,1), GetDMXIntensity(DMXChannel, _FixtureMaxIntensity));
    }

    half getDMXConeWidth(uint DMXChannel) //Motor Speed Channel// CHANNEL 5
    {
        half inputvalue = getValueAtCoords(DMXChannel + 4, _Udon_DMXGridRenderTexture);
        half DMXWidth = lerp(0, 5.5, inputvalue) - 1.5;
        return IF(isDMX() == 1, DMXWidth, getConeWidth());
    }
#endif
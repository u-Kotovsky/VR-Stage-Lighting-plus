#define IF(a, b, c) lerp(b, c, step((fixed) (a), 0));

UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(uint, _PanInvert)
    UNITY_DEFINE_INSTANCED_PROP(uint, _LegacyGoboRange)
    UNITY_DEFINE_INSTANCED_PROP(uint, _TiltInvert)
    UNITY_DEFINE_INSTANCED_PROP(uint, _EnableStrobe)
    UNITY_DEFINE_INSTANCED_PROP(uint, _EnableSpin)
    UNITY_DEFINE_INSTANCED_PROP(uint, _EnableDMX)
    UNITY_DEFINE_INSTANCED_PROP(uint, _DMXChannel)
    UNITY_DEFINE_INSTANCED_PROP(uint, _ProjectionSelection)
    UNITY_DEFINE_INSTANCED_PROP(uint, _FixtureRotationX)
    UNITY_DEFINE_INSTANCED_PROP(uint, _FixtureBaseRotationY)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Emission)
    UNITY_DEFINE_INSTANCED_PROP(float4, _EmissionDMX)
    UNITY_DEFINE_INSTANCED_PROP(float, _ConeWidth)  
    UNITY_DEFINE_INSTANCED_PROP(float, _GlobalIntensity)
    UNITY_DEFINE_INSTANCED_PROP(float, _GlobalIntensityBlend)
    UNITY_DEFINE_INSTANCED_PROP(float, _FinalIntensity)
    UNITY_DEFINE_INSTANCED_PROP(float, _ConeLength)
    UNITY_DEFINE_INSTANCED_PROP(float, _MaxConeLength)
    UNITY_DEFINE_INSTANCED_PROP(float, _MaxMinPanAngle)
    UNITY_DEFINE_INSTANCED_PROP(float, _MaxMinTiltAngle)
#ifdef _VRSL_AUDIOLINK_ON
        UNITY_DEFINE_INSTANCED_PROP(float, _EnableAudioLink)
        UNITY_DEFINE_INSTANCED_PROP(float, _EnableColorChord)
        UNITY_DEFINE_INSTANCED_PROP(float, _NumBands)
        UNITY_DEFINE_INSTANCED_PROP(float, _Band)
        UNITY_DEFINE_INSTANCED_PROP(float, _BandMultiplier)
        UNITY_DEFINE_INSTANCED_PROP(float, _Delay)
        UNITY_DEFINE_INSTANCED_PROP(uint, _EnableColorTextureSample)
        UNITY_DEFINE_INSTANCED_PROP(float, _TextureColorSampleX)
        UNITY_DEFINE_INSTANCED_PROP(float, _TextureColorSampleY)
        UNITY_DEFINE_INSTANCED_PROP(float, _ThemeColorTarget)
        UNITY_DEFINE_INSTANCED_PROP(uint, _EnableThemeColorSampling)    
#endif
UNITY_INSTANCING_BUFFER_END(Props)

#ifdef _VRSL_LEGACY_TEXTURES
    Texture2D _OSCGridRenderTexture, _OSCGridRenderTextureRAW, _OSCGridStrobeTimer, _OSCGridSpinTimer;
    uniform float4 _OSCGridRenderTextureRAW_TexelSize, _OSCGridSpinTimer_TexelSize, _OSCGridRenderTexture_TexelSize;
    SamplerState VRSL_PointClampSampler;
#else
    Texture2D _Udon_DMXGridRenderTexture;
    uniform float4 _Udon_DMXGridRenderTexture_TexelSize;
    Texture2D _Udon_DMXGridStrobeOutput, _Udon_DMXGridSpinTimer, _Udon_DMXGridRenderTextureMovement;
    uniform float4 _Udon_DMXGridStrobeOutput_TexelSize, _Udon_DMXGridSpinTimer_TexelSize, _Udon_DMXGridRenderTextureMovement_TexelSize;
    SamplerState VRSL_PointClampSampler;
#endif

//half _MaxMinTiltAngle, _MaxMinPanAngle;

float VRSL_invLerp(float from, float to, float value)
{
  return (value - from) / (to - from);
}

float VRSL_remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
{
    float rel = VRSL_invLerp(origFrom, origTo, value);
    return lerp(targetFrom, targetTo, rel);
}

float4 getBaseEmission()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _Emission);
}
float4 getAltBaseEmission()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionDMX);
}

float getGlobalIntensity()
{
    return lerp(1.0,UNITY_ACCESS_INSTANCED_PROP(Props, _GlobalIntensity), UNITY_ACCESS_INSTANCED_PROP(Props, _GlobalIntensityBlend));
}

float getFinalIntensity()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _FinalIntensity);
}

float getMaxMinPanAngle()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _MaxMinPanAngle);
}
float getMaxMinTiltAngle()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _MaxMinTiltAngle);
}

uint isStrobe()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_EnableStrobe);
}

uint isDMX()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_EnableDMX);
}

uint GetDMXChannel()
{
    return round(UNITY_ACCESS_INSTANCED_PROP(Props, _DMXChannel));  
}

int ConvertToRawDMXChannel(int chan, int universe)
{
    return abs(chan + (universe * 512) - 1);
}

uint GetPanInvert()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _PanInvert);
}
uint GetTiltInvert()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _TiltInvert);
}
float GetOffsetX()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_FixtureRotationX);
}

float GetOffsetY()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props,_FixtureBaseRotationY);
}

half ReadDMX(uint DMXChannel, Texture2D _DecodedTexture)
{
    uint channelIndex = DMXChannel;// - 1;

    if (channelIndex >= 16384) // 128*128 = 16384
        return 0.0h;

    uint x = channelIndex % 128;
    uint y = channelIndex / 128;

    float u = (x + 0.5) / 128.0; // center of pixel
    float v = (y + 0.5) / 128.0;
    
    //float4 decoded = tex2Dlod(_DecodedTexture, float4(u, v, 0, 0));
    float4 decoded = _DecodedTexture.SampleLevel(VRSL_PointClampSampler, float4(u, v, 0, 0), 0);
    return decoded.r; // r is either fallback or new dmx data
}

float ReadDMXRaw(uint DMXChannel, Texture2D _DecodedTexture)
{
    return ReadDMX(DMXChannel, _DecodedTexture);
}

float GetStrobeOutput(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float phase = ReadDMXRaw(DMXChannel, _OSCGridStrobeTimer);
        float status = ReadDMX(DMXChannel, _OSCGridRenderTextureRAW);
        half strobe = (sin(phase));//Get sin wave
        strobe = IF(strobe > 0.0, 1.0, 0.0);//turn to square wave
        //strobe = saturate(strobe);

        strobe = IF(status > 0.2, strobe, 1); //minimum channel threshold set
        
        //check if we should even be strobing at all.
        strobe = IF(isDMX() == 1, strobe, 1);
        strobe = IF(isStrobe() == 1, strobe, 1);
        
        return strobe;
        
    #else
        //float phase = ReadDMXRaw(DMXChannel, _Udon_DMXGridStrobeTimer);
        half strobe = ReadDMX(DMXChannel, _Udon_DMXGridStrobeOutput);

        //check if we should even be strobing at all.
        strobe = IF(isDMX() == 1, strobe, 1);
        strobe = IF(isStrobe() == 1, strobe, 1);
        
        return strobe;
    #endif
}

float GetImmediateStrobeOutput(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float phase = ReadDMXRaw(DMXChannel, _OSCGridStrobeTimer);
        float status = ReadDMX(DMXChannel, _OSCGridRenderTextureRAW);
        half strobe = sin(phase);//Get sin wave
        strobe = IF(strobe > 0.0, 1.0, 0.0);//turn to square wave
        //strobe = saturate(strobe);

        strobe = IF(status > 0.2, strobe, 1); //minimum channel threshold set
        return strobe;
    #else
        return ReadDMX(DMXChannel, _Udon_DMXGridStrobeOutput);
    #endif
}

//Function for getting the RGB Color Value (Channels 4, 5, and 6)
float4 GetDMXColor(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float r = ReadDMX(DMXChannel, _OSCGridRenderTextureRAW);
        float g = ReadDMX(DMXChannel + 1, _OSCGridRenderTextureRAW);
        float b = ReadDMX(DMXChannel + 2, _OSCGridRenderTextureRAW);
    #else
        float r = ReadDMX(DMXChannel, _Udon_DMXGridRenderTexture);
        float g = ReadDMX(DMXChannel + 1, _Udon_DMXGridRenderTexture);
        float b = ReadDMX(DMXChannel + 2, _Udon_DMXGridRenderTexture);
    #endif
    
    #if defined(PROJECTION_YES)
        r *= _RedMultiplier;
        g *= _GreenMultiplier;
        b *= _BlueMultiplier;
    #endif
    
    //return IF(isOSC() == 1,lerp(fixed4(0,0,0,1), float4(r,g,b,1), GetOSCIntensity(DMXChannel, _FixtureMaxIntensity)), float4(r,g,b,1) * GetOSCIntensity(DMXChannel, _FixtureMaxIntensity));
    return float4(r, g, b, 1);
}

float4 calculateRotations(float4 vertexInput, float4 vertexColor, int normalsCheck, float pan, float tilt, float4 rotationOrigin)
{
    //	vertexInput = IF(worldspacecheck == 1, float4(UnityObjectToWorldNormal(v.normal).x * -1.0, UnityObjectToWorldNormal(v.normal).y * -1.0, UnityObjectToWorldNormal(v.normal).z * -1.0, 1), vertexInput)
    #if defined(_VRSLPAN_ON)
        //CALCULATE BASE ROTATION. MORE FUN MATH. THIS IS FOR PAN.
        float angleY = radians(GetOffsetY() + pan);
        float c, s;
        sincos(angleY, s, c);

        float3x3 rotateYMatrix = float3x3(c, -s, 0,
                                        s, c, 0,
                                        0, 0, 1);
        float3 BaseAndFixturePos = vertexInput.xyz;

        //INVERSION CHECK
        rotateYMatrix = GetPanInvert() == 1 ? transpose(rotateYMatrix) : rotateYMatrix;

        float3 localRotY = mul(rotateYMatrix, BaseAndFixturePos);
        //LOCALROTY IS NEW ROTATION
    
    #endif

    #if defined(_VRSLTILT_ON)
        //CALCULATE FIXTURE ROTATION. WOO FUN MATH. THIS IS FOR TILT.

        //set new origin to do transform
        float3 newOrigin = vertexInput.w * rotationOrigin.xyz;
        //if vertexInput.w is 1 (vertex), origin changes
        //if vertexInput.w is 0 (normal/tangent), origin doesn't change

        //subtract new origin from original origin for blue vertexes
        vertexInput.xyz = vertexColor.b == 1.0 ? vertexInput.xyz - newOrigin : vertexInput.xyz;
        //DO ROTATION

        //#if defined(PROJECTION_YES)
        //buffer[3] = GetTiltValue(sector);
        //#endif
        float angleX = radians(GetOffsetX() + tilt);
        float cT, sT;
        sincos(angleX, sT, cT);
        float3x3 rotateXMatrix = float3x3(1, 0, 0,
                                        0, cT, -sT,
                                        0, sT, cT);
            
        //float4 fixtureVertexPos = vertexInput;
            
        //INVERSION CHECK
        rotateXMatrix = GetTiltInvert() == 1 ? transpose(rotateXMatrix) : rotateXMatrix;

        //float4 localRotX = mul(rotateXMatrix, fixtureVertexPos);
        //LOCALROTX IS NEW ROTATION
        //COMBINED ROTATION FOR FIXTURE
        #if defined(_VRSLPAN_ON)
            float3x3 rotateXYMatrix = mul(rotateYMatrix, rotateXMatrix);
            float3 localRotXY = mul(rotateXYMatrix, vertexInput.xyz);
        #else
            float3 localRotX = mul(rotateXMatrix, vertexInput.xyz);
        #endif
    #endif
	//LOCALROTXY IS COMBINED ROTATION
	//Apply fixture rotation ONLY to those with blue vertex colors
    #if defined(_VRSLTILT_ON)
	//apply LocalRotXY rotation then add back old origin
        #if defined(_VRSLPAN_ON)
	        vertexInput.xyz = vertexColor.b == 1.0 ? localRotXY + newOrigin : vertexInput.xyz;
        #else
            vertexInput.xyz = vertexColor.b == 1.0 ? localRotX + newOrigin : vertexInput.xyz;
        #endif
    #endif
	//vertexInput.xyz = v.color.b == 1.0 ? vertexInput.xyz + newOrigin : vertexInput.xyz;
	
	//appy LocalRotY rotation to lightfixture base;
    #if defined(_VRSLPAN_ON)
	    vertexInput.xyz = vertexColor.g == 1.0 ? localRotY : vertexInput.xyz;
    #endif
	return vertexInput;
}

// Function for getting value from coarse and fine
float getCoarseFine(float coarse, half fine)
{
    return coarse + (fine / 255.0);
}

float GetPanValue(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float coarse = ReadDMX(DMXChannel + 0, _OSCGridRenderTexture);
        float fine   = ReadDMX(DMXChannel + 1, _OSCGridRenderTexture);
    #else
        float coarse = ReadDMX(DMXChannel + 0, _Udon_DMXGridRenderTextureMovement);
        float fine   = ReadDMX(DMXChannel + 1, _Udon_DMXGridRenderTextureMovement);
    #endif
    
    float inputValue = getCoarseFine(coarse, fine);
    
    return ((getMaxMinPanAngle() * 2) * inputValue) - getMaxMinPanAngle();
}

float GetTiltValue(uint DMXChannel)
{
    #ifdef _VRSL_LEGACY_TEXTURES
        float coarse = ReadDMX(DMXChannel + 2, _OSCGridRenderTexture);
        float fine   = ReadDMX(DMXChannel + 3, _OSCGridRenderTexture);
    #else
        float coarse = ReadDMX(DMXChannel + 2, _Udon_DMXGridRenderTextureMovement);
        float fine   = ReadDMX(DMXChannel + 3, _Udon_DMXGridRenderTextureMovement);
    #endif
    
    float inputValue = getCoarseFine(coarse, fine);
    
    return ((getMaxMinTiltAngle() * 2) * inputValue) - getMaxMinTiltAngle(); 
}
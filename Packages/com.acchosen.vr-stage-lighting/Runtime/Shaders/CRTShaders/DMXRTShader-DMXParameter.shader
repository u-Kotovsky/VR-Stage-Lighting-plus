Shader "VRSL/DMX CRTs/DMXParameters"
{
    Properties
    {
        [Toggle] _EnableDMX ("Enable DMX", Int) = 0
        [NoScaleOffset]_DMXTexture("DMX Grid Render Texture", 2D) = "white" {}
        [NoScaleOffset]_ParameterMap ("Parameter Map", 2D) = "black" {}
        [Toggle(USE_ULTRA)] _UseUltra ("Use Ultra", Integer) = 0
        [Toggle(USE_UBER)] _UseUber ("Use Uber", Integer) = 0
    }

    SubShader
    {
        Lighting Off
        Blend One Zero
        Name "Merge Pass"
        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #include "UnityCG.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0
            #pragma shader_feature USE_ULTRA USE_UBER

            #define VRSL_DMX

            //#define IF(a, b, c) lerp(b, c, step((fixed) (a), 0));
            sampler2D _ParameterMap;
            sampler2D _DMXTexture;

            #define OUTPUT_W 128
            #define OUTPUT_H 128
            #define MAX_CHANNELS (OUTPUT_W * OUTPUT_H)
            
            uint2 getCoords(uint offset, uint2 start)
            {
                uint2 coords = uint2(
                    start.x + offset,
                    start.y);
                
                // TODO: redo in branchless way
                if (coords.x >= OUTPUT_W) // if we are on the border, move up to next channel
                {
                    coords.x = 0;
                    coords.y += 1;
                }
                
                /*if (coords.y > OUTPUT_H)
                {
                    // do we really need to limit it?
                }*/
                
                return coords;
            }
            
            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                half4 map = tex2D(_ParameterMap, IN.localTexcoord.xy);
                
                float2 inUV = IN.localTexcoord;
                uint2 texel = uint2(
                    (uint)inUV.x * OUTPUT_W,
                    (uint)inUV.y * OUTPUT_H);
                
                // Texels
                uint2 fine_texel = getCoords(1, texel);
                uint2 ultra_texel = getCoords(2, texel);
                uint2 uber_texel = getCoords(3, texel);
                
                // bottom left corner: black, x: 0; y: 0
                // bottom right corner: red, x: 1, y: 0
                // top left corner: green, x: 0, y: 1
                // top right corner: yellow, x: 1, y: 1
                // return float4(inUV.x, inUV.y, 0, 1);
                
                // UVs
                float2 fine_uv = float2((float)fine_texel.x/OUTPUT_W, (float)fine_texel.y/OUTPUT_H);
                float2 ultra_uv = float2((float)ultra_texel.x/OUTPUT_W, (float)ultra_texel.y/OUTPUT_H);
                float2 uber_uv = float2((float)uber_texel.x/OUTPUT_W, (float)uber_texel.y/OUTPUT_H);
                
                // DMX Raw values
                half4 coarse = tex2D(_DMXTexture, inUV.xy);
                half4 fine = tex2D(_DMXTexture, fine_uv);//IN.localTexcoord.xy);
                #if USE_ULTRA
                half4 ultra = tex2D(_DMXTexture, ultra_uv);
                #endif
                #if USE_UBER
                half4 uber = tex2D(_DMXTexture, uber_uv);
                #endif
                
                // Masking by map
                coarse = lerp(coarse, 0, step(map.r, 0.5));
                fine = lerp(fine, 0, step(map.g, 0.5));
                #if USE_ULTRA
                ultra = lerp(ultra, 0, step(map.b, 0.5));
                #endif
                #if USE_UBER
                uber = lerp(uber, 0, step(map.a, 0.5));
                #endif
                
                // Calc channels
                float value = coarse;
                value += fine / 256.0;
                // I don't think this is a right way to calculate these values... poke me 
                #if USE_ULTRA
                value += ultra / 256.0 * 256.0;
                #endif
                #if USE_UBER
                value += uber / 256.0 * 256.0 * 256.0;
                #endif
            
                return value;
            }
            ENDCG
         }
    }
    //CustomEditor "VRSLInspector"
}
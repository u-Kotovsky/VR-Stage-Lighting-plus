Shader "VRSL/DMX CRTs/DMXParameters"
{
    Properties
    {
        [Toggle] _EnableDMX ("Enable Stream DMX/DMX Control", Int) = 0
        [NoScaleOffset]_DMXTexture("DMX Grid Render Texture (To Control Lights)", 2D) = "white" {}
        
        _ParameterMap ("Parameter Map", 2D) = "black" {}
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

            #define VRSL_DMX

            sampler2D _DMXTexture;
            #define IF(a, b, c) lerp(b, c, step((fixed) (a), 0));
            
            sampler2D _ParameterMap;

            #define OUTPUT_W 128
            #define OUTPUT_H 128
            #define MAX_CHANNELS (OUTPUT_W * OUTPUT_H)
            
            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                half4 map = tex2D(_ParameterMap, IN.localTexcoord.xy);
                
                // check next few channels, tho how do we do that?
                
                float2 inUV = IN.localTexcoord;
                uint x = (uint)(inUV.x * OUTPUT_W);
                uint y = (uint)(inUV.y * OUTPUT_H);
                
                // check fine channel
                uint xf = x+1;
                uint yf = y;
                if (xf >= OUTPUT_W) // we are on the border, move up to next channel
                {
                    xf = 0;
                    yf += 1;
                }
                
                // bottom left corner: black, x: 0; y: 0
                // bottom right corner: red, x: 1, y: 0
                // top left corner: green, x: 0, y: 1
                // top right corner: yellow, x: 1, y: 1
                //return float4(inUV.x, inUV.y, 0, 1);
                
                float2 fineUV = float2((float)xf/OUTPUT_W, (float)yf/OUTPUT_H);
                //return float4(fineUV,0,1);
                
                half4 coarse = tex2D(_DMXTexture, inUV.xy);
                half4 fine = tex2D(_DMXTexture, fineUV);//IN.localTexcoord.xy);
                
                //half4 ultra = tex2D(_DMXTexture, IN.localTexcoord.xy);
                //half4 uber = tex2D(_DMXTexture, IN.localTexcoord.xy);
                
                coarse = lerp(coarse, 0, step(map.r, 0.5));
                fine = lerp(fine, 0, step(map.r, 0.5));
                //ultra = lerp(ultra, 0, step(map.r, 0.5));
                //uber = lerp(uber, 0, step(map.r, 0.5));
                
                float value = coarse
                    + fine  / 256.0;
                    //+ ultra / (256.0 * 256.0)
                    //+ uber  / (256.0 * 256.0 * 256.0);
            
                return value;
            }
            ENDCG
         }
    }
    //CustomEditor "VRSLInspector"
}

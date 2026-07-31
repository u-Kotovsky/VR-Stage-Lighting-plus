Shader "Custom/BinaryStageFlightDeserializer"
{
    Properties
    {
        _InputTex ("Encoded Input Texture", 2D) = "black" {}
        _PreviousFrame ("Previous Frame (for fallback)", 2D) = "black" {}
    }

    SubShader
    {
        Lighting Off
        Blend One Zero
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }
        Cull Off ZWrite Off ZTest Always

        Name "DecodePass"
        Pass
        {
            HLSLPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            //#pragma target 3.0

            #include "UnityCustomRenderTexture.cginc"
            #include "UnityCG.cginc"

            sampler2D _InputTex;
            sampler2D _PreviousFrame;
            float4 _InputTex_TexelSize;

            #define OUTPUT_W 128
            #define OUTPUT_H 128
            #define MAX_CHANNELS (OUTPUT_W * OUTPUT_H)

            #define COLUMN_WIDTH_PX 4
            #define BIT_HEIGHT_PX 4
            #define BYTES_PER_COLUMN 6
            #define BITS_PER_BYTE 8
            #define PARITY_BITS 4
            #define DATA_END_LINE (BYTES_PER_COLUMN * BITS_PER_BYTE)

            // --- Bit Sampling ---
            uint SampleBit(float2 uv)
            {
                float4 c = tex2Dlod(_InputTex, float4(uv, 0, 0));
                half lum = dot(c.rgb, float3(0.2126, 0.7152, 0.0722));
                return lum > 0.5h ? 1u : 0u;
            }
            
            // CRC-4/ITU: x^4 + x + 1 -> reversed feedback 0xC
            uint ComputeCRC4(uint data[6])
            {
                uint crc = 0;
                [unroll]
                for (int i = 0; i < 6; ++i)
                {
                    uint b = data[i];
                    [unroll]
                    for (int j = 0; j < 8; ++j)
                    {
                        uint input_bit = ((b >> j) ^ crc) & 1; // LSB-first
                        crc >>= 1;
                        if (input_bit) crc ^= 0xC;
                    }
                }
                return crc & 0xF;
            }

            half4 frag(v2f_customrendertexture i) : SV_Target
            {
                float2 outUV = i.globalTexcoord;

                uint x = (uint)(outUV.x * OUTPUT_W);
                uint y = (uint)(outUV.y * OUTPUT_H);
                //x = min(x, OUTPUT_W - 1);
                //y = min(y, OUTPUT_H - 1);
                uint channelIndex = x + y * OUTPUT_W;

                if (channelIndex >= MAX_CHANNELS)
                    return 0;

                uint columnIndex = channelIndex / BYTES_PER_COLUMN;
                uint byteInColumn = channelIndex % BYTES_PER_COLUMN;

                if (columnIndex >= 480) // 1920 / 4
                    return 0;
                
                // === Decode all 6 bytes: LSB-first, top = bit 0 ===
                uint bytes[6] = {0,0,0,0,0,0};

                [unroll]
                for (uint b = 0; b < BYTES_PER_COLUMN; ++b)
                {
                    [unroll]
                    for (uint bit = 0; bit < BITS_PER_BYTE; ++bit)
                    {
                        // Bit line: 0 = top -> bit 0 (LSB), 7 = bottom -> bit 7 (MSB)
                        uint bitLine = b * BITS_PER_BYTE + bit;
                        uint pixelY = bitLine * BIT_HEIGHT_PX + 2; // Center of 4px-high block

                        // Sample U: center of 4px-wide column
                        float u = (columnIndex * COLUMN_WIDTH_PX + 2.0) * _InputTex_TexelSize.x;

                        // Sample V: map to top 208px, and FLIP Y if input texture has inverted origin
                        float v = (pixelY + 0.5) * (1.0 / 208.0);
                        v = 1.0 - v; // Flip Y — DirectX (top-left vs bottom-left)

                        // Sample and extract bit
                        uint bitVal = SampleBit(float2(u, v));

                        // Reconstruct byte: LSB-first → shift by 'bit' index
                        //bytes[b] |= (bitVal << bit); // bit 0 -> shift 0, bit 7 -> shift 7
                        bytes[b] |= (bitVal << (7 - bit)); // MSB-first
                    }
                }

                /* === CRC VALIDATION ===
                // === Read 4-bit CRC from lines 48–51 ===
                uint receivedCRC = 0;
                [unroll]
                for (uint i = 0; i < PARITY_BITS; ++i)
                {
                    uint bitLine = DATA_END_LINE + i;
                    uint pixelY = bitLine * BIT_HEIGHT_PX + 2;
                    float u = (columnIndex * COLUMN_WIDTH_PX + 2.0) * (1.0 / 1920.0);
                    float v = 1.0 - ((pixelY + 0.5) / 208.0);

                    uint bit = SampleBit(float2(u, v));
                    receivedCRC |= (bit << (3 - i)); // MSB-first for CRC bits
                }

                uint computedCRC = ComputeCRC4(bytes) & 0xF;
                receivedCRC &= 0xF;

                //bool crcOK = true; // (computedCRC == receivedCRC);

                // === Fallback logic ===
                //float4 prev = tex2Dlod(_PreviousFrame, float4(outUV, 0, 0));
                //half fallbackValue = prev.b;*/

                half finalValue = //crcOK ?
                    (half)bytes[byteInColumn] / 255.0h;// : fallbackValue;
                half crcFail = 0.0h; // Always 0 — no CRC

                return half4(crcFail, 0, finalValue, 1.0); // R=error (always 0), B=value
            }
            ENDHLSL
        }
    }
}
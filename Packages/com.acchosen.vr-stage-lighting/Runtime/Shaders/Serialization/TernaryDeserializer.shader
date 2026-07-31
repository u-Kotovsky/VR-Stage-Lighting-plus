Shader "Custom/TernaryDeserializer"
{
    Properties
    {
        _InputTex ("Encoded Input", 2D) = "black" {}
        _PreviousFrame ("Previous Frame", 2D) = "black" {}
    }

    SubShader
    {
        Lighting Off
        Blend One Zero
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCustomRenderTexture.cginc"
            #include "UnityCG.cginc"

            sampler2D _InputTex;
            sampler2D _PreviousFrame;

            // Output size
            #define OUTPUT_W 128
            #define OUTPUT_H 128
            #define MAX_CHANNELS (OUTPUT_W * OUTPUT_H)

            // Encoding settings
            #define BLOCK_SIZE_PX 4          // Each block is 4x4 pixels
            #define BITS_PER_BLOCK 2       // 2 bits per block
            #define BYTES_PER_COLUMN 6     // 6 bytes per column
            #define BLOCKS_PER_BYTE 4     // 4 blocks per byte
            #define COLUMN_WIDTH_PX BLOCK_SIZE_PX  // Width of column = block width
            #define COLUMN_HEIGHT_PX (BYTES_PER_COLUMN * BLOCKS_PER_BYTE * BLOCK_SIZE_PX) // 96px high
            #define TOTAL_COLUMNS (1920 / COLUMN_WIDTH_PX) // 480 columns

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 globalTexCoord : TEXCOORD0;
            };

            // Sample ternary state: returns 0, 1, or 3
            uint SampleTernary(float2 uv)
            {
                float4 c = tex2Dlod(_InputTex, float4(uv, 0, 0));
                half lum = dot(c.rgb, float3(0.2126, 0.7152, 0.0722));

                if (lum < 0.33h) return 0;   // Black
                if (lum < 0.66h) return 1;   // Gray
                return 3;                   // White (bright)
            }

            half4 frag(VertexOutput i) : SV_Target
            {
                float2 outUV = i.globalTexCoord;
                uint x = (uint)(outUV.x * OUTPUT_W);
                uint y = (uint)(outUV.y * OUTPUT_H);
                x = min(x, OUTPUT_W - 1);
                y = min(y, OUTPUT_H - 1);
                uint channelIndex = x + y * OUTPUT_W;

                if (channelIndex >= MAX_CHANNELS)
                    return 0;

                uint columnIndex = channelIndex / BYTES_PER_COLUMN;
                uint byteInColumn = channelIndex % BYTES_PER_COLUMN;

                if (columnIndex >= TOTAL_COLUMNS)
                    return 0;

                // === Decode all 6 bytes: 2 bits per block × 4 blocks = 8 bits per byte ===
                uint bytes[6] = {0, 0, 0, 0, 0, 0};

                [unroll]
                for (uint b = 0; b < BYTES_PER_COLUMN; ++b)
                {
                    uint blockStartY = (b * BLOCKS_PER_BYTE); // Starting block index
                    uint byteValue = 0;

                    [unroll]
                    for (uint block = 0; block < BLOCKS_PER_BYTE; ++block)
                    {
                        uint blockIndex = blockStartY + block;
                        uint pixelY = (blockIndex * BLOCK_SIZE_PX) + (BLOCK_SIZE_PX / 2); // Center Y

                        float u = (columnIndex * COLUMN_WIDTH_PX + (BLOCK_SIZE_PX / 2)) * (1.0 / 1920.0);
                        float v = 1.0 - ((pixelY + 0.5) / 1080.0); // Adjust if needed

                        uint ternaryVal = SampleTernary(float2(u, v)); // 0, 1, or 3

                        // Extract two bits
                        uint bit0 = (ternaryVal >> 0) & 1;
                        uint bit1 = (ternaryVal >> 1) & 1;

                        // Place into byte (MSB first: block0 = bits 7-6, ..., block3 = bits 1-0)
                        byteValue |= (bit1 << (7 - block*2));
                        byteValue |= (bit0 << (6 - block*2));
                    }

                    bytes[b] = byteValue;
                }

                // For now: assume valid
                bool decodeOK = true;

                // Get fallback value
                float4 prev = tex2Dlod(_PreviousFrame, float4(outUV, 0, 0));
                half fallbackValue = prev.b;

                half finalValue = decodeOK ? (half)bytes[byteInColumn] / 255.0h : fallbackValue;
                half errorFlag = decodeOK ? 0.0h : 1.0h;

                return half4(errorFlag, 0, finalValue, 1.0); // R=error, B=value
            }
            ENDHLSL
        }
    }
}
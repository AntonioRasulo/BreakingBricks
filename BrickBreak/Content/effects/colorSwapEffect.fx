#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Vertex shader parameters
float4x4 MatrixTransform;
float2 ScreenSize;
float SpinAmount;

// Pixel shader parameters
Texture2D SpriteTexture;

// A value between 0 and 1 that controls the intensity of the grayscale effect.
// 0 = full color, 1 = full grayscale.
float Saturation = 1.0;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

// the custom color map passed to the Material.SetParameter()
Texture2D ColorMap;
sampler2D ColorMapSampler = sampler_state
{
    Texture = <ColorMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

// a control variable to lerp between original color and swapped color  
float OriginalAmount;

struct VertexShaderInput
{
    float4 Position	: POSITION0;
    float4 Color	: COLOR0;
    float2 TexCoord	: TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 Grayscale(float4 color)
{
    // Calculate the grayscale value based on human perception of colors
    float grayscale = dot(color.rgb, float3(0.3, 0.59, 0.11));

    // create a grayscale color vector (same value for R, G, and B)
    float3 grayscaleColor = float3(grayscale, grayscale, grayscale);

    // Linear interpolation between the grayscale color and the original color's
    // rgb values based on the saturation parameter.
    float3 finalColor = lerp(grayscale, color.rgb, Saturation);

    // Return the final color with the original alpha value
    return float4(finalColor, color.a);
}

float4 SwapColors(float4 color)
{
    // produce the key location
    //  note the x-offset by half a texel solves rounding errors.
    float2 keyUv = float2(color.r , 0);
    
    // read the swap color value
    float4 swappedColor = tex2D(ColorMapSampler, keyUv) * color.a;
    
    // ignore the swap if the map does not have a value
    bool hasSwapColor = swappedColor.a > 0;
    if (!hasSwapColor)
    {
        return color;
    }
    
    // return the result color
    return lerp(swappedColor, color, OriginalAmount);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // read the original color value
    float4 originalColor = tex2D(SpriteTextureSampler,input.TextureCoordinates);

    float4 swapped = SwapColors(originalColor);
    float4 saturated = Grayscale(swapped);
    
    return saturated;
}

VertexShaderOutput MainVS(VertexShaderInput input) 
{
    VertexShaderOutput output;

    float4 pos = input.Position;

    // create the center of rotation
    float2 centerXZ = float2(ScreenSize.x * .5, 0);

    // convert the debug variable into an angle from 0 to 2 pi. 
    //  shaders use radians for angles, so 2 pi = 360 degrees
    float angle = SpinAmount * 6.28;
    
    // pre-compute the cos and sin of the angle
    float cosA = cos(angle);
    float sinA = sin(angle);
    
    // shift the position to the center of rotation
    pos.xz -= centerXZ;
    
    // compute the rotation
    float nextX = pos.x * cosA - pos.z * sinA;
    float nextZ = pos.x * sinA + pos.z * cosA;
    
    // apply the rotation
    pos.x = nextX;
    pos.z = nextZ;
    
    // shift the position away from the center of rotation
    pos.xz += centerXZ;
    
    output.Position = mul(pos, MatrixTransform);
    output.Color = input.Color;
    output.TextureCoordinates = input.TexCoord;
    return output;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
		VertexShader = compile VS_SHADERMODEL MainVS();
	}
};
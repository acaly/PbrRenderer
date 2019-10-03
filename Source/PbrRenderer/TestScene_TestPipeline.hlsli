
cbuffer Constants : register(b0)
{
	float4x4 mWorld;
	float4x4 mViewProj;
	float4 mViewPosition;
}

Texture2D faceTexture : register(t0);
TextureCube diffuseSkyMap : register(t1);
TextureCube specularSkyMap : register(t2);
SamplerState textureSampler : register(s0);

struct VS_INPUT
{
	float4 Pos : POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD;
};

struct PS_INPUT
{
	float4 Pos : SV_POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD;
	float3 ViewDir : POSITION;
};

static const float3 Fresnel_F0 = float3(0.955, 0.638, 0.538);

PS_INPUT VS(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;
	output.Pos = mul(input.Pos, mWorld);
	output.ViewDir = (output.Pos - mViewPosition).xyz;
	output.Pos = mul(output.Pos, mViewProj);
	output.Normal = mul(input.Normal, (float3x3)mWorld);
	output.TexCoord = input.TexCoord;
	//output.Color = input.Pos.z;
	return output;
}

float4 PS(PS_INPUT input) : SV_Target
{
	//For diffuse
	float3 diffuseColor = diffuseSkyMap.Sample(textureSampler, input.Normal).xyz * float3(1, 1, 1) / 3.14;

	//For reflection
	float3 l = -normalize(input.ViewDir);
	float nl = dot(input.Normal, l);
	float x_1_nl = 1 - nl;
	float x_1_nl_2 = x_1_nl * x_1_nl;
	float3 fresnel_factor = Fresnel_F0 + (float3(1, 1, 1) - Fresnel_F0) * x_1_nl_2 * x_1_nl_2 * x_1_nl;
	float3 reflectionDir = reflect(normalize(input.ViewDir), input.Normal);
	float3 specularColor = specularSkyMap.Sample(textureSampler, reflectionDir).xyz * fresnel_factor;

	float3 finalColor = specularColor * 5 + diffuseColor * 1;
	return float4(finalColor, 1);
}

//Reference image generation
//  output normal + view dir (before normalization) (both world space)

cbuffer Constants : register(b0)
{
	float4x4 mWorld;
	float4x4 mViewProj;
}

Texture2D faceTexture : register(t0);
TextureCube testSkyMap : register(t1);
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
	float Color : Color;
};

PS_INPUT VS(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;
	output.Pos = mul(input.Pos, mWorld);
	output.Pos = mul(output.Pos, mViewProj);
	output.Normal = mul(input.Normal, (float3x3)mWorld);
	output.TexCoord = input.TexCoord;
	//output.Color = input.Pos.z;
	return output;
}

float4 PS(PS_INPUT input) : SV_Target
{
	return testSkyMap.Sample(textureSampler, input.Normal.xyz) + float4(0.1, 0.1, 0.1, 0);
	//return faceTexture.Sample(textureSampler, input.TexCoord.xy);

	//
	//float4 pos = input.Pos;
	//if (input.TexCoord.x > 0.9 || input.TexCoord.x < 0.1 ||
	//	input.TexCoord.y > 0.9 || input.TexCoord.y < 0.1)
	//{
	//	return float4(0.5, 0.5, 0.5, 1);
	//}
	//return float4(1, 1, 1, 1);
}

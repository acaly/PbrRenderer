
cbuffer Constants : register(b0)
{
	float4x4 mWorld;
	float4x4 mViewProj;
	float4 mViewPosition;
}

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
	float3 ViewDir : POSITION;
};

struct PS_OUTPUT
{
	float4 Normal : SV_TARGET0;
	float4 ViewDir : SV_TARGET1;
};

PS_INPUT VS(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;
	output.Pos = mul(input.Pos, mWorld);
	output.ViewDir = (output.Pos - mViewPosition).xyz;
	output.Pos = mul(output.Pos, mViewProj);
	output.Normal = mul(input.Normal, (float3x3)mWorld);
	return output;
}

PS_OUTPUT PS(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;
	output.Normal = float4(input.Normal, 0);
	output.ViewDir = float4(input.ViewDir, 1);
	return output;
}

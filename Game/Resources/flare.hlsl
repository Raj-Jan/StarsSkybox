Texture2D _texture;
sampler _sampler;

cbuffer vs
{
	float3 sunPos;
	float2 fov;
};

struct VertexIn
{
	float3 pos : POSITION0;
	float3 nor : NORMAL0;
	float2 tex : TEXCOORD0;

	float4 lum : INSTANCE0;
};

struct Vertex
{
	float4 pos : SV_Position;
	float2 tex : TEXCOORD0;
	float lum : TEXCOORD1;
};

void MainVS(in VertexIn input, out Vertex output)
{
	float2 size = fov.x / fov;

	output.pos.xy = sunPos.xy * input.lum.y;
	output.pos.xy += input.lum.z * input.pos.xy * size;
	output.pos.z = 0;
	output.pos.w = sign(sunPos.z);
	output.tex = input.tex + float2(input.lum.w, 0);
	output.lum = input.lum.x;
}

void MainPS(in Vertex input, out float4 color : SV_Target)
{
	color = input.lum * _texture.Sample(_sampler, input.tex);
}
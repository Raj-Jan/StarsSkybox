static float PI = 1.5707963267948966192313216916398;
static float step = 12;
static float off = 0.2;

Texture2D _texture;
sampler _sampler;

cbuffer vs
{
	float3 position;
	float2 fov;
	float scale;
};

struct VertexIn
{
	float3 pos : POSITION0;
	float3 nor : NORMAL0;
	float2 tex : TEXCOORD0;
};

struct Vertex
{
	float4 pos : SV_Position;
	float2 tex : TEXCOORD0;
	float lum : TEXCOORD1;
};

void MainVS(in VertexIn input, out Vertex output)
{
	float4 pos = float4(position, 0);
	float2 size = scale * sqrt(fov.x) / fov;
	float x = atan2(pos.x, pos.z);
	float y = PI - acos(pos.y);
	float mix = clamp(step * (1 - abs(x) / fov.x + off) * (1 - abs(y) / fov.y + off), 0, 1);

	output.pos.xy = 1 / fov * float2(x, y) + size * input.pos.xy;
	output.pos.z = 0;
	output.pos.w = 1;
	output.tex = input.tex;
	output.lum = mix;
}

void MainPS(in Vertex input, out float4 color : SV_Target)
{
	color = input.lum * _texture.Sample(_sampler, input.tex);

	if (color.r == 0) discard;

	color.a = 1 - clamp(sqrt(input.tex.x * input.tex.x + input.tex.y * input.tex.y), 0, 1);
}
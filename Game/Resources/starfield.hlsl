static float PI = 1.5707963267948966192313216916398;
static float rangeInv = 600;
static float min = 0.0035;
static float fade = 10;

Texture2D _texture;
sampler _sampler;

cbuffer vs
{
	float4x4 view;
	float2 fov;
};

struct VertexIn
{
	float3 pos : POSITION0;
	float3 nor : NORMAL0;
	float2 tex : TEXCOORD0;

	float4 position : INSTANCE0;
	float4 color : INSTANCE1;
};

struct Vertex
{
	float4 pos : SV_Position;
	float4 col : COLOR0;
	float2 tex : TEXCOORD0;
};

void MainVS(in VertexIn input, out Vertex output)
{
	float4 pos = mul(view, input.position);
	float2 size = input.color.a * sqrt(fov.x) / fov;
	float mix = clamp(rangeInv * (size.x - min), 0, 1);
	float x = atan2(pos.x, pos.z);
	float y = PI - acos(pos.y);

	output.pos.xy = 1 / fov * float2(x, y) + size * input.pos.xy;
	output.pos.z = 0;
	output.pos.w = 1;
	output.col = mix * mix * input.color;
	output.tex = input.tex;
}

void MainPS(in Vertex input, out float4 color : SV_Target)
{
	if (input.col.a == 0) discard;

	color = input.col * _texture.Sample(_sampler, input.tex);
	color.a = 1;
}
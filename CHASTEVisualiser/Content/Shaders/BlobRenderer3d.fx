
float4 colour;
matrix worldViewProj;

struct VS_OUTPUT
{
	float4 position : SV_POSITION;
	float4 rawPos : TEXCOORD0;
};

VS_OUTPUT VShader(float4 position : POSITION)
{
	VS_OUTPUT Output;
	Output.position = mul(position, worldViewProj);
    return Output;
}

struct PsOut
{
	float4 colour : SV_Target;
};

PsOut PShader(VS_OUTPUT input)
{	
	PsOut outStruct;
	
	outStruct.colour = colour;
	
    return outStruct;
}

technique11 Blob
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_5_0, VShader() ) );
		SetPixelShader( CompileShader( ps_5_0, PShader() ) );
	}
}
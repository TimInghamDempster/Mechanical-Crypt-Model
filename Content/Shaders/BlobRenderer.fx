
Texture2D quadImage : register( t0 );
SamplerState mainSampler : register( s0 );
float2 scale;
float2 cameraPos;
float2 entityPosition;
float4 colour;

struct VS_OUTPUT
{
	float4 position : SV_POSITION;
	float4 rawPos : TEXCOORD0;
};

VS_OUTPUT VShader(float4 position : POSITION)
{
	VS_OUTPUT Output;
	Output.position = position;
	Output.position.xy *= scale;
	Output.position.xy -= cameraPos;
	Output.position.xy += entityPosition;
	Output.position.z = 0.5f;
	Output.rawPos = Output.position;
    return Output;
}

struct PsOut
{
	float4 colour : SV_Target;
	float depth : SV_Depth;
};

PsOut PShader(VS_OUTPUT input)
{
	float2 deltaPosition = input.rawPos.xy - entityPosition.xy;
	float2 normalisedDelta = deltaPosition / scale;
	float4 outputColour = colour;
	float dist = dot(normalisedDelta, normalisedDelta);
	outputColour *= dist > 0.5f ? 0.5f : 1.0f;
	float unnormalisedDist = dot(deltaPosition, deltaPosition);
	unnormalisedDist = abs(unnormalisedDist);
	unnormalisedDist *= 100.0f;
	unnormalisedDist +=  0.11;
	float depth = dist > 1.0f ? 10.0f : unnormalisedDist;
	
	PsOut outStruct;
	
	outStruct.colour = outputColour;
	outStruct.depth = depth;
	
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
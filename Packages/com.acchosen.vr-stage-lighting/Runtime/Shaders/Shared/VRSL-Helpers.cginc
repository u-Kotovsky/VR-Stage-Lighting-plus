#ifdef VRSL_DMX
half4 calculateRotations(float4 color, half4 input, int normalsCheck, half pan, half tilt)
{
	//input = IF(worldspacecheck == 1, half4(UnityObjectToWorldNormal(v.normal).x * -1.0, UnityObjectToWorldNormal(v.normal).y * -1.0, UnityObjectToWorldNormal(v.normal).z * -1.0, 1), input)
	//CALCULATE BASE ROTATION. MORE FUN MATH. THIS IS FOR PAN.
	half angleY = radians(getOffsetY() + pan);
	half c, s;
	sincos(angleY, s, c);

	half3x3 rotateYMatrix = half3x3(c, -s, 0,
									s, c, 0,
									0, 0, 1);
	half3 BaseAndFixturePos = input.xyz;

	//INVERSION CHECK
	rotateYMatrix = checkPanInvertY() == 1 ? transpose(rotateYMatrix) : rotateYMatrix;

	half3 localRotY = mul(rotateYMatrix, BaseAndFixturePos);
	//LOCALROTY IS NEW ROTATION


	//CALCULATE FIXTURE ROTATION. WOO FUN MATH. THIS IS FOR TILT.

	//set new origin to do transform
	half3 newOrigin = input.w * _FixtureRotationOrigin.xyz;
	//if input.w is 1 (vertex), origin changes
	//if input.w is 0 (normal/tangent), origin doesn't change

	//subtract new origin from original origin for blue vertexes
	input.xyz = color.b == 1.0 ? input.xyz - newOrigin : input.xyz;


	//DO ROTATION


	//#if defined(PROJECTION_YES)
	//buffer[3] = GetTiltValue(sector);
	//#endif
	half angleX = radians(getOffsetX() + tilt);
	sincos(angleX, s, c);
	half3x3 rotateXMatrix = half3x3(1, 0, 0,
									0, c, -s,
									0, s, c);
		
	//half4 fixtureVertexPos = input;
		
	//INVERSION CHECK
	rotateXMatrix = checkTiltInvertZ() == 1 ? transpose(rotateXMatrix) : rotateXMatrix;

	//half4 localRotX = mul(rotateXMatrix, fixtureVertexPos);
	//LOCALROTX IS NEW ROTATION



	//COMBINED ROTATION FOR FIXTURE

	half3x3 rotateXYMatrix = mul(rotateYMatrix, rotateXMatrix);
	half3 localRotXY = mul(rotateXYMatrix, input.xyz);
	//LOCALROTXY IS COMBINED ROTATION

	//Apply fixture rotation ONLY to those with blue vertex colors

	//apply LocalRotXY rotation then add back old origin
	input.xyz = color.b == 1.0 ? localRotXY + newOrigin : input.xyz;
	//input.xyz = color.b == 1.0 ? input.xyz + newOrigin : input.xyz;
	
	//appy LocalRotY rotation to lightfixture base;
	input.xyz = color.g == 1.0 ? localRotY : input.xyz;

	return input;
}
#endif
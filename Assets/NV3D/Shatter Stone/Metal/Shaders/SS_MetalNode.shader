// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NV3D/SS_Metal/MetalNode"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		[Normal]_Normal("Normal", 2D) = "bump" {}
		_Masks("Masks", 2D) = "white" {}
		_MetalTInt("Metal TInt", Color) = (1,0.2216981,0.2216981,0)
		_MetalSmothness("MetalSmothness", Range( 0 , 1)) = 1
		_MetalStrength("MetalStrength", Range( 0 , 1)) = 1
		_RockTint("Rock Tint", Color) = (1,1,1,0)
		_RustTint("Rust Tint", Color) = (0.363341,0.6698113,0.4364884,0)
		[HDR]_EmissionColor("Emission Color", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float4 _MetalTInt;
		uniform sampler2D _Masks;
		uniform float4 _Masks_ST;
		uniform float4 _RockTint;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _RustTint;
		uniform float4 _EmissionColor;
		uniform float _MetalStrength;
		uniform float _MetalSmothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Masks = i.uv_texcoord * _Masks_ST.xy + _Masks_ST.zw;
			float4 tex2DNode3 = tex2D( _Masks, uv_Masks );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode2 = tex2D( _Albedo, uv_Albedo );
			float4 lerpResult17 = lerp( ( ( ( _MetalTInt * tex2DNode3.b ) + ( _RockTint * ( 1.0 - tex2DNode3.b ) ) ) * tex2DNode2 ) , ( tex2DNode2 * _RustTint ) , tex2DNode3.a);
			o.Albedo = lerpResult17.rgb;
			o.Emission = ( tex2DNode3.b * _EmissionColor ).rgb;
			o.Metallic = saturate( ( tex2DNode3.b * _MetalStrength ) );
			float lerpResult22 = lerp( tex2DNode3.g , saturate( ( tex2DNode3.g * _MetalSmothness ) ) , tex2DNode3.b);
			o.Smoothness = lerpResult22;
			o.Occlusion = tex2DNode3.r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-236.6017,-555.886;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-665.4811,-155.1165;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;-1;e667eadc9b6422d49b3263a4fdf5ff63;e667eadc9b6422d49b3263a4fdf5ff63;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-660.5041,327.5844;Inherit;True;Property;_Masks;Masks;2;0;Create;True;0;0;0;False;0;False;-1;828979d29e8c15242983f0c5aba05279;828979d29e8c15242983f0c5aba05279;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;506.0895,465.3885;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-40.82007,-128.7918;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;13;-405.9056,-317.6269;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-233.0069,-392.1622;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-53.36117,-469.5088;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-39.80025,-225.6636;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;17;166.6926,-153.341;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;6;-258.4126,-45.17417;Inherit;False;Property;_RustTint;Rust Tint;7;0;Create;True;0;0;0;False;0;False;0.363341,0.6698113,0.4364884,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;268.2887,549.1876;Inherit;False;Property;_EmissionColor;Emission Color;8;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-656.5688,94.40793;Inherit;True;Property;_Normal;Normal;1;1;[Normal];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;5;-694.0974,-401.8821;Inherit;False;Property;_RockTint;Rock Tint;6;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-712.9907,-599.2;Inherit;False;Property;_MetalTInt;Metal TInt;3;0;Create;True;0;0;0;False;0;False;1,0.2216981,0.2216981,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-347.0747,742.8623;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;22;20.04438,669.9744;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;25;-167.7634,747.665;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-644.858,848.8901;Inherit;False;Property;_MetalSmothness;MetalSmothness;4;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;331.7467,177.7115;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-70.10268,187.3175;Inherit;False;Property;_MetalStrength;MetalStrength;5;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;28;509.0162,157.8563;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1032.635,54.22679;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;NV3D/SS_Metal/MetalNode;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;12;0;4;0
WireConnection;12;1;3;3
WireConnection;8;0;3;3
WireConnection;8;1;7;0
WireConnection;18;0;2;0
WireConnection;18;1;6;0
WireConnection;13;0;3;3
WireConnection;14;0;5;0
WireConnection;14;1;13;0
WireConnection;15;0;12;0
WireConnection;15;1;14;0
WireConnection;16;0;15;0
WireConnection;16;1;2;0
WireConnection;17;0;16;0
WireConnection;17;1;18;0
WireConnection;17;2;3;4
WireConnection;24;0;3;2
WireConnection;24;1;23;0
WireConnection;22;0;3;2
WireConnection;22;1;25;0
WireConnection;22;2;3;3
WireConnection;25;0;24;0
WireConnection;26;0;3;3
WireConnection;26;1;27;0
WireConnection;28;0;26;0
WireConnection;0;0;17;0
WireConnection;0;1;1;0
WireConnection;0;2;8;0
WireConnection;0;3;28;0
WireConnection;0;4;22;0
WireConnection;0;5;3;1
ASEEND*/
//CHKSM=E646696B901DE4155DEB61C4EE2045849AD86D7E
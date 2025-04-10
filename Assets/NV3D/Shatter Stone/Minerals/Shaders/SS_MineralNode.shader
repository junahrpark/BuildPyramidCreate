// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NV3D/SS_Minerals/MineralNode"
{
	Properties
	{
		[SingleLineTexture]_Albedo("Albedo", 2D) = "white" {}
		[Normal][SingleLineTexture]_Normal("Normal", 2D) = "bump" {}
		[SingleLineTexture]_Masks("Masks", 2D) = "black" {}
		[SingleLineTexture]_Emissive("Emissive", 2D) = "black" {}
		[SingleLineTexture]_Depth_Noise("Depth_Noise", 2D) = "white" {}
		[HDR]_GlowTint("GlowTint", Color) = (1,1,1,0)
		_GlowStrength("GlowStrength", Float) = 5
		_MineralTintNoise("MineralTintNoise", Color) = (1,1,1,0)
		_MineralTintBase("MineralTintBase", Color) = (0,0,0,0)
		_RockTint("RockTint", Color) = (0.5660378,0.5660378,0.5660378,0)
		_AOStrength("AOStrength", Float) = 1
		_FresnelStrength("FresnelStrength", Range( 0 , 2)) = 1
		_NoiseSpeed("NoiseSpeed", Float) = 0.005
		_DepthOffset("DepthOffset", Float) = -0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float3 viewDir;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _Masks;
		uniform float4 _Masks_ST;
		uniform float _FresnelStrength;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _RockTint;
		uniform float4 _MineralTintBase;
		uniform sampler2D _Depth_Noise;
		uniform float _NoiseSpeed;
		uniform float _DepthOffset;
		uniform float4 _MineralTintNoise;
		uniform sampler2D _Emissive;
		uniform float4 _Emissive_ST;
		uniform float4 _GlowTint;
		uniform float _GlowStrength;
		uniform float _AOStrength;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV23 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode23 = ( 0.0 + 2.0 * pow( max( 1.0 - fresnelNdotV23 , 0.0001 ), 10.0 ) );
			float2 uv_Masks = i.uv_texcoord * _Masks_ST.xy + _Masks_ST.zw;
			float4 tex2DNode3 = tex2D( _Masks, uv_Masks );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode1 = tex2D( _Albedo, uv_Albedo );
			float2 temp_cast_0 = (_NoiseSpeed).xx;
			float2 uv_TexCoord52 = i.uv_texcoord + ( _DepthOffset * i.viewDir ).xy;
			float2 panner12 = ( 1.0 * _Time.y * temp_cast_0 + uv_TexCoord52);
			float4 tex2DNode9 = tex2D( _Depth_Noise, panner12 );
			float4 temp_output_29_0 = ( tex2DNode9 * 1.5 );
			float4 saferPower31 = abs( temp_output_29_0 );
			float4 temp_output_32_0 = saturate( temp_output_29_0 );
			float4 lerpResult38 = lerp( ( _MineralTintBase * ( 1.0 - pow( saferPower31 , 1.5 ) ) ) , ( temp_output_32_0 * _MineralTintNoise ) , temp_output_32_0);
			float4 lerpResult39 = lerp( ( tex2DNode1 * _RockTint ) , lerpResult38 , tex2DNode3.a);
			o.Albedo = ( ( ( fresnelNode23 * tex2DNode3.a ) * _FresnelStrength ) + lerpResult39 ).rgb;
			float2 uv_Emissive = i.uv_texcoord * _Emissive_ST.xy + _Emissive_ST.zw;
			float4 tex2DNode4 = tex2D( _Emissive, uv_Emissive );
			o.Emission = ( ( ( saturate( ( tex2DNode4 + tex2DNode9 ) ) * _GlowTint ) * tex2DNode1 * tex2DNode3.a ) * _GlowStrength ).rgb;
			o.Metallic = tex2DNode3.b;
			o.Smoothness = tex2DNode3.g;
			o.Occlusion = ( tex2DNode3.r * _AOStrength );
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;1302.341,654.9858;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;1098.982,757.5818;Inherit;False;Property;_AOStrength;AOStrength;10;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;1019.54,-615.4194;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;807.5402,-675.4195;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;774.5404,-516.4197;Inherit;False;Property;_FresnelStrength;FresnelStrength;11;0;Create;True;0;0;0;False;0;False;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-465.1533,-962.2425;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-478.4883,-571.899;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;36;-837.3124,-564.6257;Inherit;False;Property;_MineralTintNoise;MineralTintNoise;7;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;9;-1895.026,-175.2347;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;37;-838.0525,-998.0325;Inherit;False;Property;_MineralTintBase;MineralTintBase;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;38;-281.6057,-808.6132;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-75.14929,-444.5034;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;41;-375.1493,-412.5034;Inherit;False;Property;_RockTint;RockTint;9;0;Create;True;0;0;0;False;0;False;0.5660378,0.5660378,0.5660378,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;39;153.6684,-471.7671;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;1506.177,-447.0317;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;23;473.9199,-606.1095;Inherit;False;Standard;WorldNormal;ViewDir;False;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;52;-2595.439,-39.2749;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-2800.439,6.725098;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;55;-3142.439,58.7251;Inherit;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;13;-2459.305,93.88828;Inherit;False;Property;_NoiseSpeed;NoiseSpeed;12;0;Create;True;0;0;0;False;0;False;0.005;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;12;-2219.305,83.88828;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;20;517.5358,372.8167;Inherit;False;Property;_GlowStrength;GlowStrength;6;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;701.2667,269.8159;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;327.248,138.5891;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1413.729,-705.2076;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;16;-313.5885,399.2306;Inherit;False;Property;_GlowTint;GlowTint;5;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;15;2.014141,221.0133;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-151.4345,129.6784;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-641.4256,110.5357;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;50;-467.5122,85.42001;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;33;-665.9884,-798.2633;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;32;-805.4292,-681.8248;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-734.6667,-170.8333;Inherit;True;Property;_Albedo;Albedo;0;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-933.9613,287.094;Inherit;True;Property;_Emissive;Emissive;3;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-69.87534,552.8379;Inherit;True;Property;_Masks;Masks;2;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;1206.337,-172.4714;Inherit;True;Property;_Normal;Normal;1;2;[Normal];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;8;-2553.736,-307.0019;Inherit;True;Property;_Depth_Noise;Depth_Noise;4;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;54;-3020.439,-38.2749;Inherit;False;Property;_DepthOffset;DepthOffset;13;0;Create;True;0;0;0;False;0;False;-0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1970.886,-50.68258;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;NV3D/SS_Minerals/MineralNode;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-1196.462,-713.1722;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;31;-1058.843,-808.4586;Inherit;False;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1.5;False;1;COLOR;0
WireConnection;21;0;3;1
WireConnection;21;1;22;0
WireConnection;26;0;24;0
WireConnection;26;1;27;0
WireConnection;24;0;23;0
WireConnection;24;1;3;4
WireConnection;34;0;37;0
WireConnection;34;1;33;0
WireConnection;35;0;32;0
WireConnection;35;1;36;0
WireConnection;9;0;8;0
WireConnection;9;1;12;0
WireConnection;38;0;34;0
WireConnection;38;1;35;0
WireConnection;38;2;32;0
WireConnection;40;0;1;0
WireConnection;40;1;41;0
WireConnection;39;0;40;0
WireConnection;39;1;38;0
WireConnection;39;2;3;4
WireConnection;28;0;26;0
WireConnection;28;1;39;0
WireConnection;52;1;53;0
WireConnection;53;0;54;0
WireConnection;53;1;55;0
WireConnection;12;0;52;0
WireConnection;12;2;13;0
WireConnection;56;0;17;0
WireConnection;56;1;20;0
WireConnection;17;0;14;0
WireConnection;17;1;1;0
WireConnection;17;2;3;4
WireConnection;15;1;16;0
WireConnection;15;2;4;0
WireConnection;14;0;50;0
WireConnection;14;1;16;0
WireConnection;59;0;4;0
WireConnection;59;1;9;0
WireConnection;50;0;59;0
WireConnection;33;0;31;0
WireConnection;32;0;29;0
WireConnection;0;0;28;0
WireConnection;0;1;2;0
WireConnection;0;2;56;0
WireConnection;0;3;3;3
WireConnection;0;4;3;2
WireConnection;0;5;21;0
WireConnection;29;0;9;0
WireConnection;29;1;30;0
WireConnection;31;0;29;0
ASEEND*/
//CHKSM=049BBDDF8BDECD729ABCCD288BE5BBC5BE1F54AA
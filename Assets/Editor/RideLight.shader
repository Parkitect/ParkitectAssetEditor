Shader "Rollercoaster/RideLight" {
	Properties {
		_Color ("Main Color", Color) = (1.0,1.0,1.0,1.0)
		_IllumMin ("Illumination (Min)", Range(0.0,10.0)) = 0.0
		_IllumMax ("Illumination (Max)", Range(0.0,10.0)) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert keepalpha noforwardadd

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.5

		// Enable instancing for this shader
		#pragma multi_compile_instancing
		#pragma instancing_options nolightmap nolightprobe procedural:manualInstancingSetup

		sampler2D _MainTex;
      	float _IllumMin;
		float _IllumMax;

		struct Input {
			float2 uv_MainTex;
		};

		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		struct LightParentData {
			float4x4 localToWorld;
		};
		
		struct LightInstancingData {
			float4x4 localToWorld;
			int parentIndex;
			int groupIndex;
			float twinkleIntensity;
		};

		struct LightEffectGroupData {
			float4 _Color;
			float _Intensity;
			int twinkle;
		};

		StructuredBuffer<LightInstancingData> LightInstancingDataBuffer;
		StructuredBuffer<LightEffectGroupData> LightEffectGroupDataBuffer;
		StructuredBuffer<LightParentData> LightParentDataBuffer;
		uint light_InstanceID;
		#endif

		void manualInstancingSetup() {
			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			LightInstancingData instanceData = LightInstancingDataBuffer[unity_InstanceID];
			unity_ObjectToWorld = mul(LightParentDataBuffer[instanceData.parentIndex].localToWorld, instanceData.localToWorld);
			light_InstanceID = unity_InstanceID;
			#endif
		}

		void surf (Input IN, inout SurfaceOutput o) {
			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			LightInstancingData instanceData = LightInstancingDataBuffer[unity_InstanceID];
			LightEffectGroupData effectData = LightEffectGroupDataBuffer[instanceData.groupIndex];
			half4 c = effectData._Color;
			
			o.Alpha = 1;

			half3 illuminationColor = c/10 * lerp(_IllumMin, _IllumMax, c.a * (effectData.twinkle == 1 ? instanceData.twinkleIntensity : 1));
			o.Emission = illuminationColor * effectData._Intensity;
			o.Albedo = illuminationColor * effectData._Intensity;
			#endif
		}
		ENDCG
	} 
	FallBack "Specular"
}

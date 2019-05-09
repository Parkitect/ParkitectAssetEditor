Shader "Rollercoaster/SceneViewGhost" {
	Properties {
		_Color ("Main Color", Color) = (1.0,1.0,1.0,1.0)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0
	}
	SubShader {
		Tags {"RenderType"="Transparent" "Queue"="Overlay"} // queue = overlay because, since it writes to Z, it needs to render as the very last thing (otherwise it for example can block other transparent objects)
		LOD 200
		Offset -1, -1

		// Render into depth buffer only
	    Pass {
	        ZWrite On
	        ColorMask 0
	   
	        CGPROGRAM
	        #pragma vertex vert
	        #pragma fragment frag
	        #include "UnityCG.cginc"
	 
	        struct v2f {
	            float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
	        };

			sampler2D _MainTex;
            float4 _MainTex_ST;
			half _Cutoff;
	 
	        v2f vert (appdata_base v) {
	            v2f o;
	            o.pos = UnityObjectToClipPos (v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
	            return o;
	        }
	 
	        half4 frag (v2f i) : COLOR {
				fixed4 col = tex2D(_MainTex, i.uv);
				clip(col.a - _Cutoff);

	            return half4(0.0, 0.0, 0.0, 0.0);
	        }
	        ENDCG  
	    }
	   
	    CGPROGRAM
		#include "CustomLighting.cginc"
	    #pragma surface surf LambertUnlit alpha:blend

	    fixed4 _Color;
		half _Cutoff;
		float _TimeUnscaled;
		sampler2D _MainTex;
	 
	    struct Input {
	        float2 uv_MainTex;
	    };

	    void surf (Input IN, inout SurfaceOutput o) {
	        fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			clip(c.a - _Cutoff);

			c *= _Color;

	        o.Albedo = c;
	        o.Alpha = c.a;
	    }
	    ENDCG
	} 
	FallBack "Transparent/Diffuse"
}
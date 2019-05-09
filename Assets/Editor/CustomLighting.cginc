// light functions are pretty much just compied from Lighting.cginc in the Unity builtin shaders, just removed the directional light color/intensity
inline fixed4 UnityLambertUnlitLight (SurfaceOutput s, UnityLight light) {
	fixed diff = max (0, dot (s.Normal, light.dir));

	fixed4 c;
	c.rgb = s.Albedo * diff;
	c.a = s.Alpha;
	return c;
}

inline fixed4 LightingLambertUnlit (SurfaceOutput s, UnityGI gi) {
	fixed4 c;
	c = UnityLambertUnlitLight(s, gi.light);

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		c.rgb += s.Albedo * 0.25f;
	#endif

	return c;
}

inline void LightingLambertUnlit_GI (
	SurfaceOutput s,
	UnityGIInput data,
	inout UnityGI gi) {
	gi = UnityGlobalIllumination (data, 1.0, s.Normal);
}
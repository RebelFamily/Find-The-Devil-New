Shader "Shader Graphs/Screen Effect" {
	Properties {
		Color_8edf0f63475846b19b9971f97f1a1ed6 ("Tint", Vector) = (0.175228,0.617,0.2110474,0.2352941)
		Vector1_6493ac4eff3141d8b0016aeb7820e907 ("Alpha", Float) = 0.2
		Vector1_c06970efdff643939b78ff7eaea9edb3 ("Scroll Speed", Float) = 0.0005
		Color_02eb8f840998434396a678b25bace988 ("OffColor", Vector) = (0.1886792,0.1886792,0.1886792,1)
		Vector1_51e77377b684463597764c291d5c68a3 ("ScreenOn", Range(0, 1)) = 0
		[NoScaleOffset] _Texture ("Texture", 2D) = "white" {}
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}
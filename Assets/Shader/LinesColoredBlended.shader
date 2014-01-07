Shader "Lines/Colored Blended" {
	Properties {
		_Color("Color & Transparency", Color) = (0, 0, 0, 1.0)
	}

	SubShader { 
		ColorMaterial AmbientAndDiffuse
		Pass { 
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			Cull Off 
			Fog { Mode Off } 
		} 
	} 
}
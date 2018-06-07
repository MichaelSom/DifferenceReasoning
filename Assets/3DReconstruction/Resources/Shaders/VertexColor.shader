//@author: Michael Sommerhalder, Dominik Frey, Nikhilesh Alatur
//
//Simple Shader to color the reconstruction mesh. Only ColorMaterial is needed,
//no fancy lighting etc.
//

Shader "Custom/Vertex Colored" {
	Properties{
	}
	SubShader{
		Pass{
			ColorMaterial AmbientAndDiffuse
		}
	}
}
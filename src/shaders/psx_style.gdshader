shader_type canvas_item;

uniform sampler2D screenTexture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float ditherGamma = 1.0f;

void fragment() {
	vec3 color = textureLod(screenTexture, SCREEN_UV, 0.0).rgb;
    
	int ps1_dither_matrix[16] = {
		-4, 0, -3, 1,
		2, -2, 3, -1,
		-3, 1, -4, 0,
		3, -1, 2, -2
	};
	
	// Index 1D dither matrix based on 2D screen coordinates
	float noise = float(ps1_dither_matrix[(int(FRAGCOORD.x) % 4) + (int(FRAGCOORD.y) % 4) * 4]);
	
	// Apply dithering and quantize 24 bit srgb to 15 bit srgb according to https://psx-spx.consoledev.net/graphicsprocessingunitgpu/
	vec3 quantized = pow(color, vec3(1.0f / ditherGamma)); // Convert to srgb cause it imo looks better and is probably correct idk looks more correct than linear quantization
	quantized = round(quantized * 255.0f + noise); // Convert to 0-255 and add dither noise
	quantized = clamp(round(quantized), vec3(0), vec3(255)); // Clamp to 0-255 in case of overflow
	quantized = clamp(quantized / 8.0f, vec3(0), vec3(31)); // Convert to 0-31 range
	quantized /= 31.0; // Convert back to 0-1 range
	
	quantized = pow(quantized, vec3(ditherGamma)); // Convert back to linear
	
	COLOR = vec4(quantized, 1.0f);
}
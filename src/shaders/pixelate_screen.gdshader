shader_type canvas_item;

uniform int amount = 2;

void fragment() {
	vec2 grid_uv = round(UV / pow(float(amount), 2));
	
	vec4 text = texture(TEXTURE, grid_uv);
	
	COLOR = text;
}
shader_type canvas_item;

// Outline color (RGBA)
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

// Outline thickness in pixels
uniform float outline_width : hint_range(0.0, 16.0, 0.5) = 2.0;

// Alpha threshold: pixels above this are considered "solid"
uniform float alpha_threshold : hint_range(0.0, 1.0, 0.01) = 0.1;

void fragment() {
	// COLOR already holds texture(TEXTURE, UV) * modulate coming in.
	// We use raw texture sample only for alpha comparisons.
	vec4 raw = texture(TEXTURE, UV);
	vec2 texel = TEXTURE_PIXEL_SIZE;

	bool near_solid = false;
	int steps = max(8, int(outline_width) * 4);
	for (int i = 0; i < steps; i++) {
		float angle = (float(i) / float(steps)) * TAU;
		vec2 offset = vec2(cos(angle), sin(angle)) * outline_width * texel;
		float neighbor_a = texture(TEXTURE, UV + offset).a;
		if (neighbor_a >= alpha_threshold) {
			near_solid = true;
			break;
		}
	}

	bool is_solid = raw.a >= alpha_threshold;
	// Solid pixels: leave COLOR as-is (modulation preserved).
	// Transparent + neighbour solid: draw outline.
	// Transparent + no neighbour: clear.
	if (!is_solid) {
		COLOR = near_solid ? outline_color : vec4(0.0);
	}
}

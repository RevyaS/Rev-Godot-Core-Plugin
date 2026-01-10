shader_type canvas_item;
render_mode blend_mix;

uniform float edge_strength : hint_range(0.0, 1.0) = 1.0;
uniform float inner_strength : hint_range(0.0, 1.0) = 0.0;
uniform float radius : hint_range(0.0, 1.0) = 0.4;
uniform float feather : hint_range(0.001, 1.0) = 0.2;

void fragment() {
	vec4 bloom_color = COLOR;
    vec2 centered = UV - vec2(0.5);
    vec2 box = abs(centered) * 2.0;

    float dist = max(box.x, box.y);

    // If inside the inner radius, *don’t draw at all* → transparent center
    if (dist <= radius) {
        discard;
    }

    // Compute inward fade:
    // At the exact edge of the node → bloom_factor = edge_strength
    // At radius inward → bloom_factor = inner_strength
    float bloom_factor = mix(edge_strength, inner_strength, smoothstep(radius + feather, radius, dist));

    COLOR = vec4(bloom_color.rgb, bloom_factor * bloom_color.a);
}
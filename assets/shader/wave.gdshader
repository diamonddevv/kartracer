shader_type canvas_item;

uniform float warp : hint_range(0.0, 5.0, 0.01) = 0.05;

float msqr(vec4 v) {
	return 
	v.r * v.r + 
	v.g * v.g + 
	v.b * v.b + 
	v.a * v.a;
}

bool closeenough(vec4 v, vec4 to, float tolerance) {
	float ts = tolerance * tolerance;
	float v_msqr = msqr(v);
	float to_msqr = msqr(to);
	return v_msqr >= to_msqr - ts && v_msqr <= to_msqr + ts;
}

vec4 t(sampler2D s, vec2 uv) {
	vec4 real = texture(s, uv);
	
	
	if (uv.y < 0.) {
		real = vec4(1);
	}
	if (uv.x > 1.) {
		real = vec4(1);
	}
	if (uv.x < 0.) {
		real = vec4(1);
	}
	
	if (uv.y > 0.93) {
		real = vec4(1); // no more banner watermark lol
	} 
	if (closeenough(real, vec4(1), 0.35)) {
		real = vec4(0);
	}
	
	return real;
}

void fragment() {
	vec2 newv = UV + vec2(
		warp * cos(radians(UV.y*360.) + TIME), 
		warp * sin(radians(UV.x*360.) + TIME)
	);
	
	COLOR = t(TEXTURE, newv);
}
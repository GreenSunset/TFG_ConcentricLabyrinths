//// MAIN ////
void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    // Normalized pixel coordinates (from 0 to 1)
    vec2 uv = fragCoord/iResolution.xy;
    // Time varying pixel color

    // Output to screen
    fragColor = texture(iChannel0, uv);
}




//// BUFFER B ////
#define MAX_STEPS 100.
#define MAX_DIST 100.
#define SURF_DIST .01

mat2 Rot(float a) {
    float c = cos(a);
    float s = sin(a);
    return mat2(c, -s, s, c);
}

float dBox(vec3 p, vec3 s) {
    return length(max(abs(p)-s,0.));
}


float sdTorus(vec3 p, vec2 r) {
    float x = length(p.xz)-r.x;
    return length(vec2(x, p.y))-r.y;
}

float sdCylinder(vec3 p, vec3 a, vec3 b, float r) {
    vec3 ab = b-a;
    vec3 ap = p-a;
    
    float t = dot(ab, ap) / dot(ab, ab);
    vec3 c = a + t*ab;
    
    float x = length(p-c) - r;
    
    float y = (abs(t-.5)-.5) * length(ab);
    float e = length(max(vec2(x,y),0.));
    float i = min(max(x,y),0.);
    return e+i;
}

float sdCapsule(vec3 p, vec3 a, vec3 b, float r) {
    vec3 ab = b-a;
    vec3 ap = p-a;
    
    float t = dot(ab, ap) / dot(ab, ab);
    t = clamp(t, 0., 1.);
    
    vec3 c = a + t*ab;
    
    return length(p-c) - r;
}

float GetDist(vec3 p) {
    //vec4 s = vec4(0,1,6,1);
    //float sDist = length(p-s.xyz)-s.w;
    float pDist = p.y;
    //float cDist = sdCapsule(p, vec3(0,1,6), vec3(1,2,6), .2);
    //float tDist = sdTorus(p-vec3(0,.5,6), vec2(1.5, .3));
    //float cyDist = sdCylinder(p, vec3(0,.3,6), vec3(1,1,4), .3);
    vec3 bp = vec3(0,1,0);
    bp.xz *= Rot(iTime);
    float bDist = dBox(p-bp, vec3(1));
    float d = min(pDist,bDist);
    //d = min(d, tDist);
    //d = min(d, cDist);
    //d = min(d, cyDist);
    
    return d;
}

vec3 GetNormal(vec3 p) {
    vec2 e = vec2(.01, 0);
    float d = GetDist(p);
    vec3 n = vec3(
        d-GetDist(p-e.xyy),
        d-GetDist(p-e.yxy),
        d-GetDist(p-e.yyx)
    );
    return normalize(n);
}

float RayMarch(vec3 ro, vec3 rd) {
    float dO = 0.;
    for (float steps=0.; steps<MAX_STEPS; steps++) {
        vec3 p = ro + rd*dO;
        float dS = GetDist(p);
        dO += dS;
        if (dO < SURF_DIST || dS > MAX_DIST) break;
    }
    return dO;
}
float RayMarch(vec3 ro, vec3 rd, out float steps) {
    float dO = 0.;
    for (steps=0.; steps<MAX_STEPS; steps++) {
        vec3 p = ro + rd*dO;
        float dS = GetDist(p);
        dO += dS;
        if (dO > MAX_DIST || dS< SURF_DIST ) break;
    }
    return dO;
}

float GetLight(vec3 p) {
    vec3 lp = vec3(0,3,0);
    lp.xz += vec2(sin(iTime), cos(iTime)) * 2.;
    vec3 l = normalize(lp-p);
    vec3 n = GetNormal(p);
    
    float dif = clamp(dot(n, l), 0., 1.);
    float d = RayMarch(p + n*SURF_DIST*2., l);
    if (d<length(lp-p)) dif *= .1;
    return dif;
}

vec3 GetRayDirection(vec3 ro, vec3 lp, vec2 uv, float zoom) {
    vec3 f = normalize(lp - ro);
    vec3 r = cross(vec3(0.,-1.,0.),f);
    vec3 u = cross(r,f);
    vec3 c = f * zoom;
    vec3 i = c + uv.x*r + uv.y*u;
    return i;
}

mat3 camera(vec3 cameraPos, vec3 lookAtPoint) {
	vec3 cd = normalize(lookAtPoint - cameraPos); // camera direction
	vec3 cr = normalize(cross(vec3(0, 1, 0), cd)); // camera right
	vec3 cu = normalize(cross(cd, cr)); // camera up
	
	return mat3(-cr, cu, -cd);
}


void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = (fragCoord-.5*iResolution.xy)/iResolution.y;
    vec3 ro = texelFetch(iChannel0, ivec2(0, 0), 0).xyz;
    vec3 camDir = texelFetch(iChannel0, ivec2(1, 0), 0).xyz;
    vec3 lp = ro + camDir;
    vec3 rd = camera(ro, lp) * normalize(vec3(uv, -1));
    
    //vec3 ro = vec3(3.,3.,-3.);
    //vec3 lookat = vec3(0.);
    //vec3 rd = GetRayDirection(ro, lookat, uv, 1.);
    float steps;
    float d = RayMarch(ro, rd, steps);
    
    vec3 p = ro + rd*d;
   
    float dif = GetLight(p);
    vec3 col = vec3(dif);
    col.g *= 1. + (steps  / 100.);

    // Output to screen
    fragColor = vec4(col,1.0);
}





//// BUFFER A ////
#define MAX_STEPS 100.
#define MAX_DIST 100.
#define SURF_DIST .01

mat2 Rot(float a) {
    float c = cos(a);
    float s = sin(a);
    return mat2(c, -s, s, c);
}

float dBox(vec3 p, vec3 s) {
    return length(max(abs(p)-s,0.));
}


float sdTorus(vec3 p, vec2 r) {
    float x = length(p.xz)-r.x;
    return length(vec2(x, p.y))-r.y;
}

float sdCylinder(vec3 p, vec3 a, vec3 b, float r) {
    vec3 ab = b-a;
    vec3 ap = p-a;
    
    float t = dot(ab, ap) / dot(ab, ab);
    vec3 c = a + t*ab;
    
    float x = length(p-c) - r;
    
    float y = (abs(t-.5)-.5) * length(ab);
    float e = length(max(vec2(x,y),0.));
    float i = min(max(x,y),0.);
    return e+i;
}

float sdCapsule(vec3 p, vec3 a, vec3 b, float r) {
    vec3 ab = b-a;
    vec3 ap = p-a;
    
    float t = dot(ab, ap) / dot(ab, ab);
    t = clamp(t, 0., 1.);
    
    vec3 c = a + t*ab;
    
    return length(p-c) - r;
}

float GetDist(vec3 p) {
    //vec4 s = vec4(0,1,6,1);
    //float sDist = length(p-s.xyz)-s.w;
    float pDist = p.y;
    //float cDist = sdCapsule(p, vec3(0,1,6), vec3(1,2,6), .2);
    //float tDist = sdTorus(p-vec3(0,.5,6), vec2(1.5, .3));
    //float cyDist = sdCylinder(p, vec3(0,.3,6), vec3(1,1,4), .3);
    vec3 bp = vec3(0,1,0);
    bp.xz *= Rot(iTime);
    float bDist = dBox(p-bp, vec3(1));
    float d = min(pDist,bDist);
    //d = min(d, tDist);
    //d = min(d, cDist);
    //d = min(d, cyDist);
    
    return d;
}

vec3 GetNormal(vec3 p) {
    vec2 e = vec2(.01, 0);
    float d = GetDist(p);
    vec3 n = vec3(
        d-GetDist(p-e.xyy),
        d-GetDist(p-e.yxy),
        d-GetDist(p-e.yyx)
    );
    return normalize(n);
}

float RayMarch(vec3 ro, vec3 rd) {
    float dO = 0.;
    for (float steps=0.; steps<MAX_STEPS; steps++) {
        vec3 p = ro + rd*dO;
        float dS = GetDist(p);
        dO += dS;
        if (dO < SURF_DIST || dS > MAX_DIST) break;
    }
    return dO;
}
float RayMarch(vec3 ro, vec3 rd, out float steps) {
    float dO = 0.;
    for (steps=0.; steps<MAX_STEPS; steps++) {
        vec3 p = ro + rd*dO;
        float dS = GetDist(p);
        dO += dS;
        if (dO > MAX_DIST || dS< SURF_DIST ) break;
    }
    return dO;
}

float GetLight(vec3 p) {
    vec3 lp = vec3(0,3,0);
    lp.xz += vec2(sin(iTime), cos(iTime)) * 2.;
    vec3 l = normalize(lp-p);
    vec3 n = GetNormal(p);
    
    float dif = clamp(dot(n, l), 0., 1.);
    float d = RayMarch(p + n*SURF_DIST*2., l);
    if (d<length(lp-p)) dif *= .1;
    return dif;
}

vec3 GetRayDirection(vec3 ro, vec3 lp, vec2 uv, float zoom) {
    vec3 f = normalize(lp - ro);
    vec3 r = cross(vec3(0.,-1.,0.),f);
    vec3 u = cross(r,f);
    vec3 c = f * zoom;
    vec3 i = c + uv.x*r + uv.y*u;
    return i;
}

mat3 camera(vec3 cameraPos, vec3 lookAtPoint) {
	vec3 cd = normalize(lookAtPoint - cameraPos); // camera direction
	vec3 cr = normalize(cross(vec3(0, 1, 0), cd)); // camera right
	vec3 cu = normalize(cross(cd, cr)); // camera up
	
	return mat3(-cr, cu, -cd);
}


void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = (fragCoord-.5*iResolution.xy)/iResolution.y;
    vec3 ro = texelFetch(iChannel0, ivec2(0, 0), 0).xyz;
    vec3 camDir = texelFetch(iChannel0, ivec2(1, 0), 0).xyz;
    vec3 lp = ro + camDir;
    vec3 rd = camera(ro, lp) * normalize(vec3(uv, -1));
    
    //vec3 ro = vec3(3.,3.,-3.);
    //vec3 lookat = vec3(0.);
    //vec3 rd = GetRayDirection(ro, lookat, uv, 1.);
    float steps;
    float d = RayMarch(ro, rd, steps);
    
    vec3 p = ro + rd*d;
   
    float dif = GetLight(p);
    vec3 col = vec3(dif);
    col.g *= 1. + (steps  / 100.);

    // Output to screen
    fragColor = vec4(col,1.0);
}
#ifndef ADDITIONAL_LIGHT_INCLUDED
#define ADDITIONAL_LIGHT_INCLUDED

// ============================================================
// MAIN LIGHT (Directional Light)
// ============================================================
void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction = normalize(float3(1.0f, 1.0f, 0.0f));
    Color = 1.0f;
    DistanceAtten = 1.0f;
    ShadowAtten = 1.0f;
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    Light mainLight = GetMainLight(shadowCoord);
    
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void MainLight_half(half3 WorldPos, out half3 Direction, out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction = normalize(half3(1.0f, 1.0f, 0.0f));
    Color = 1.0f;
    DistanceAtten = 1.0f;
    ShadowAtten = 1.0f;
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    Light mainLight = GetMainLight(shadowCoord);
    
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

// ============================================================
// SINGLE ADDITIONAL LIGHT (Point / Spot Light)
// ============================================================
void AdditionalLight_float(float3 WorldPos, int lightID, out float3 Direction, out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
    Direction = normalize(float3(1.0f, 1.0f, 0.0f));
    Color = 0.0f;
    DistanceAtten = 0.0f;
    ShadowAtten = 1.0f;

#ifndef SHADERGRAPH_PREVIEW
    int lightCount = GetAdditionalLightsCount();
    if (lightID < lightCount)
    {
        half4 shadowMask = half4(1.0, 1.0, 1.0, 1.0);
        Light light = GetAdditionalLight(lightID, WorldPos, shadowMask);
        
        Direction = light.direction;
        Color = light.color;
        DistanceAtten = light.distanceAttenuation;
        ShadowAtten = light.shadowAttenuation;
    }
#endif
}

void AdditionalLight_half(half3 WorldPos, int lightID, out float3 Direction, out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
    Direction = normalize(half3(1.0f, 1.0f, 0.0f));
    Color = 0.0f;
    DistanceAtten = 0.0f;
    ShadowAtten = 1.0f;

#ifndef SHADERGRAPH_PREVIEW
    int lightCount = GetAdditionalLightsCount();
    if (lightID < lightCount)
    {
        half4 shadowMask = half4(1.0, 1.0, 1.0, 1.0);
        Light light = GetAdditionalLight(lightID, WorldPos, shadowMask);
        
        Direction = light.direction;
        Color = light.color;
        DistanceAtten = light.distanceAttenuation;
        ShadowAtten = light.shadowAttenuation;
    }
#endif
}

// ============================================================
// ALL ADDITIONAL LIGHTS (Looping All Lights)
// ============================================================
void AllAdditionalLights_float(float3 WorldPos, float3 WorldNormal, float2 CutoffThresholds, out float3 LightColor)
{
    LightColor = 0.0f;

#ifndef SHADERGRAPH_PREVIEW
    int lightCount = GetAdditionalLightsCount();
    half4 shadowMask = half4(1.0, 1.0, 1.0, 1.0);

    for (int i = 0; i < lightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPos, shadowMask);

        half NdotL = dot(light.direction, WorldNormal);
        half celStep = smoothstep(CutoffThresholds.x, CutoffThresholds.y, NdotL);
        
        // Gabungkan Cel Shading + Distance + Shadow tanpa tumpang tindih
        half finalAtten = celStep * light.distanceAttenuation * light.shadowAttenuation;

        LightColor += light.color * finalAtten;
    }
#endif
}

void AllAdditionalLights_half(half3 WorldPos, half3 WorldNormal, half2 CutoffThresholds, out float3 LightColor)
{
    LightColor = 0.0f;

#ifndef SHADERGRAPH_PREVIEW
    int lightCount = GetAdditionalLightsCount();
    half4 shadowMask = half4(1.0, 1.0, 1.0, 1.0);

    for (int i = 0; i < lightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPos, shadowMask);

        half NdotL = dot(light.direction, WorldNormal);
        half celStep = smoothstep(CutoffThresholds.x, CutoffThresholds.y, NdotL);
        
        half finalAtten = celStep * light.distanceAttenuation * light.shadowAttenuation;

        LightColor += light.color * finalAtten;
    }
#endif
}

#endif
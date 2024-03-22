using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_2023_1_OR_NEWER
[Serializable, VolumeComponentMenu("WoBok/Volumetric Clouds"), SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
#else
[Serializable, VolumeComponentMenuForRenderPipeline("WoBok/Volumetric Clouds", typeof(UniversalRenderPipeline))]
#endif
public class VolumetricCloudsVolume : VolumeComponent, IPostProcessComponent
{
    [Header("March Settings")]
    public ClampedFloatParameter numStepsLight = new(0, 20, 20);
    public ClampedFloatParameter rayOffsetStrength = new(0, 15, 10);
    public TextureParameter blueNoise = new(null);

    [Header("Base Shape")]
    public ClampedFloatParameter cloudScale = new(0, 100, 1);
    public ClampedFloatParameter densityMultiplier = new(0, 100, 1);
    public ClampedFloatParameter densityOffset = new(0, 100, -1);
    public Vector4Parameter shapeNoiseWeights = new(new Vector4(6, 0.5f, 1, 2));
    public Vector3Parameter shapeOffset = new(new Vector3(2, 0, 0));
    public Vector2Parameter heightOffset = new(new Vector2(0, 0));

    [Header("Detail")]
    public ClampedFloatParameter detailNoiseScale = new(1, 100, 2);
    public ClampedFloatParameter detailNoiseWeight = new(1, 100, 5);
    public Vector3Parameter detailNoiseWeights = new(new Vector3(15, 0.5f, 0.5f));
    public Vector3Parameter detailOffset = new(new Vector3(5, 0, 0));

    [Header("Lighting")]
    public ClampedFloatParameter lightAbsorptionThroughCloud = new(0, 100, 1);
    public ClampedFloatParameter lightAbsorptionTowardSun = new(0, 100, 2);
    [Range(0, 1)]
    public ClampedFloatParameter darknessThreshold = new(0, 1, 0.2f);
    [Range(0, 1)]
    public ClampedFloatParameter forwardScattering = new(0, 1, 0.8f);
    [Range(0, 1)]
    public ClampedFloatParameter backScattering = new(0, 1, 0.3f);
    [Range(0, 1)]
    public ClampedFloatParameter baseBrightness = new(0, 1, 0.8f);
    [Range(0, 1)]
    public ClampedFloatParameter phaseFactor = new(0, 1, 0.15f);

    [Header("Animation")]
    public ClampedFloatParameter timeScale = new(0, 1, 1);
    public ClampedFloatParameter baseSpeed = new(0, 1, 1);
    public ClampedFloatParameter detailSpeed = new(0, 1, 2);

    [Header("Sky")]
    public ColorParameter colA = new(Color.white);
    public ColorParameter colB = new(Color.white);

    public bool IsActive()
    {
        return true;//Todo
    }
    public bool IsTileCompatible() => false;
}
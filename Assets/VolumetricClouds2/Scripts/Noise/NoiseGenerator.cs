using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    const int computeThreadGroupSize = 8;
    public const string detailNoiseName = "DetailNoise";
    public const string shapeNoiseName = "ShapeNoise";

    public enum CloudNoiseType { Shape, Detail }
    public enum TextureChannel { R, G, B, A }

    [Header("Editor Settings")]
    public CloudNoiseType activeTextureType;
    public TextureChannel activeChannel;
    public bool autoUpdate;
    public bool logComputeTime;

    [Header("Noise Settings")]
    public int shapeResolution = 128;
    public int detailResolution = 64;

    public WorleyNoiseSettings[] shapeSettings;
    public WorleyNoiseSettings[] detailSettings;
    public ComputeShader noiseCompute;
    public ComputeShader copy;

    [Header("Viewer Settings")]
    public bool viewerEnabled;
    public bool viewerGreyscale = true;
    public bool viewerShowAllChannels;
    [Range(0, 1)]
    public float viewerSliceDepth;
    [Range(1, 5)]
    public float viewerTileAmount = 1;
    [Range(0, 1)]
    public float viewerSize = 1;

    // Internal
    List<ComputeBuffer> m_BuffersToRelease = new List<ComputeBuffer>();
    bool updateNoise;

    [HideInInspector]
    public bool showSettingsEditor = true;
    [SerializeField, HideInInspector]
    public RenderTexture shapeTexture;
    [SerializeField, HideInInspector]
    public RenderTexture detailTexture;

    public RenderTexture ActiveTexture
    {
        get
        {
            return activeTextureType == CloudNoiseType.Shape ? shapeTexture : detailTexture;
        }
    }
    public WorleyNoiseSettings ActiveSettings
    {
        get
        {
            WorleyNoiseSettings[] settings = activeTextureType == CloudNoiseType.Shape ? shapeSettings : detailSettings;
            int activeChannelIndex = (int)activeChannel;
            if (activeChannelIndex >= settings.Length)
            {
                return null;
            }
            return settings[activeChannelIndex];
        }
    }
    public Vector4 ChannelMask
    {
        get
        {
            Vector4 channelWeight = new Vector4(
                (activeChannel == NoiseGenerator.TextureChannel.R) ? 1 : 0,
                (activeChannel == NoiseGenerator.TextureChannel.G) ? 1 : 0,
                (activeChannel == NoiseGenerator.TextureChannel.B) ? 1 : 0,
                (activeChannel == NoiseGenerator.TextureChannel.A) ? 1 : 0
            );
            return channelWeight;
        }
    }
    public void UpdateNoise()
    {
        ValidateParamaters();
        CreateTexture(ref shapeTexture, shapeResolution, shapeNoiseName);
        CreateTexture(ref detailTexture, detailResolution, detailNoiseName);

        if (updateNoise && noiseCompute)
        {
            updateNoise = false;
            if (ActiveSettings == null) return;
            UpdateCompute(ActiveSettings, ActiveTexture);
        }
    }
    void ValidateParamaters()
    {
        detailResolution = Mathf.Max(1, detailResolution);
        shapeResolution = Mathf.Max(1, shapeResolution);
    }
    public void UpdateCompute(WorleyNoiseSettings settings, RenderTexture texture)
    {
        UpdateWorley(settings);

        int activeTextureResolution = texture.width;
        noiseCompute.SetFloat("persistence", settings.persistence);
        noiseCompute.SetInt("resolution", activeTextureResolution);
        noiseCompute.SetVector("channelMask", ChannelMask);

        noiseCompute.SetTexture(0, "Result", texture);
        noiseCompute.SetTexture(1, "Result", texture);

        var minMaxBuffer = CreateBuffer(new int[] { int.MaxValue, 0 }, sizeof(int), "minMax", 0);
        noiseCompute.SetBuffer(1, "minMax", minMaxBuffer);

        int numThreadGroups = Mathf.CeilToInt(activeTextureResolution / (float)computeThreadGroupSize);
        noiseCompute.Dispatch(0, numThreadGroups, numThreadGroups, numThreadGroups);
        noiseCompute.Dispatch(1, numThreadGroups, numThreadGroups, numThreadGroups);

        if (logComputeTime)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var minMax = new int[2];
            minMaxBuffer.GetData(minMax);
            Debug.Log($"Noise Generation: {timer.ElapsedMilliseconds}ms");
        }
        ReleaseBuffer();
    }
    void UpdateWorley(WorleyNoiseSettings settings)
    {
        var prng = new System.Random(settings.seed);
        CreateWorleyPointsBuffer(prng, settings.numDivisionsA, "pointsA");
        CreateWorleyPointsBuffer(prng, settings.numDivisionsB, "pointsB");
        CreateWorleyPointsBuffer(prng, settings.numDivisionsC, "pointsC");

        noiseCompute.SetInt("numCellsA", settings.numDivisionsA);
        noiseCompute.SetInt("numCellsB", settings.numDivisionsB);
        noiseCompute.SetInt("numCellsC", settings.numDivisionsC);
        noiseCompute.SetBool("invertNoise", settings.invert);
        noiseCompute.SetInt("tile", settings.tile);
    }
    void CreateWorleyPointsBuffer(System.Random prng, int numCellsPerAxis, string bufferName)
    {
        var points = new Vector3[numCellsPerAxis * numCellsPerAxis * numCellsPerAxis];
        float cellSize = 1f / numCellsPerAxis;

        for (int x = 0; x < numCellsPerAxis; x++)
        {
            for (int y = 0; y < numCellsPerAxis; y++)
            {
                for (int z = 0; z < numCellsPerAxis; z++)
                {
                    float randomX = (float)prng.NextDouble();
                    float randomY = (float)prng.NextDouble();
                    float randomZ = (float)prng.NextDouble();
                    Vector3 randomOffset = new Vector3(randomX, randomY, randomZ) * cellSize;
                    Vector3 cellCorner = new Vector3(x, y, z) * cellSize;
                    int index = x + numCellsPerAxis * (y + z * numCellsPerAxis);
                    points[index] = cellCorner + randomOffset;
                }
            }
        }
        CreateBuffer(points, sizeof(float) * 3, bufferName);
    }
    ComputeBuffer CreateBuffer(System.Array data, int stride, string bufferName, int kernel = 0)
    {
        var buffer = new ComputeBuffer(data.Length, stride, ComputeBufferType.Structured);
        m_BuffersToRelease.Add(buffer);
        buffer.SetData(data);
        noiseCompute.SetBuffer(kernel, bufferName, buffer);
        return buffer;
    }
    void ReleaseBuffer()
    {
        foreach (var buffer in m_BuffersToRelease)
        {
            buffer.Release();
        }
    }
    public void Load(string saveName, RenderTexture target)
    {
        Texture3D savedTex = (Texture3D)Resources.Load("CloudTextures/" + saveName);
        if (savedTex != null && savedTex.width == target.width)
        {
            copy.SetTexture(0, "tex", savedTex);
            copy.SetTexture(0, "renderTex", target);
            int numThreadGroups = Mathf.CeilToInt(savedTex.width / 8f);
            copy.Dispatch(0, numThreadGroups, numThreadGroups, numThreadGroups);
        }
    }
    void CreateTexture(ref RenderTexture texture, int resolution, string name)
    {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
        if (texture == null || !texture.IsCreated() || texture.width != resolution || texture.height != resolution || texture.volumeDepth != resolution || texture.graphicsFormat != format)
        {
            if (texture != null)
            {
                texture.Release();
            }
            texture = new RenderTexture(resolution, resolution, 0);
            texture.graphicsFormat = format;
            texture.volumeDepth = resolution;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture.name = name;
            texture.Create();
            Load(name, texture);
        }
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
    }
    public void ManualUpdate()
    {
        updateNoise = true;
        UpdateNoise();
    }
    public void ActiveNoiseSettingsChanged()
    {
        if (autoUpdate)
        {
            updateNoise = true;
        }
    }
}
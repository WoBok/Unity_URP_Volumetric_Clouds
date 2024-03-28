using System;
using System.Collections.Generic;
using UnityEngine;

namespace VolumetricClouds.Editor
{
    public class NoiseGenerator
    {
        public enum CloudNoiseType { Shape, Detail }
        public enum TextureChannel { R, G, B, A }

        static NoiseGenerator s_Instance;
        public static NoiseGenerator Instance { get { if (s_Instance == null) s_Instance = new NoiseGenerator(); return s_Instance; } set => s_Instance = value; }

        public const string DETAILNOISENAME = "DetailNoise";
        public const string SHAPENOISENAME = "ShapeNoise";
        const int COMPUTETHREADGROUPSIZE = 8;

        public Action<RenderTexture[]> UpdateNoise;

        public CloudNoiseType activeTextureType;
        public TextureChannel activeChannel;

        public int shapeResolution = 128;
        public int detailResolution = 32;

        public RenderTexture shapeTexture;
        public RenderTexture detailTexture;

        public WorleyNoiseSettings[] shapeSettings;
        public WorleyNoiseSettings[] detailSettings;
        public ComputeShader noiseCompute;
        public ComputeShader copy;
        List<ComputeBuffer> m_BuffersToRelease = new List<ComputeBuffer>();

        public RenderTexture ActiveTexture => activeTextureType == CloudNoiseType.Shape ? shapeTexture : detailTexture;
        public WorleyNoiseSettings ActiveSettings
        {
            get
            {
                WorleyNoiseSettings[] settings = activeTextureType == CloudNoiseType.Shape ? shapeSettings : detailSettings;
                var activeChannelIndex = (int)activeChannel;
                if (activeChannelIndex >= settings.Length)
                    return null;
                return settings[activeChannelIndex];
            }
        }
        public Vector4 ChannelMask
        {
            get
            {
                return new Vector4(
                    (activeChannel == TextureChannel.R) ? 1 : 0,
                    (activeChannel == TextureChannel.G) ? 1 : 0,
                    (activeChannel == TextureChannel.B) ? 1 : 0,
                    (activeChannel == TextureChannel.A) ? 1 : 0
                );
            }
        }
        public void Init()
        {
            CreateTexture();
            UpdateCompute();
            UpdateNoise?.Invoke(new RenderTexture[] { shapeTexture, detailTexture });
        }
        public void Update()
        {
            UpdateCompute();
            UpdateNoise?.Invoke(new RenderTexture[] { shapeTexture, detailTexture });
        }
        void CreateTexture()
        {
            ValidateParamaters();
            CreateTexture(ref shapeTexture, shapeResolution, SHAPENOISENAME);
            CreateTexture(ref detailTexture, detailResolution, DETAILNOISENAME);
        }
        void ValidateParamaters()
        {
            detailResolution = Mathf.Max(1, detailResolution);
            shapeResolution = Mathf.Max(1, shapeResolution);
        }
        public void CreateTexture(ref RenderTexture texture, int resolution, string name)
        {
            var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
            if (texture == null || !texture.IsCreated() || texture.width != resolution || texture.height != resolution || texture.volumeDepth != resolution || texture.graphicsFormat != format)
            {
                texture = new RenderTexture(resolution, resolution, 0);
                texture.wrapMode = TextureWrapMode.Repeat;
                texture.filterMode = FilterMode.Bilinear;
                texture.graphicsFormat = format;
                texture.volumeDepth = resolution;
                texture.enableRandomWrite = true;
                texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
                texture.name = name;
                texture.Create();
                Load(name, texture);
            }
        }
        public void Load(string saveName, RenderTexture target)
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            saveName = sceneName + "_" + saveName;
            var savedTex = Resources.Load<Texture3D>(saveName);
            if (savedTex != null && savedTex.width == target.width)
            {
                copy.SetTexture(0, "tex", savedTex);
                copy.SetTexture(0, "renderTex", target);

                int numThreadGroups = Mathf.CeilToInt(savedTex.width / 8f);
                copy.Dispatch(0, numThreadGroups, numThreadGroups, numThreadGroups);
            }
            else
                Debug.LogError("Load noise texture failed!");
        }
        public void UpdateCompute()
        {
            if (ActiveSettings == null) return;

            UpdateWorley();

            noiseCompute.SetFloat("persistence", ActiveSettings.persistence);
            noiseCompute.SetInt("resolution", ActiveTexture.width);
            noiseCompute.SetVector("channelMask", ChannelMask);

            noiseCompute.SetTexture(0, "Result", ActiveTexture);
            noiseCompute.SetTexture(1, "Result", ActiveTexture);

            var minMaxBuffer = CreateBuffer(new int[] { int.MaxValue, 0 }, sizeof(int), "minMax", 0);
            noiseCompute.SetBuffer(1, "minMax", minMaxBuffer);

            var numThreadGroups = Mathf.CeilToInt(ActiveTexture.width / (float)COMPUTETHREADGROUPSIZE);
            noiseCompute.Dispatch(0, numThreadGroups, numThreadGroups, numThreadGroups);
            noiseCompute.Dispatch(1, numThreadGroups, numThreadGroups, numThreadGroups);

            ReleaseBuffer();
        }
        void UpdateWorley()
        {
            var random = new System.Random(ActiveSettings.seed);
            CreateWorleyPointsBuffer(random, ActiveSettings.divisionsA, "pointsA");
            CreateWorleyPointsBuffer(random, ActiveSettings.divisionsB, "pointsB");
            CreateWorleyPointsBuffer(random, ActiveSettings.divisionsC, "pointsC");

            noiseCompute.SetInt("numCellsA", ActiveSettings.divisionsA);
            noiseCompute.SetInt("numCellsB", ActiveSettings.divisionsB);
            noiseCompute.SetInt("numCellsC", ActiveSettings.divisionsC);
            noiseCompute.SetBool("invertNoise", ActiveSettings.invert);
            noiseCompute.SetInt("tile", ActiveSettings.tile);
        }
        void CreateWorleyPointsBuffer(System.Random random, int numCellsPerAxis, string bufferName)
        {
            var points = new Vector3[numCellsPerAxis * numCellsPerAxis * numCellsPerAxis];
            float cellSize = 1f / numCellsPerAxis;

            for (int x = 0; x < numCellsPerAxis; x++)
            {
                for (int y = 0; y < numCellsPerAxis; y++)
                {
                    for (int z = 0; z < numCellsPerAxis; z++)
                    {
                        float randomX = (float)random.NextDouble();
                        float randomY = (float)random.NextDouble();
                        float randomZ = (float)random.NextDouble();
                        Vector3 randomOffset = new Vector3(randomX, randomY, randomZ) * cellSize;
                        Vector3 cellCorner = new Vector3(x, y, z) * cellSize;
                        int index = x + numCellsPerAxis * (y + z * numCellsPerAxis);
                        points[index] = cellCorner + randomOffset;
                    }
                }
            }
            CreateBuffer(points, sizeof(float) * 3, bufferName);
        }
        ComputeBuffer CreateBuffer(Array data, int stride, string bufferName, int kernel = 0)
        {
            var buffer = new ComputeBuffer(data.Length, stride, ComputeBufferType.Structured);
            buffer.SetData(data);
            noiseCompute.SetBuffer(kernel, bufferName, buffer);
            m_BuffersToRelease.Add(buffer);
            return buffer;
        }
        void ReleaseBuffer()
        {
            foreach (var buffer in m_BuffersToRelease)
            {
                buffer.Release();
            }
        }
    }
}
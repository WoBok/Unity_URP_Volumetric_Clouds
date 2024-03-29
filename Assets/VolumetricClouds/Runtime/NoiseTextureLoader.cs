using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricClouds
{
    public class NoiseTextureLoader
    {
        public static void CreateTexture(ref RenderTexture texture, int resolution, string name)
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
                texture.dimension = TextureDimension.Tex3D;
                texture.name = name;
                texture.Create();
                Load(name, texture);
            }
        }
        public static void Load(string saveName, RenderTexture target)
        {
            var copy = Resources.Load<ComputeShader>("ComputeShader/Copy");

            if (copy == null)
            {
                Debug.LogError("Cannot find  copy compute shader in Resources/ComputeShader!");
                return;
            }
            if (target == null)
            {
                Debug.LogError("Target render texture cannot be empty!");
                return;
            }
            var savedTex = Resources.Load<Texture3D>("CloudTextures/" + saveName);
            if (savedTex != null)
            {
                if (savedTex.width == target.width)
                {
                    copy.SetTexture(0, "tex", savedTex);
                    copy.SetTexture(0, "renderTex", target);

                    int numThreadGroups = Mathf.CeilToInt(savedTex.width / 8f);
                    copy.Dispatch(0, numThreadGroups, numThreadGroups, numThreadGroups);
                }
                else
                    Debug.LogError("The newly created texture does not match the size of the stored texture!");
            }
            else
                Debug.LogError("Load stored texture failed!");
        }
    }
}
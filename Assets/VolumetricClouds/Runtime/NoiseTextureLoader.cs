using UnityEngine;

namespace VolumetricClouds
{
    public class NoiseTextureLoader
    {
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
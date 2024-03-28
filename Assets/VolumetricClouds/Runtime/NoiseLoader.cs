using UnityEngine;

namespace VolumetricClouds
{
    public class NoiseLoader
    {
        public static void Load(string saveName, RenderTexture target, ComputeShader copy)
        {
            if (copy == null)
            {
                Debug.LogError("Copy compute shader cannot be empty!");
                return;
            }
            if (target == null)
            {
                Debug.LogError("Target render texture cannot be empty!");
                return;
            }
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
    }
}
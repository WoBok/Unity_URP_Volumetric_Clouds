using System.IO;
using UnityEngine;

namespace VolumetricClouds
{
    public class NoiseTextureInfo : ScriptableObject
    {
        [Range(1, 512)]
        public int shapeResolution;
        [Range(1, 512)]
        public int detailResolution;
        public string shapeNoiseName;
        public string detailNoiseName;

        static NoiseTextureInfo s_NoiseTextureInfo;
        public static NoiseTextureInfo Info
        {
            get
            {
                if (s_NoiseTextureInfo == null)
                {
                    s_NoiseTextureInfo = Resources.Load<NoiseTextureInfo>("NoiseTextureInfo");
#if UNITY_EDITOR
                    if (s_NoiseTextureInfo == null)
                    {
                        s_NoiseTextureInfo = CreateInstance<NoiseTextureInfo>();
                        s_NoiseTextureInfo.shapeResolution = 128;
                        s_NoiseTextureInfo.detailResolution = 64;
                        s_NoiseTextureInfo.shapeNoiseName = "ShapeNoise";
                        s_NoiseTextureInfo.detailNoiseName = "DetailNoise";

                        string path = Application.dataPath + "/Resources";
                        if (!Directory.Exists(path))
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                            UnityEditor.AssetDatabase.CreateAsset(s_NoiseTextureInfo, "Assets/Resources/NoiseTextureInfo.asset");
                        }
                        else
                        {
                            UnityEditor.AssetDatabase.CreateAsset(s_NoiseTextureInfo, "Assets/Resources/NoiseTextureInfo.asset");
                        }
                    }
#endif
                }
                return s_NoiseTextureInfo;
            }
        }
    }
}
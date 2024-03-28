using UnityEngine;

namespace VolumetricClouds.Editor
{
    public class NoiseTextureInfo : ScriptableObject
    {
        [Range(1, 512)]
        public int shapeResolution = 128;
        [Range(1, 512)]
        public int detailResolution = 64;
        public string shapeNoiseName = "ShapeNoise";
        public string detailNoiseName = "DetailNoise";

        static NoiseTextureInfo s_NoiseTextureInfo;
        public static NoiseTextureInfo Info
        {
            get
            {

                if (s_NoiseTextureInfo == null)
                {
                    s_NoiseTextureInfo = Resources.Load<NoiseTextureInfo>("Settings/NoiseTextureInfo");
                    if (s_NoiseTextureInfo == null)
                    {
                        s_NoiseTextureInfo = CreateInstance<NoiseTextureInfo>();
                        s_NoiseTextureInfo.shapeResolution = 128;
                        s_NoiseTextureInfo.detailResolution = 64;
                        s_NoiseTextureInfo.shapeNoiseName = "ShapeNoise";
                        s_NoiseTextureInfo.detailNoiseName = "DetailNoise";
                    }
                }
                return s_NoiseTextureInfo;
            }
        }
    }
}
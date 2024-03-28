using UnityEngine;

namespace VolumetricClouds.Editor
{
    [CreateAssetMenu]
    public class WorleyNoiseSettings : ScriptableObject
    {
        public int seed;
        [Range(1, 50)]
        public int divisionsA = 5;
        [Range(1, 50)]
        public int divisionsB = 10;
        [Range(1, 50)]
        public int divisionsC = 15;
        public float persistence = .5f;
        public int tile = 1;
        public bool invert = true;
    }
}
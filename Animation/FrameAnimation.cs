using UnityEngine;

namespace Pigeon
{
    [CreateAssetMenu()]
    public class FrameAnimation : ScriptableObject
    {
        public bool loop;
        public bool mirror;

        [Space(10)]
        public float frameDelay = 0.1f;

        [Space(20)]
        public Sprite[] sprites;

        [Space(20)]
        public Sprite endSprite;
    }
}
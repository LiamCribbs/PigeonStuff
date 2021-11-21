using System.Collections;
using UnityEngine;

namespace Pigeon
{
    public class Animator : MonoBehaviour
    {
        public new SpriteRenderer renderer;

        [Space(10)]
        public bool playOnAwake;
        public float frameDelayMultiplier = 1f;

        [Space(10)]
        public FrameAnimation[] animations;
        public FrameAnimation currentAnimation;

        Coroutine animationCoroutine;

        public System.Action<int> onSpriteChanged;
        public System.Action onAnimationComplete;

        public bool IsPlaying
        {
            get => animationCoroutine != null;
        }

        public bool IsPlayingLoopedAnimation
        {
            get => IsPlaying && currentAnimation.loop;
        }

        public void Play(FrameAnimation animation)
        {
            Stop();
            currentAnimation = animation;
            animationCoroutine = StartCoroutine(Animate());
        }

        public void Play(int animationIndex)
        {
            Play(animations[animationIndex]);
        }

        public void Play(string animationName)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i].name == animationName)
                {
                    Play(animations[i]);
                    return;
                }
            }

            throw new System.Exception("No animation found with name " + animationName);
        }

        public void Stop()
        {
            if (IsPlaying)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
                currentAnimation = null;
            }
        }

        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        IEnumerator Animate()
        {
            WaitForSeconds wait = new WaitForSeconds(currentAnimation.frameDelay * frameDelayMultiplier);

            while (true)
            {
                int length = currentAnimation.sprites.Length;

                for (int i = 0; i < length; i++)
                {
                    renderer.sprite = currentAnimation.sprites[i];
                    onSpriteChanged?.Invoke(i);
                    if (i < length - 1)
                    {
                        yield return wait;
                    }
                }

                if (currentAnimation.mirror)
                {
                    yield return wait;

                    for (int i = length - 1; i >= 0; i--)
                    {
                        renderer.sprite = currentAnimation.sprites[i];
                        onSpriteChanged?.Invoke(i);
                        if (i > 0)
                        {
                            yield return wait;
                        }
                    }
                }

                if (!currentAnimation.loop)
                {
                    break;
                }
            }

            if (currentAnimation.endSprite != null)
            {
                yield return wait;
                renderer.sprite = currentAnimation.endSprite;
            }

            onAnimationComplete?.Invoke();
            currentAnimation = null;
            animationCoroutine = null;
        }

        void Awake()
        {
            if (playOnAwake)
            {
                Play(currentAnimation);
            }
        }

        void Reset()
        {
            renderer = GetComponent<SpriteRenderer>();
            if (!renderer)
            {
                renderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
    }
}
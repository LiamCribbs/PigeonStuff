using System.Collections;
using UnityEngine;

namespace Pigeon
{
    public class FrameAnimator : MonoBehaviour
    {
        public new SpriteRenderer renderer;
        public UnityEngine.UI.Image imageRenderer;

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

        [ContextMenu("Play")]
        void EDITOR_Play()
        {
            if (Application.isPlaying)
            {
                Play(currentAnimation ?? animations[0]);
            }
        }

        [ContextMenu("PlayReverse")]
        void EDITOR_PlayReverse()
        {
            if (Application.isPlaying)
            {
                Play(currentAnimation ?? animations[0], true);
            }
        }

        public void Play(FrameAnimation animation, bool reverse = false)
        {
            Stop();
            currentAnimation = animation;
            animationCoroutine = StartCoroutine(imageRenderer != null ? AnimateImage(reverse) : Animate(reverse));
        }

        public void Play(int animationIndex, bool reverse = false)
        {
            Play(animations[animationIndex], reverse);
        }

        public void Play(string animationName, bool reverse = false)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i].name == animationName)
                {
                    Play(animations[i], reverse);
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
            if (imageRenderer != null)
            {
                imageRenderer.sprite = sprite;
            }
            else
            {
                renderer.sprite = sprite;
            }
        }

        IEnumerator Animate(bool reverse = false)
        {
            WaitForSeconds wait = new WaitForSeconds(currentAnimation.frameDelay * frameDelayMultiplier);

            while (true)
            {
                int length = currentAnimation.sprites.Length;

                for (int i = 0; i < length; i++)
                {
                    int index = reverse ? length - 1 - i : i;
                    renderer.sprite = currentAnimation.sprites[index];
                    onSpriteChanged?.Invoke(index);
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
                        int index = reverse ? length - 1 - i : i;
                        renderer.sprite = currentAnimation.sprites[index];
                        onSpriteChanged?.Invoke(index);
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

        IEnumerator AnimateImage(bool reverse = false)
        {
            WaitForSeconds wait = new WaitForSeconds(currentAnimation.frameDelay * frameDelayMultiplier);

            while (true)
            {
                int length = currentAnimation.sprites.Length;

                for (int i = 0; i < length; i++)
                {
                    int index = reverse ? length - 1 - i : i;
                    imageRenderer.sprite = currentAnimation.sprites[index];
                    onSpriteChanged?.Invoke(index);
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
                        int index = reverse ? length - 1 - i : i;
                        imageRenderer.sprite = currentAnimation.sprites[index];
                        onSpriteChanged?.Invoke(index);
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
                imageRenderer.sprite = currentAnimation.endSprite;
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
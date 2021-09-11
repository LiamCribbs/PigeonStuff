using System;
using System.Collections;
using UnityEngine;

namespace Pigeon
{
    public static class EaseFunctions
    {
        public delegate float EvaluateMode(float x);

        /// <summary>
        /// Exponential, Circle, and Elastic functions are slower because they use Pow and/or Sqrt. Exponential can be estimated with Quintic.
        /// </summary>
        public enum EaseMode
        {
            EaseInSin, EaseOutSin, EaseInOutSin, EaseInQuadratic, EaseOutQuadratic, EaseInOutQuadratic, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInQuartic,
            EaseOutQuartic, EaseInOutQuartic, EaseInQuintic, EaseOutQuintic, EaseInOutQuintic, EaseInExponential, EaseOutExponential, EaseInOutExponential,
            EaseInCircle, EaseOutCircle, EaseInOutCircle, EaseInBack, EaseOutBack, EaseInOutBack, EaseInElastic, EaseOutElastic, EaseInOutElastic,
            EaseInBounce, EaseOutBounce, EaseInOutBounce, AnimationCurve
        }

        public static EvaluateMode SetEaseMode(EaseMode mode)
        {
            switch (mode)
            {
                case EaseMode.EaseInSin:
                    return EaseInSin;
                case EaseMode.EaseOutSin:
                    return EaseOutSin;
                case EaseMode.EaseInOutSin:
                    return EaseInOutSin;
                case EaseMode.EaseInQuadratic:
                    return EaseInQuadratic;
                case EaseMode.EaseOutQuadratic:
                    return EaseOutQuadratic;
                case EaseMode.EaseInOutQuadratic:
                    return EaseInOutQuadratic;
                case EaseMode.EaseInCubic:
                    return EaseInCubic;
                case EaseMode.EaseOutCubic:
                    return EaseOutCubic;
                case EaseMode.EaseInOutCubic:
                    return EaseInOutCubic;
                case EaseMode.EaseInQuartic:
                    return EaseInQuartic;
                case EaseMode.EaseOutQuartic:
                    return EaseOutQuartic;
                case EaseMode.EaseInOutQuartic:
                    return EaseInOutQuartic;
                case EaseMode.EaseInQuintic:
                    return EaseInQuintic;
                case EaseMode.EaseOutQuintic:
                    return EaseOutQuintic;
                case EaseMode.EaseInOutQuintic:
                    return EaseInOutQuintic;
                case EaseMode.EaseInExponential:
                    return EaseInExponential;
                case EaseMode.EaseOutExponential:
                    return EaseOutExponential;
                case EaseMode.EaseInOutExponential:
                    return EaseInOutExponential;
                case EaseMode.EaseInCircle:
                    return EaseInCircle;
                case EaseMode.EaseOutCircle:
                    return EaseOutCircle;
                case EaseMode.EaseInOutCircle:
                    return EaseInOutCircle;
                case EaseMode.EaseInBack:
                    return EaseInBack;
                case EaseMode.EaseOutBack:
                    return EaseOutBack;
                case EaseMode.EaseInOutBack:
                    return EaseInOutBack;
                case EaseMode.EaseInElastic:
                    return EaseInElastic;
                case EaseMode.EaseOutElastic:
                    return EaseOutElastic;
                case EaseMode.EaseInOutElastic:
                    return EaseInOutElastic;
                case EaseMode.EaseInBounce:
                    return EaseInBounce;
                case EaseMode.EaseOutBounce:
                    return EaseOutBounce;
                case EaseMode.EaseInOutBounce:
                    return EaseInOutBounce;
                case EaseMode.AnimationCurve:
                    throw new Exception("I didn't feel like adding this.");
                default:
                    return EaseOutQuintic;
            }
        }

        public static IEnumerator LerpValueOverTime(Action<float> OnValueChanged, float value, float targetValue, float speed, EvaluateMode EvaluateMode)
        {
            float initValue = value;
            float time = 0f;

            while (time < 1f)
            {
                time += Time.deltaTime * speed;
                if (time > 1f)
                {
                    time = 1f;
                }
                value = Mathf.LerpUnclamped(initValue, targetValue, EvaluateMode(time));
                OnValueChanged?.Invoke(value);
                yield return null;
            }
        }

        public static float EaseInSin(float x)
        {
            return 1f - Mathf.Cos(x * Mathf.PI / 2f);
        }

        public static float EaseOutSin(float x)
        {
            return 1f - Mathf.Sin(x * Mathf.PI / 2f);
        }

        public static float EaseInOutSin(float x)
        {
            return -(Mathf.Cos(Mathf.PI * x) - 1f) / 2f;
        }

        public static float EaseInQuadratic(float x)
        {
            return x * x;
        }

        public static float EaseOutQuadratic(float x)
        {
            return 1f - ((1f - x) * (1f - x));
        }

        public static float EaseInOutQuadratic(float x)
        {
            return x < 0.5f ? 2f * x * x : 1f - (-2f * x + 2f) * (-2f * x + 2f) / 2f;
        }

        public static float EaseInCubic(float x)
        {
            return x * x * x;
        }

        public static float EaseOutCubic(float x)
        {
            return 1f - ((1f - x) * (1f - x) * (1f - x));
        }

        public static float EaseInOutCubic(float x)
        {
            return x < 0.5f ? 4f * x * x * x : 1f - (-2f * x + 2f) * (-2f * x + 2f) * (-2f * x + 2f) / 2f;
        }

        public static float EaseInQuartic(float x)
        {
            return x * x * x * x;
        }

        public static float EaseOutQuartic(float x)
        {
            return 1f - ((1f - x) * (1f - x) * (1f - x) * (1f - x));
        }

        public static float EaseInOutQuartic(float x)
        {
            return x < 0.5f ? 8f * x * x * x * x : 1f - (-2f * x + 2f) * (-2f * x + 2f) * (-2f * x + 2f) * (-2f * x + 2f) / 2f;
        }

        public static float EaseInQuintic(float x)
        {
            return x * x * x * x * x;
        }

        public static float EaseOutQuintic(float x)
        {
            return 1f - ((1f - x) * (1f - x) * (1f - x) * (1f - x) * (1f - x));
        }

        public static float EaseInOutQuintic(float x)
        {
            return x < 0.5f ? 16f * x * x * x * x * x : 1f - (-2f * x + 2f) * (-2f * x + 2f) * (-2f * x + 2f) * (-2f * x + 2f) * (-2f * x + 2f) / 2f;
        }

        public static float EaseInExponential(float x)
        {
            return x == 0f ? 0f : Mathf.Pow(2f, 10f * x - 10f);
        }

        public static float EaseOutExponential(float x)
        {
            return x == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * x);
        }

        public static float EaseInOutExponential(float x)
        {
            return x == 0f ? 0f : x == 1f ? 1f : x < 0.5f ? Mathf.Pow(2f, 20f * x - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * x + 10f)) / 2f;
        }

        public static float EaseInCircle(float x)
        {
            return 1f - Mathf.Sqrt(1f - (x * x));
        }

        public static float EaseOutCircle(float x)
        {
            return Mathf.Sqrt(1f - ((x - 1f) * (x - 1f)));
        }

        public static float EaseInOutCircle(float x)
        {
            return x < 0.5f ? (1f - Mathf.Sqrt(1f - (2f * x * 2f * x))) / 2f : (Mathf.Sqrt(1f - ((-2f * x + 2f) * (-2f * x + 2f))) + 1f) / 2f;
        }

        public static float EaseInBack(float x)
        {
            return 2.70158f * x * x * x - 1.70158f * x * x;
        }

        public static float EaseOutBack(float x)
        {
            return 1f + 2.70158f * ((x - 1f) * (x - 1f) * (x - 1f)) + 1.70158f * ((x - 1f) * (x - 1f));
        }

        public static float EaseInOutBack(float x)
        {
            return x < 0.5f ? (2f * x) * (2f * x) * ((2.59491f + 1f) * 2f * x - 2.59491f) / 2f : ((2f * x - 2f) * (2f * x - 2f) * ((2.59491f + 1f) * (x * 2f - 2f) + 2.59491f) + 2f) / 2f;
        }

        public static float EaseInElastic(float x)
        {
            return x == 0f ? 0f : x == 1f ? 1f : -Mathf.Pow(2f, 10f * x - 10f) * Mathf.Sin((x * 10f - 10.75f) * 2.09439f);
        }

        public static float EaseOutElastic(float x)
        {
            return x == 0f ? 0f : x >= 1f ? 1f : Mathf.Pow(2f, -10f * x) * Mathf.Sin((x * 10f - 0.75f) * (2f * Mathf.PI / 3f)) + 1f;
        }

        public static float EaseInOutElastic(float x)
        {
            return x == 0f ? 0f : x >= 1f ? 1f : x < 0.5f ? -(Mathf.Pow(2f, 20f * x - 10f) * Mathf.Sin((20f * x - 11.125f) * (2f * Mathf.PI) / 4.5f)) / 2f
                : Mathf.Pow(2f, -20f * x + 10f) * Mathf.Sin((20f * x - 11.125f) * (2f * Mathf.PI) / 4.5f) / 2f + 1f;
        }

        public static float EaseInBounce(float x)
        {
            return 1f - EaseOutBounce(1f - x);
        }

        public static float EaseOutBounce(float x)
        {
            if (x < 1f / 2.75f)
            {
                return 7.5625f * x * x;
            }
            else if (x < 2f / 2.75f)
            {
                return 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f;
            }
            else if (x < 2.5f / 2.75f)
            {
                return 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f;
            }
            else
            {
                return 7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f;
            }
        }

        public static float EaseInOutBounce(float x)
        {
            return x < 0.5f ? (1f - EaseOutBounce(1f - 2f * x)) / 2f : (1f + EaseOutBounce(2f * x - 1f)) / 2f;
        }

        public static float BellCurveSin(float x)
        {
            return Mathf.Sin(x * Mathf.PI);
        }

        public static float BellCurveQuadratic(float x)
        {
            return -((x - 0.5f) * (x - 0.5f)) * 4f + 1f;
        }

        /// <summary>
        /// Combine two ease functions using power of 1
        /// </summary>
        public static float CombineHalf(float x, EvaluateMode A, EvaluateMode B)
        {
            float a = A(x);
            return a + (1f - x) * (B(x) - a);
        }

        /// <summary>
        /// Combine two ease functions using power of 0.5
        /// </summary>
        public static float CombineQuarter(float x, EvaluateMode A, EvaluateMode B)
        {
            float a = A(x);
            return a + (1f - Math.MathUtil.SqrtVeryFast(x)) * (B(x) - a);
        }

        /// <summary>
        /// Combine two ease functions
        /// 
        /// Power -- 0 returns A, 1 returns in between, infinity returns B
        /// </summary>
        public static float Combine(float x, EvaluateMode A, EvaluateMode B, float power)
        {
            float a = A(x);
            return a + Mathf.Pow(1f - x, power) * (B(x) - a);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Pigeon.Math
{
    public static class MathUtil
    {
        public static int CoordinateHash(Vector2Int coordinate)
        {
            // Magic
            uint state = (uint)(46340L * coordinate.y + coordinate.x + int.MaxValue);

            state ^= 2747636419u;
            state *= 2654435769u;
            state ^= state >> 16;
            state *= 2654435769u;
            state ^= state >> 16;
            state *= 2654435769u;
            return unchecked((int)state);
        }

        public static int Hash(int state)
        {
            uint uState = unchecked((uint)state);

            uState ^= 2747636419u;
            uState *= 2654435769u;
            uState ^= uState >> 16;
            uState *= 2654435769u;
            uState ^= uState >> 16;
            return unchecked((int)(uState * 2654435769u));
        }

        /// <summary>
        /// Return a hash of this int
        /// </summary>
        public static int HashThis(this int state)
        {
            uint uState = unchecked((uint)state);

            uState ^= 2747636419u;
            uState *= 2654435769u;
            uState ^= uState >> 16;
            uState *= 2654435769u;
            uState ^= uState >> 16;
            return unchecked((int)(uState * 2654435769u));
        }

        public static uint Hash(uint uState)
        {
            uState ^= 2747636419u;
            uState *= 2654435769u;
            uState ^= uState >> 16;
            uState *= 2654435769u;
            uState ^= uState >> 16;
            return uState * 2654435769u;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NextBool(this System.Random random)
        {
            return (~random.Next() & 1) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RandomIntToBool(int randomNumber)
        {
            return (~randomNumber & 1) == 0;
        }

        /// <summary>
        /// Mathf.LerpAngle, but unclamped
        /// </summary>
        public static float LerpAngleUnclamped(float a, float b, float t)
        {
            float delta = Mathf.Repeat((b - a), 360);
            if (delta > 180)
                delta -= 360;
            return a + delta * t;
        }

        #region Fast Square Root
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        struct FloatIntUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public float f;

            [System.Runtime.InteropServices.FieldOffset(0)]
            public int tmp;
        }

        /// <summary>
        /// Very good approximation if we don't need exact decimals
        /// </summary>
        public static float SqrtFast(float z)
        {
            if (z == 0) return 0;
            FloatIntUnion u;
            u.tmp = 0;
            float xhalf = 0.5f * z;
            u.f = z;
            u.tmp = 0x5f375a86 - (u.tmp >> 1);
            u.f = u.f * (1.5f - xhalf * u.f * u.f);
            return u.f * z;
        }

        /// <summary>
        /// Within ~0.1 for low numbers, gets pretty far off for large numbers
        /// </summary>
        public static float SqrtVeryFast(float z)
        {
            if (z == 0) return 0;
            FloatIntUnion u;
            u.tmp = 0;
            u.f = z;
            u.tmp -= 1 << 23; // Subtract 2^m.
            u.tmp >>= 1; // Divide by 2.
            u.tmp += 1 << 29; // Add ((b + 1) / 2) * 2^m.
            return u.f;
        }
        #endregion
    }

    public static class Vector3Extension
    {
        /// <summary>
        /// Very good approximation if we don't need exact decimals
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MagFast(this Vector3 v)
        {
            return MathUtil.SqrtFast(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        /// <summary>
        /// Within ~0.1 for low numbers, gets pretty far off for large numbers
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MagVeryFast(this Vector3 v)
        {
            return MathUtil.SqrtVeryFast(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        /// <summary>
        /// Very good approximation if we don't need exact decimals
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizedFast(this Vector3 v)
        {
            float mag = MathUtil.SqrtFast(v.x * v.x + v.y * v.y + v.z * v.z);
            return mag == 0f ? Vector3.zero : v / mag;
        }

        /// <summary>
        /// Within ~0.1 for low numbers, gets pretty far off for large numbers
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizedVeryFast(this Vector3 v)
        {
            float mag = MathUtil.SqrtVeryFast(v.x * v.x + v.y * v.y + v.z * v.z);
            return mag == 0f ? Vector3.zero : v / mag;
        }

        /// <summary>
        /// Very good approximation if we don't need exact decimals
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MagFast(this Vector2 v)
        {
            return MathUtil.SqrtFast(v.x * v.x + v.y * v.y);
        }

        /// <summary>
        /// Within ~0.1 for low numbers, gets pretty far off for large numbers
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MagVeryFast(this Vector2 v)
        {
            return MathUtil.SqrtVeryFast(v.x * v.x + v.y * v.y);
        }

        /// <summary>
        /// Very good approximation if we don't need exact decimals
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NormalizedFast(this Vector2 v)
        {
            float mag = MathUtil.SqrtFast(v.x * v.x + v.y * v.y);
            return mag == 0f ? Vector2.zero : v / mag;
        }

        /// <summary>
        /// Within ~0.1 for low numbers, gets pretty far off for large numbers
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NormalizedVeryFast(this Vector2 v)
        {
            float mag = MathUtil.SqrtVeryFast(v.x * v.x + v.y * v.y);
            return mag == 0f ? Vector2.zero : v / mag;
        }
    }
}
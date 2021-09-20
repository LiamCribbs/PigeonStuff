using UnityEngine;

public static class BetterPrint
{
    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Print(this MonoBehaviour monoBehaviour, params object[] messages)
    {
        string message = "";
        int length = messages.Length;
        for (int i = 0; i < length; i++)
        {
            message += i < length - 1 ? (messages[i] == null ? "null" : messages[i].ToString()) + " | " : (messages[i] == null ? "null" : messages[i].ToString());
        }

        Debug.Log(message);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Print(params object[] messages)
    {
        string message = "";
        int length = messages.Length;
        for (int i = 0; i < length; i++)
        {
            message += i < length - 1 ? (messages[i] == null ? "null" : messages[i].ToString()) + " | " : (messages[i] == null ? "null" : messages[i].ToString());
        }

        Debug.Log(message);
    }
}
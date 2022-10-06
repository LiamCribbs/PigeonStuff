using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Pigeon.Sequencer
{
    [Serializable]
    public abstract class OverrideField
    {
        [SerializeReference] [HideInInspector] RefInt index = new RefInt();

        /// <summary>
        /// Order of this field in the sequence
        /// </summary>
        public int Index { get => index.value; internal set => index.value = value; }

        /// <summary>
        /// Check if this field has the same type and index as other
        /// </summary>
        public bool ValueEquals(OverrideField other)
        {
            return other != null && index.value == other.index.value && GetType() == other.GetType();
        }

        internal virtual bool IsGeneric => false;
    }

    [Serializable]
    public abstract class OverrideField<T> : OverrideField
    {
        public T value;

        internal override bool IsGeneric => true;

        public static implicit operator T(OverrideField<T> o) => o.value;
    }

    [Serializable]
    public class OverrideUnityEvent<T> : OverrideField<UnityEngine.Events.UnityEvent<T>> { };

    [Serializable]
    public class OverrideUnityEvent<T1, T2> : OverrideField<UnityEngine.Events.UnityEvent<T1, T2>> { };
    [Serializable]
    public class OverrideUnityEvent<T1, T2, T3> : OverrideField<UnityEngine.Events.UnityEvent<T1, T2, T3>> { };
    [Serializable]
    public class OverrideUnityEvent<T1, T2, T3, T4> : OverrideField<UnityEngine.Events.UnityEvent<T1, T2, T3, T4>> { };

    [Serializable]
    public class OverrideGameObject : OverrideField<GameObject> { };

    [Serializable]
    public class OverrideTransform : OverrideField<Transform> { };

    [Serializable]
    public class OverrideVector3 : OverrideField<Vector3> { };

    [Serializable]
    public class OverrideString : OverrideField<string> { };

    [Serializable]
    public class OverrideAudioSource : OverrideField<AudioSource> { };

    [Serializable]
    public class OverrideSpriteRenderer : OverrideField<SpriteRenderer> { };

    [Serializable]
    public class OverrideParticleSystem : OverrideField<ParticleSystem> { };

    [Serializable]
    public class OverrideUnityEventSequencePlayer : OverrideUnityEvent<SequencePlayer> { };

    [Serializable]
    public class OverrideUnityEventRefBool : OverrideUnityEvent<SequencePlayer, RefBool> { };

    public class RefIEnumerator
    {
        public IEnumerator enumerator;
    }

    [Serializable]
    public class OverrideUnityEventRefIEnumerator : OverrideUnityEvent<RefIEnumerator> { };
    [Serializable]
    public class OverrideGameObjectArray : OverrideField<GameObject[]> { };

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(OverrideField<>), true)]
    class OverrideFieldDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(OverrideUnityEvent<>), true)]
    class OverrideUnityEventDrawer : PropertyDrawer
    {
        UnityEventDrawer eventDrawer;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (eventDrawer == null)
            {
                eventDrawer = new UnityEventDrawer();
            }

            return eventDrawer.GetPropertyHeight(property.FindPropertyRelative("value"), GUIContent.none);
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (eventDrawer == null)
            {
                eventDrawer = new UnityEventDrawer();
            }

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            //EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), GUIContent.none);
            eventDrawer.OnGUI(position, property.FindPropertyRelative("value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(OverrideUnityEvent<,>), true)]
    class OverrideUnityEvent2Drawer : PropertyDrawer
    {
        UnityEventDrawer eventDrawer;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (eventDrawer == null)
            {
                eventDrawer = new UnityEventDrawer();
            }

            return eventDrawer.GetPropertyHeight(property.FindPropertyRelative("value"), GUIContent.none);
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (eventDrawer == null)
            {
                eventDrawer = new UnityEventDrawer();
            }

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            //EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), GUIContent.none);
            eventDrawer.OnGUI(position, property.FindPropertyRelative("value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(OverrideUnityEvent<,,>), true)]
    class OverrideUnityEvent3Drawer : PropertyDrawer
    {
        UnityEventDrawer eventDrawer;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (eventDrawer == null)
            {
                eventDrawer = new UnityEventDrawer();
            }

            return eventDrawer.GetPropertyHeight(property.FindPropertyRelative("value"), GUIContent.none);
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (eventDrawer == null)
            {
                eventDrawer = new UnityEventDrawer();
            }

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            //EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), GUIContent.none);
            eventDrawer.OnGUI(position, property.FindPropertyRelative("value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(OverrideUnityEvent<,,,>), true)]
    class OverrideUnityEvent4Drawer : PropertyDrawer
    {
        UnityEventDrawer eventDrawer;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (eventDrawer == null)
            {
                eventDrawer = new UnityEventDrawer();
            }

            return eventDrawer.GetPropertyHeight(property.FindPropertyRelative("value"), GUIContent.none);
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (eventDrawer == null)
            {
                eventDrawer = new UnityEventDrawer();
            }

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            //EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), GUIContent.none);
            eventDrawer.OnGUI(position, property.FindPropertyRelative("value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
#endif
}
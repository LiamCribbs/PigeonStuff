using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pigeon.Sequencer
{
    /// <summary>
    /// MonoBehaviour handler that plays a <see cref="Sequencer.Sequence"/>
    /// </summary>
    public class SequencePlayer : MonoBehaviour
    {
        [SerializeField] Sequence sequence;

        public Sequence Sequence { get; set; }

        [SerializeField] bool playOnAwake;

        Coroutine sequenceCoroutine;

        /// <summary>
        /// Index of the currently playing node.
        /// This ref int points to an index in a nested node if an <see cref="INestedSequenceNode"/> is currently playing.
        /// </summary>
        public RefInt NodeEnumerationIndex { get; set; }

        [SerializeReference] List<OverrideFieldList> overrides = new List<OverrideFieldList>();

        [Serializable]
        class OverrideFieldList
        {
            [SerializeField] public Sequence sequence;
            [SerializeField] public byte[] nodeID = null;

            [SerializeReference] public List<OverrideField> list = new List<OverrideField>();
            [SerializeField] public List<bool> enabled = new List<bool>();

            public OverrideFieldList(Sequence sequence, byte[] nodeID)
            {
                this.sequence = sequence;
                this.nodeID = nodeID;
            }
        }

        /// <summary>
        /// Is the sequence currently playing?
        /// </summary>
        public bool IsPlaying => sequenceCoroutine != null;

        /// <summary>
        /// Fired when the sequence is canceled with <see cref="Stop"/>
        /// </summary>
        public Action<SequencePlayer> onStop;

        /// <summary>
        /// Fired when the sequence finishes playing
        /// </summary>
        public Action onFinish;

        Dictionary<int, object> parameters;

        public T GetParameter<T>(string name)
        {
            return parameters == null || !parameters.TryGetValue(Shader.PropertyToID(name), out object value) ? default : (T)value;
        }

        public T GetParameter<T>(int id)
        {
            return parameters == null || !parameters.TryGetValue(id, out object value) ? default : (T)value;
        }

        public void SetParameter<T>(string name, T value)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<int, object>();
            }

            parameters[Shader.PropertyToID(name)] = value;
        }

        public void SetParameter<T>(int id, T value)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<int, object>();
            }

            parameters[id] = value;
        }

        /// <summary>
        /// Get this player's override corresponding to a node's OverrideField from this sequence. Returns the given field if this override is not enabled.
        /// </summary>
        public T GetOverride<T>(T field, Sequence sequence, byte[] id) where T : OverrideField
        {
            for (int i = 0; i < overrides.Count; i++)
            {
                var o = overrides[i];

                if (o.sequence == sequence && INestedSequenceNode.AreIDsEqual(o.nodeID, id))
                {
                    for (int j = 0; j < o.list.Count; j++)
                    {
                        if (field.Index == o.list[j].Index)
                        {
                            return o.enabled[j] ? (T)o.list[j] : field;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Manually get an OverrideField at index i
        /// </summary>
        public OverrideField<T> ManualGetOverride<T>(int i, int listIndex = 0)
        {
            return overrides[listIndex].enabled[i] ? (OverrideField<T>)overrides[listIndex].list[i] : null;
        }

        void Awake()
        {
            if (playOnAwake)
            {
                Play();
            }
        }

        void OnDestroy()
        {
            Stop();
        }

        /// <summary>
        /// Play this sequence. Will do nothing if the sequence is already playing
        /// </summary>
        public void Play()
        {
            if (sequenceCoroutine == null)
            {
                sequenceCoroutine = StartCoroutine(EnumerateSequence());
            }
        }

        /// <summary>
        /// Play this sequence. Will stop the sequence first if its currently playing.
        /// </summary>
        public void Restart()
        {
            Stop();
            sequenceCoroutine = StartCoroutine(EnumerateSequence());
        }

        /// <summary>
        /// Stops the sequence if it's currently playing
        /// </summary>
        public void Stop()
        {
            if (sequenceCoroutine != null)
            {
                onStop?.Invoke(this);
                StopAllCoroutines();
                sequenceCoroutine = null;
            }
        }

        [ContextMenu("Start Sequence")]
        void EDITOR_StartSequence()
        {
            if (Application.isPlaying)
            {
                Play();
            }
        }

        [ContextMenu("Restart Sequence")]
        void EDITOR_RestartSequence()
        {
            if (Application.isPlaying)
            {
                Restart();
            }
        }

        [ContextMenu("Stop Sequence")]
        void EDITOR_StopSequence()
        {
            if (Application.isPlaying)
            {
                Stop();
            }
        }

        /// <summary>
        /// Coroutine that plays sequence over time
        /// </summary>
        IEnumerator EnumerateSequence()
        {
            RefInt index = new RefInt();

            if (sequence.Parallel)
            {
                RefInt completedCoroutineCount = new RefInt(0);
                int startedCoroutines = 0;

                for (index.value = 0; index.value < sequence.Nodes.Count; index.value++)
                {
                    NodeEnumerationIndex = index;

                    IEnumerator nodeEnumerator = sequence.Nodes[index.value].Invoke(this, sequence, null);
                    if (nodeEnumerator != null)
                    {
                        //StartCoroutine(nodeEnumerator);
                        StartCoroutine(Sequence.RunParallelCoroutine(nodeEnumerator, completedCoroutineCount));
                        startedCoroutines++;
                    }
                }

                while (completedCoroutineCount.value < startedCoroutines)
                {
                    yield return null;
                }
            }
            else
            {
                for (index.value = 0; index.value < sequence.Nodes.Count; index.value++)
                {
                    NodeEnumerationIndex = index;

                    IEnumerator nodeEnumerator = sequence.Nodes[index.value].Invoke(this, sequence, null);
                    if (nodeEnumerator != null)
                    {
                        yield return nodeEnumerator;
                    }
                }
            }

            NodeEnumerationIndex = null;
            sequenceCoroutine = null;

            onFinish?.Invoke();
        }

        /// <summary>
        /// Set the index of the current node being played
        /// </summary>
        internal void SetEnumerationIndex(int value)
        {
            NodeEnumerationIndex.value = value;
        }

        /// <summary>
        /// Set the index of the current node being played
        /// </summary>
        internal void IncreaseEnumerationIndex(int amount)
        {
            NodeEnumerationIndex.value += amount;
        }

        /// <summary>
        /// Set the index of the current node being played
        /// </summary>
        internal void DecreaseEnumerationIndex(int amount)
        {
            NodeEnumerationIndex.value -= amount;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(SequencePlayer))]
        [CanEditMultipleObjects()]
        class SequenceHandlerEditor : Editor
        {
            Editor sequenceEditor;
            static bool foldout = true;

            HashSet<int> usedOverrideLists = new HashSet<int>();

            static readonly GUIContent overrideLabel = new GUIContent(""/*, "Override this field?"*/);
            static readonly GUIContent overrideHeader = new GUIContent("Overrides", "These fields from the sequence can be overriden on this object" +
                "\n\nHold click on an override to see its corresponding node in the sequence");

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                SequencePlayer player = (SequencePlayer)target;

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(player.sequence)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(player.playOnAwake)));

                //EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(player.allOverrides)));

                if (player.sequence == null)
                {
                    serializedObject.ApplyModifiedProperties();
                    return;
                }

                GUILayout.Space(20f);
                GUILayout.Label(overrideHeader);

                // Draw sequence inspector
                CreateCachedEditor(player.sequence, null, ref sequenceEditor);

                List<OverrideField> sequenceOverrides = player.sequence.Overrides;
                int selectedNodeIndex = -1;
                byte[] selectedNodeID = null;

                //if (sequenceOverrides.Count == 0)
                //{
                //    goto SyncNestedOverrides;
                //}

                usedOverrideLists.Clear();
                usedOverrideLists.Add(0);

                if (player.overrides.Count == 0)
                {
                    player.overrides.Add(new OverrideFieldList(player.sequence, null));
                }

                if (player.overrides[0].sequence != player.sequence)
                {
                    player.overrides[0].sequence = player.sequence;
                }
                if (player.overrides[0].nodeID != null)
                {
                    player.overrides[0].nodeID = null;
                }

                SyncOverrides(sequenceOverrides, player.overrides[0]);

                //SyncNestedOverrides:

                for (int i = 0; i < player.sequence.Nodes.Count; i++)
                {
                    Node node = player.sequence.Nodes[i];
                    if (node is INestedSequenceNode nestedSequence)
                    {
                        Sequence sequence = nestedSequence.GetSequence();
                        byte[] nodeID = nestedSequence.GetID();
                        bool foundMatchingList = false;

                        for (int j = 1; j < player.overrides.Count; j++)
                        {
                            var overrideList = player.overrides[j];
                            if (overrideList.sequence == sequence && INestedSequenceNode.AreIDsEqual(overrideList.nodeID, nodeID))
                            {
                                SyncOverrides(sequence.Overrides, overrideList);
                                usedOverrideLists.Add(j);
                                foundMatchingList = true;
                                break;
                            }
                        }

                        if (!foundMatchingList)
                        {
                            player.overrides.Add(new OverrideFieldList(sequence, nodeID));
                            SyncOverrides(sequence.Overrides, player.overrides[player.overrides.Count - 1]);
                            usedOverrideLists.Add(player.overrides.Count - 1);
                        }
                    }
                }

                for (int i = player.overrides.Count - 1; i >= 0; i--)
                {
                    if (!usedOverrideLists.Contains(i))
                    {
                        player.overrides.RemoveAt(i);
                    }
                }

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                if (player.sequence == null)
                {
                    return;
                }

                SerializedProperty overridesProp = serializedObject.FindProperty(nameof(player.overrides));

                for (int i = 0; i < overridesProp.arraySize; i++)
                {
                    DrawOverrides(i, overridesProp.GetArrayElementAtIndex(i), player.overrides[i], ref selectedNodeIndex, ref selectedNodeID);
                }

                GUILayout.Space(32f);

                // Draw sequence editor
                foldout = EditorGUILayout.InspectorTitlebar(foldout, player.sequence);
                if (foldout)
                {
                    //sequenceEditor.OnInspectorGUI();
                    ((Sequence.SequenceEditor)sequenceEditor).DrawNodesInOtherEditor(selectedNodeIndex, selectedNodeID, null, 0);
                }

                serializedObject.ApplyModifiedProperties();
            }

            void SyncOverrides(List<OverrideField> sequenceOverrides, OverrideFieldList thisOverrides)
            {
                while (thisOverrides.enabled.Count < thisOverrides.list.Count)
                {
                    thisOverrides.enabled.Add(false);
                }

                // Sync overrides list with sequence overrides list
                int i;
                for (i = 0; i < sequenceOverrides.Count; i++)
                {
                    // Add new override if we're at the end of this list
                    if (thisOverrides.list.Count <= i)
                    {
                        InsertOverride(i);
                        continue;
                    }

                    // Continue if overrides match
                    if (sequenceOverrides[i].ValueEquals(thisOverrides.list[i]))
                    {
                        continue;
                    }

                    // Look through the rest of this array for an override that matches the current sequence override
                    bool foundMatch = false;
                    for (int j = i + 1; j < thisOverrides.list.Count; j++)
                    {
                        // If overrides match, swap the matching one with the non-matching one at i
                        if (sequenceOverrides[i].ValueEquals(thisOverrides.list[i]))
                        {
                            OverrideField lowerIndexField = thisOverrides.list[i];
                            bool lowerIndexEnabled = thisOverrides.enabled[i];
                            thisOverrides.list[i] = thisOverrides.list[j];
                            thisOverrides.enabled[i] = thisOverrides.enabled[j];
                            thisOverrides.list[j] = lowerIndexField;
                            thisOverrides.enabled[j] = lowerIndexEnabled;

                            foundMatch = true;

                            break;
                        }
                    }

                    // If we didn't find any matching overrides, insert a new override
                    if (!foundMatch)
                    {
                        InsertOverride(i);
                    }
                }

                void InsertOverride(int i)
                {
                    OverrideField newOverride = (OverrideField)Activator.CreateInstance(sequenceOverrides[i].GetType());
                    newOverride.Index = sequenceOverrides[i].Index;
                    //if (newOverride.IsGeneric)
                    //{
                    //    var o = (OverrideField<>)newOverride;
                    //}
                    thisOverrides.list.Insert(i, newOverride);
                    thisOverrides.enabled.Insert(i, false);
                }

                // Remove excess overrides from this list
                for (; i < thisOverrides.list.Count; i++)
                {
                    int removeAt = thisOverrides.list.Count - 1;
                    thisOverrides.list.RemoveAt(removeAt);

                    if (removeAt < thisOverrides.enabled.Count)
                    {
                        thisOverrides.enabled.RemoveAt(removeAt);
                    }
                }
            }

            void DrawOverrides(int listIndex, SerializedProperty overridesProp, OverrideFieldList thisOverrides, ref int selectedNodeIndex, ref byte[] selectedNodeID)
            {
                if (thisOverrides.list.Count == 0)
                {
                    return;
                }

                SerializedProperty overridesEnabledProp = overridesProp.FindPropertyRelative(nameof(thisOverrides.enabled));
                overridesProp = overridesProp.FindPropertyRelative(nameof(thisOverrides.list));

                Sequence.SequenceEditor.CreateStyles();

                int count = overridesProp.arraySize;
                for (int i = 0; i < count; i++)
                {
                    SerializedProperty current = overridesProp.GetArrayElementAtIndex(i);
                    SerializedProperty next = current.Copy();
                    next.NextVisible(false);

                    int startDepth = current.depth;
                    const int DepthIndentation = 16;

                    Node node = null;
                    int nodeIndex = Sequence.SequenceEditor.GetNodeContainingOverride(thisOverrides.list[i], thisOverrides.sequence.Nodes);
                    if (nodeIndex > -1)
                    {
                        node = thisOverrides.sequence.Nodes[nodeIndex];
                    }

                    GUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(overridesEnabledProp.GetArrayElementAtIndex(i), overrideLabel, GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    bool overrideEnabled = thisOverrides.enabled[i];

                    //Event.current.mousePosition
                    if (GUILayout.RepeatButton(node != null ? $"{node.Name} - Override {thisOverrides.list[i].Index} ({listIndex}/{i})" : $"Override {thisOverrides.list[i].Index} ({listIndex}/{i})", Sequence.SequenceEditor.eventButtonStyle))
                    {
                        EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), Sequence.SequenceEditor.flowColor);

                        selectedNodeIndex = nodeIndex;
                        selectedNodeID = thisOverrides.nodeID;
                    }

                    GUILayout.EndHorizontal();

                    if (!overrideEnabled)
                    {
                        //GUI.enabled = false;
                        GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    }

                    EditorGUI.BeginChangeCheck();
                    while (current.NextVisible(true))
                    {
                        if (current.depth > startDepth + 1)
                        {
                            continue;
                        }

                        if (SerializedProperty.EqualContents(current, next))
                        {
                            break;
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(DepthIndentation * (current.depth - startDepth));
                        EditorGUILayout.PropertyField(current);
                        EditorGUILayout.EndHorizontal();
                    }

                    if (EditorGUI.EndChangeCheck() && !overrideEnabled)
                    {
                        overridesEnabledProp.GetArrayElementAtIndex(i).boolValue = true;
                    }

                    if (!overrideEnabled)
                    {
                        //GUI.enabled = true;
                        GUI.color = Color.white;
                    }

                    //if (node != null && node is INestedSequenceNode nestedSequence && nestedSequence.GetSequence() != null)
                    //{
                    //    Sequence sequence = nestedSequence.GetSequence();
                    //    bool foundMatchingList = false;

                    //    for (int j = 1; j < player.overrides.Count; j++)
                    //    {
                    //        if (player.overrides[j].sequence == sequence)
                    //        {
                    //            SyncOverrides(sequence.Overrides, player.overrides[j]);
                    //            usedOverrideLists.Add(j);
                    //            foundMatchingList = true;
                    //            break;
                    //        }
                    //    }

                    //    if (!foundMatchingList)
                    //    {
                    //        player.overrides.Add(new OverrideFieldList(sequence));
                    //        SyncOverrides(sequence.Overrides, player.overrides[player.overrides.Count - 1]);
                    //        usedOverrideLists.Add(player.overrides.Count - 1);
                    //    }
                    //}

                    EditorGUILayout.Space(2);
                }
            }
        }
#endif
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityDropdown.Editor;
using static Pigeon.Util.EditorUtil;
#endif

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Represents a sequence of events that can play over time. Play this sequence with a <see cref="SequencePlayer"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Pigeon/Sequence")]
    public class Sequence : ScriptableObject
    {
        [SerializeReference] List<Node> nodes;

        /// <summary>
        /// This sequence's nodes
        /// </summary>
        internal List<Node> Nodes => nodes;

        [SerializeReference] List<OverrideField> overrides = new List<OverrideField>();

        /// <summary>
        /// Override fields from this sequence's nodes
        /// </summary>
        internal List<OverrideField> Overrides => overrides;

        [SerializeField] [Tooltip("If true, all nodes will be run at the same time")] bool parallel;

        /// <summary>
        /// If true, all nodes will be run at the same time
        /// </summary>
        internal bool Parallel => parallel;

        internal static IEnumerator RunParallelCoroutine(IEnumerator coroutine, RefInt onComplete)
        {
            yield return coroutine;
            onComplete.value++;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Sequence))]
        internal class SequenceEditor : Editor
        {
            const int VerticalLinePadding = 3;
            const int HeaderHorizontalPadding = 5;
            const int IconHorizontalPadding = 1;

            static readonly Dictionary<Type, Texture2D> nodeIcons = new Dictionary<Type, Texture2D>();
            static readonly List<Type> nodeTypes = new List<Type>();
            static readonly List<Type> overrideDataTypes = new List<Type>();
            static readonly List<MonoScript> monoScripts = new List<MonoScript>();

            public static readonly Color flowColor = new Color(0.7607843f, 0.3411765f, 1f, 0.25f);
            public static readonly Color iconColor = new Color(0.7607843f, 0.3411765f, 1f, 1f);
            //internal static readonly Color flowColor = new Color(0.6431373f, 0.6431373f, 0.6431373f, 0.25f);

            internal static GUIStyle eventButtonStyle;
            static GUIStyle foldoutButtonStyle;
            static GUIStyle iconButtonStyle;
            static GUIStyle rightAlignStyle;

            static GUIContent foldoutIconExpanded;
            static GUIContent foldoutIconCollapsed;
            static GUIContent addIcon;
            static GUIContent moreIcon;
            static GUIContent loopIcon;

            static Texture2D defaultIcon;

            static GenericMenuWrapper nodeMenu;

            class GenericMenuWrapper
            {
                public DropdownMenu<MenuItem> menu;
                public Node selectedNode;
                public SerializedProperty selectedNodeProp;
                public SequenceEditor editor;

                public void ShowAsContext(SequenceEditor editor)
                {
                    this.editor = editor;
                    menu.ResetSearch();
                    menu.ShowAsContext();
                }
            }

            class MenuItem
            {
                public string name;
                public Type type;

                public MenuItem(string name, Type type)
                {
                    this.name = name;
                    this.type = type;
                }
            }

            SerializedProperty selectedPropertyForEditing;

            void Awake()
            {
                if (nodeMenu != null)
                {
                    return;
                }

                // Create node menu
                nodeMenu = new GenericMenuWrapper();
                List<DropdownItem<MenuItem>> menuItems = new List<DropdownItem<MenuItem>>(32);

                // Find all MonoScripts of type Node
                nodeIcons.Clear();
                string[] scriptPaths = AssetDatabase.FindAssets("t:MonoScript"/*, new string[] { "Assets/Nodes" }*/);

                for (int i = 0; i < scriptPaths.Length; i++)
                {
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(scriptPaths[i]));
                    Type scriptClass = script.GetClass();

                    if (scriptClass != null)
                    {
                        if (scriptClass.IsSubclassOf(typeof(Node)))
                        {
                            monoScripts.Add(script);
                        }
                        else if (scriptClass == typeof(Node))
                        {
                            defaultIcon = EditorGUIUtility.GetIconForObject(script);
                        }
                    }
                }

                // Add each MonoScript class that has a SerializedNodeAttribute
                nodeTypes.Clear();
                for (int i = 0; i < monoScripts.Count; i++)
                {
                    Type type = monoScripts[i].GetClass();
                    if (type != null && type.IsDefined(typeof(SerializedNodeAttribute), true))
                    {
                        var attribute = type.GetCustomAttribute<SerializedNodeAttribute>();
                        nodeTypes.Add(type);

                        Texture2D icon = EditorGUIUtility.GetIconForObject(monoScripts[i]);
                        string name = System.IO.Path.GetFileName(attribute.Path);
                        menuItems.Add(new DropdownItem<MenuItem>(new MenuItem(name, type), "Add Node/" + attribute.Path, icon ?? defaultIcon, name)/*, false, AddNode, type*/);

                        if (icon != null)
                        {
                            nodeIcons.Add(type, icon);
                        }
                    }
                }

                // Order by path depth
                menuItems.Sort((x, y) => y.Path.Count((c) => c == '/').CompareTo(x.Path.Count((c) => c == '/')));

                FindTypesWithOverrideData();

                menuItems.Insert(0, new DropdownItem<MenuItem>(new MenuItem("Remove", null), "Remove")/*, false, RemoveSelectedNode*/);

                menuItems.Insert(1, new DropdownItem<MenuItem>(new MenuItem("Move Up", null), "Move Up")/*, false, MoveSelectedNodeUp*/);
                menuItems.Insert(2, new DropdownItem<MenuItem>(new MenuItem("Move Down", null), "Move Down")/*, false, MoveSelectedNodeUp*/);

                menuItems.Add(new DropdownItem<MenuItem>(new MenuItem("Select Script", null), "Select Script")/*, false, SelectScript*/);

                nodeMenu.menu = new DropdownMenu<MenuItem>(menuItems, OnMenuItemSelected);
            }

            void OnEnable()
            {
                serializedObject.Update();
                FixMissingOverrideData();
                serializedObject.ApplyModifiedProperties();
            }

            static void OnMenuItemSelected(MenuItem item)
            {
                if (item.type == null)
                {
                    switch (item.name)
                    {
                        case "Remove":
                            nodeMenu.editor.RemoveSelectedNode();
                            break;
                        case "Move Up":
                            nodeMenu.editor.MoveSelectedNodeUp();
                            break;
                        case "Move Down":
                            nodeMenu.editor.MoveSelectedNodeDown();
                            break;
                        case "Select Script":
                            nodeMenu.editor.SelectScript();
                            break;
                    }
                }
                else
                {
                    nodeMenu.editor.AddNode(item.type);
                }
            }

            void AddSelectScriptOption(bool enabled)
            {
                ///if (enabled)
                ///{
                ///    nodeMenu.menu.AddItem(new GUIContent("Move Up"), false, MoveSelectedNodeUp);
                ///    nodeMenu.menu.AddItem(new GUIContent("Move Down"), false, MoveSelectedNodeUp);
                ///    nodeMenu.menu.AddItem(new GUIContent("Select Script"), false, SelectScript);
                ///}
                ///else
                ///{
                ///    nodeMenu.menu.AddDisabledItem(new GUIContent("Move Up"), false);
                ///    nodeMenu.menu.AddDisabledItem(new GUIContent("Move Down"), false);
                ///    nodeMenu.menu.AddDisabledItem(new GUIContent("Select Script"), false);
                ///}
            }

            void SelectScript()
            {
                if (nodeMenu.selectedNode != null)
                {
                    Type type = nodeMenu.selectedNode.GetType();
                    MonoScript asset = monoScripts.Find((script) => script.GetClass() == type);
                    if (asset)
                    {
                        Selection.activeObject = asset;
                    }
                }
            }

            // Callback for nodeMenu to add a new node
            void AddNode(Type type)
            {
                Undo.RecordObject(target, "Add Node");

                // Create node
                //Type type = (Type)context;
                Node node = (Node)Activator.CreateInstance(type);
                node.Name = System.IO.Path.GetFileName(type.GetCustomAttribute<SerializedNodeAttribute>().Path);

                Sequence sequencer = (Sequence)target;
                if (sequencer.Nodes == null)
                {
                    sequencer.nodes = new List<Node>();
                }

                // Insert after selected node or add to empty list
                int index;
                if (nodeMenu.selectedNode != null)
                {
                    index = sequencer.nodes.IndexOf(nodeMenu.selectedNode) + 1;
                    sequencer.nodes.Insert(index, node);
                }
                else
                {
                    index = sequencer.nodes.Count;
                    sequencer.nodes.Add(node);
                }

                //node.OnCreate(sequencer, index);

                ///serializedObject.ApplyModifiedProperties();
                //serializedObject.Update();

                StoreOverrideDataFromNode(node);

                nodeMenu.selectedNode = null;
                nodeMenu.selectedNodeProp = null;
                selectedPropertyForEditing = null;
                GUI.FocusControl(null);
            }

            void RemoveSelectedNode()
            {
                if (nodeMenu.selectedNode != null)
                {
                    if (EditorUtility.DisplayDialog("Remove Node", "Remove Node?", "Remove", "Cancel", DialogOptOutDecisionType.ForThisSession, "removenode"))
                    {
                        Undo.RecordObject(target, "Remove Node");

                        // Remove node and corresponding data
                        RemoveOverrideDataFromRemovedNode(nodeMenu.selectedNodeProp);
                        ((Sequence)target).Nodes.Remove(nodeMenu.selectedNode);

                        ///serializedObject.ApplyModifiedProperties();
                        //serializedObject.Update();
                        GUI.FocusControl(null);
                    }

                    nodeMenu.selectedNode = null;
                    nodeMenu.selectedNodeProp = null;
                    selectedPropertyForEditing = null;
                }
            }

            void SwapNodes(Node a, Node b)
            {
                var sequence = (Sequence)target;

                int indexA = sequence.nodes.IndexOf(a);
                int indexB = sequence.nodes.IndexOf(b);

                sequence.nodes[indexA] = b;
                sequence.nodes[indexB] = a;
                GUI.FocusControl(null);
            }

            void MoveSelectedNodeUp()
            {
                var sequence = (Sequence)target;

                if (nodeMenu.selectedNode != null)
                {
                    int index = sequence.nodes.IndexOf(nodeMenu.selectedNode);
                    if (index == 0)
                    {
                        return;
                    }

                    Undo.RecordObject(target, "Swap Nodes");

                    SwapNodes(nodeMenu.selectedNode, sequence.nodes[index - 1]);

                    ///serializedObject.ApplyModifiedProperties();

                    nodeMenu.selectedNode = null;
                    nodeMenu.selectedNodeProp = null;
                    selectedPropertyForEditing = null;
                }
            }

            void MoveSelectedNodeDown()
            {
                var sequence = (Sequence)target;

                if (nodeMenu.selectedNode != null)
                {
                    int index = sequence.nodes.IndexOf(nodeMenu.selectedNode);
                    if (index == sequence.nodes.Count - 1)
                    {
                        return;
                    }

                    Undo.RecordObject(target, "Swap Nodes");

                    SwapNodes(nodeMenu.selectedNode, sequence.nodes[index + 1]);

                    ///serializedObject.ApplyModifiedProperties();

                    nodeMenu.selectedNode = null;
                    nodeMenu.selectedNodeProp = null;
                    selectedPropertyForEditing = null;
                }
            }

            internal static void CreateStyles()
            {
                if (eventButtonStyle == null)
                {
                    eventButtonStyle = new GUIStyle(EditorStyles.iconButton)
                    {
                        fixedWidth = 0f,
                        fixedHeight = 0f,
                        padding = new RectOffset(HeaderHorizontalPadding, HeaderHorizontalPadding, VerticalLinePadding, VerticalLinePadding),
                        fontStyle = FontStyle.Bold,
                    };
                    foldoutButtonStyle = new GUIStyle(eventButtonStyle)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(IconHorizontalPadding, IconHorizontalPadding, VerticalLinePadding, VerticalLinePadding)
                    };
                    iconButtonStyle = new GUIStyle(foldoutButtonStyle)
                    {
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                    rightAlignStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleRight
                    };

                    foldoutIconExpanded = EditorGUIUtility.IconContent("IN foldout act on", "Toggle Visiblity");
                    foldoutIconCollapsed = EditorGUIUtility.IconContent("d_IN_foldout_act", "Toggle Visiblity");
                    addIcon = EditorGUIUtility.IconContent("Toolbar Plus", "Add Node");
                    moreIcon = EditorGUIUtility.IconContent("align_vertically_center", "Collapsed Nodes Below");
                    loopIcon = EditorGUIUtility.IconContent("preAudioLoopOff", "Loop Count");
                }
            }

            public override void OnInspectorGUI()
            {
                // Create button styles
                CreateStyles();

                serializedObject.Update();

                //EditorGUILayout.PropertyField(serializedObject.FindProperty("overrides"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("parallel"));
                GUILayout.Space(20f);

                // Draw header
                GUILayout.BeginHorizontal();
                GUILayout.Space(-9.5f);
                GUILayout.Label("NODES", eventButtonStyle);
                GUILayout.EndHorizontal();

                // Draw all nodes
                DrawNodes();

                // Draw add button
                if (GUILayout.Button(addIcon, iconButtonStyle, GUILayout.Width(EditorGUIUtility.singleLineHeight + VerticalLinePadding), GUILayout.Height(EditorGUIUtility.singleLineHeight + VerticalLinePadding)))
                {
                    nodeMenu.selectedNode = null;
                    nodeMenu.selectedNodeProp = null;
                    AddSelectScriptOption(false);
                    nodeMenu.ShowAsContext(this);
                }

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            void FindTypesWithOverrideData()
            {
                overrideDataTypes.Clear();

                // Find each node type that has a field of type OverrideField
                for (int i = 0; i < nodeTypes.Count; i++)
                {
                    var fields = nodeTypes[i].GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (field.FieldType.IsSubclassOf(typeof(OverrideField)))
                        {
                            overrideDataTypes.Add(nodeTypes[i]);
                            break;
                        }
                    }
                }
            }

            void StoreOverrideDataFromNode(Node node)
            {
                var sequencer = (Sequence)target;
                var list = sequencer.overrides;

                Type type = node.GetType();
                if (overrideDataTypes.Contains(type))
                {
                    // Check each field of this node's type. If that field is of type OverrideField, add it to the overrides list.
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (field.FieldType.IsSubclassOf(typeof(OverrideField)))
                        {
                            OverrideField data = (OverrideField)field.GetValue(node);

                            if (data == null)
                            {
                                data = (OverrideField)Activator.CreateInstance(field.FieldType);
                                field.SetValue(node, data);
                            }

                            if (!list.Contains(data))
                            {
                                int highestIndex = -1;
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (list[i].Index > highestIndex)
                                    {
                                        highestIndex = list[i].Index;
                                    }
                                }

                                data.Index = highestIndex + 1;
                                list.Add(data);

                                // Sift data down until it's sorted
                                //int listIndex = list.Count - 1;
                                //while (listIndex > 0 && data.Index < list[listIndex].Index)
                                //{
                                //    list[listIndex] = list[data.Index - 1];
                                //    list[listIndex - 1] = data;
                                //    listIndex--;
                                //}
                            }

                            ///serializedObject.ApplyModifiedProperties();
                            //serializedObject.Update();
                        }
                    }
                }
            }

            static readonly List<bool> foundOverrides = new List<bool>();

            void FixMissingOverrideData()
            {
                var sequencer = (Sequence)target;
                var list = sequencer.overrides;

                for (int i = 0; i < foundOverrides.Count; i++)
                {
                    foundOverrides[i] = false;
                }
                for (int i = foundOverrides.Count; i < list.Count; i++)
                {
                    foundOverrides.Add(false);
                }

                if (sequencer.nodes == null || sequencer.nodes.Count == 0)
                {
                    goto RemoveExtraOverrides;
                }

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == null)
                    {
                        list.RemoveAt(i);
                        foundOverrides.RemoveAt(i);
                    }
                }

                for (int k = 0; k < sequencer.nodes.Count; k++)
                {
                    Node node = sequencer.nodes[k];
                    Type type = node.GetType();

                    if (overrideDataTypes.Contains(type))
                    {
                        // Check each field of this node's type. If that field is of type OverrideField, add it to the overrides list.
                        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (var field in fields)
                        {
                            if (field.FieldType.IsSubclassOf(typeof(OverrideField)))
                            {
                                OverrideField data = (OverrideField)field.GetValue(node);

                                if (data == null)
                                {
                                    data = (OverrideField)Activator.CreateInstance(field.FieldType);
                                    field.SetValue(node, data);
                                }

                                int index = 0;
                                bool foundMatch = false;
                                for (; index < sequencer.overrides.Count; index++)
                                {
                                    if (sequencer.overrides[index].Index == data.Index && sequencer.overrides[index].GetType() == data.GetType())
                                    {
                                        foundMatch = true;
                                        foundOverrides[index] = true;
                                        break;
                                    }
                                }

                                if (!foundMatch)
                                {
                                    int highestIndex = -1;
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        if (list[i].Index > highestIndex)
                                        {
                                            highestIndex = list[i].Index;
                                        }
                                    }

                                    data.Index = highestIndex + 1;
                                    list.Add(data);
                                    if (foundOverrides.Count < list.Count)
                                    {
                                        foundOverrides.Add(true);
                                    }
                                    else
                                    {
                                        foundOverrides[list.Count] = true;
                                    }

                                    // Sift data down until it's sorted
                                    //int listIndex = list.Count - 1;
                                    //while (listIndex > 0 && data.Index < list[listIndex].Index)
                                    //{
                                    //    list[listIndex] = list[data.Index - 1];
                                    //    list[listIndex - 1] = data;
                                    //    listIndex--;
                                    //}
                                }

                                ///serializedObject.ApplyModifiedProperties();
                                //serializedObject.Update();
                            }
                        }
                    }
                }

                RemoveExtraOverrides:

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (!foundOverrides[i])
                    {
                        list.RemoveAt(i);
                    }
                }
            }

            void RemoveOverrideDataFromRemovedNode(SerializedProperty node)
            {
                Sequence sequencer = (Sequence)target;
                object property = GetTargetObjectOfProperty(node);

                var list = ((Sequence)target).overrides;

                Type type = property.GetType();
                if (overrideDataTypes.Contains(type))
                {
                    // Check each field of this node's type. If that field is of type OverrideField, remove it from the overrides list.
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (field.FieldType.IsSubclassOf(typeof(OverrideField)))
                        {
                            OverrideField data = (OverrideField)field.GetValue(property);

                            int index = 0;
                            bool foundMatch = false;
                            for (; index < sequencer.overrides.Count; index++)
                            {
                                if (sequencer.overrides[index].Index == data.Index)
                                {
                                    foundMatch = true;
                                    break;
                                }
                            }

                            if (!foundMatch)
                            {
                                index = -1;
                            }

                            if (index > -1)
                            {
                                list.RemoveAt(index);

                                // Increment Index of all OverrideFields above this one
                                //for (; index < list.Count; index++)
                                //{
                                //    list[index].Index--;
                                //}

                                ///serializedObject.ApplyModifiedProperties();
                                //serializedObject.Update();
                            }
                        }
                    }
                }
            }

            public static int GetNodeContainingOverride(OverrideField fieldToMatch, List<Node> nodes)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    Node node = nodes[i];
                    Type type = node.GetType();

                    if (overrideDataTypes.Contains(type))
                    {
                        // Check each field of this node's type. If that field is of type OverrideField and that field matches the passed fieldToMatch, return the index of this node
                        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (var field in fields)
                        {
                            if (field.FieldType.IsSubclassOf(typeof(OverrideField)))
                            {
                                if (fieldToMatch.ValueEquals((OverrideField)field.GetValue(node)))
                                {
                                    return i;
                                }
                            }
                        }
                    }
                }

                return -1;
            }

            const int DepthIndentation = 16;

            void DrawSelectedNode()
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                // Navigate to name field
                SerializedProperty current = selectedPropertyForEditing.Copy();
                current.NextVisible(true);

                // Draw node name field
                //Node node = (Node)GetTargetObjectOfProperty(selectedPropertyForEditing);
                //nodeIcons.TryGetValue(node.GetType(), out Texture2D icon);

                // Draw icon and editable name field
                EditorGUILayout.BeginHorizontal();
                //GUILayout.Label(icon, GUILayout.Width(DepthIndentation), GUILayout.Height(DepthIndentation));
                GUILayout.Space(DepthIndentation * 2.5f);
                //urrent.stringValue = EditorGUILayout.TextField(current.stringValue/*, eventButtonStyle*/);
                EditorGUILayout.PropertyField(current);

                Rect startRect = GUILayoutUtility.GetLastRect();

                EditorGUILayout.EndHorizontal();

                // Store the first property after this node's child properties end
                SerializedProperty next = selectedPropertyForEditing.Copy();
                next.NextVisible(false);

                int startDepth = current.depth - 1;

                //Draw node properties
                while (current.NextVisible(true))
                {
                    //SkipContinue:
                    // Break if we reach the next element at our start depth
                    if (current.depth > startDepth + 1)
                    {
                        continue;
                    }

                    if (SerializedProperty.EqualContents(current, next))
                    {
                        break;
                    }

                    // Break if we reach another node
                    //object value = GetTargetObjectOfProperty(current);
                    //if (value is Node)
                    //{
                    //    break;
                    //}
                    //// Skip if we reach a List<Node>
                    //else if (value is List<Node>)
                    //{
                    //    current.NextVisible(false);
                    //    goto SkipContinue;
                    //}

                    // Draw child property
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(DepthIndentation * 1.5f + DepthIndentation * (current.depth - startDepth));
                    EditorGUILayout.PropertyField(current);
                    EditorGUILayout.EndHorizontal();
                }

                Rect endRect = GUILayoutUtility.GetLastRect();
                startRect.height = endRect.y + endRect.height - startRect.y;
                startRect.width = 2f;
                startRect.x -= DepthIndentation * 0.9f;

                EditorGUI.DrawRect(startRect, new Color(0f, 0f, 0f, 0.3f));

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            void DrawNodes(int depth = 0)
            {
                SerializedProperty nodes = serializedObject.FindProperty("nodes");

                int count = nodes.arraySize;
                for (int i = 0; i < count; i++)
                {
                    // Get current element in nodes list
                    SerializedProperty element = nodes.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(depth * DepthIndentation);

                    // Get the actual node object from our SerializedProperty
                    Node node = GetTargetObjectOfProperty(element) as Node;
                    if (node == null)
                    {
                        nodes.DeleteArrayElementAtIndex(i);
                        continue;
                    }

                    nodeIcons.TryGetValue(node.GetType(), out Texture2D icon);
                    icon ??= defaultIcon;

                    Color color = iconColor;
                    if (depth > 0)
                    {
                        color.a *= 0.5f;
                    }
                    GUI.color = color;
                    GUILayout.Label(icon, GUILayout.Width(DepthIndentation), GUILayout.Height(DepthIndentation));
                    GUI.color = Color.white;

                    if (depth > 0)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }

                    // Draw node name
                    if (GUILayout.Button(new GUIContent(element.displayName, node.Description), eventButtonStyle))
                    {
                        // Show context menu if right-clicked, otherwise open property for editing
                        if (Event.current.button == 1 || (Event.current.button == 0 && Event.current.control))
                        {
                            nodeMenu.selectedNode = node;
                            nodeMenu.selectedNodeProp = element.Copy();
                            AddSelectScriptOption(true);
                            nodeMenu.ShowAsContext(this);
                        }
                        else if (depth == 0)
                        {
                            selectedPropertyForEditing = SerializedProperty.EqualContents(element, selectedPropertyForEditing) ? null : element.Copy();
                            GUI.FocusControl(null);
                        }
                    }

                    GUI.color = Color.white;

                    // Draw preview value on the right side
                    string previewValue = node.GetPreviewValue();
                    if (previewValue != null)
                    {
                        Rect lastRect = GUILayoutUtility.GetLastRect();
                        lastRect.width -= lastRect.height;

                        GUI.Label(lastRect, previewValue, rightAlignStyle);
                    }

                    EditorGUILayout.EndHorizontal();

                    if (SerializedProperty.EqualContents(element, selectedPropertyForEditing))
                    {
                        DrawSelectedNode();
                    }

                    if (node is INestedSequenceNode nestedSequence && nestedSequence.GetSequence())
                    {
                        SequenceEditor nestedEditor = (SequenceEditor)CreateEditor(nestedSequence.GetSequence());
                        ///nestedEditor.serializedObject.Update();
                        nestedEditor.DrawNodes(depth + 1);
                        ///nestedEditor.serializedObject.ApplyModifiedProperties();
                        DestroyImmediate(nestedEditor);
                    }
                }
            }

            public void DrawNodesInOtherEditor(int selectedIndex, byte[] selectedNodeID, byte[] thisNodeID, int depth)
            {
                CreateStyles();

                SerializedProperty nodes = serializedObject.FindProperty("nodes");

                int count = nodes.arraySize;
                for (int i = 0; i < count; i++)
                {
                    // Get current element in nodes list
                    SerializedProperty element = nodes.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Space(depth * DepthIndentation);

                    // Get the actual node object from our SerializedProperty
                    Node node = GetTargetObjectOfProperty(element) as Node;
                    nodeIcons.TryGetValue(node.GetType(), out Texture2D icon);
                    icon ??= defaultIcon;

                    GUI.color = iconColor;
                    GUILayout.Label(icon, GUILayout.Width(DepthIndentation), GUILayout.Height(DepthIndentation));
                    GUI.color = Color.white;

                    // Draw node name
                    GUILayout.Button(new GUIContent(element.displayName, node.Description), eventButtonStyle);

                    if (selectedIndex == i && INestedSequenceNode.AreIDsEqual(selectedNodeID, thisNodeID))
                    {
                        EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), flowColor);
                    }

                    // Draw preview value on the right side
                    string previewValue = node.GetPreviewValue();
                    if (previewValue != null)
                    {
                        Rect lastRect = GUILayoutUtility.GetLastRect();
                        lastRect.width -= lastRect.height;

                        GUI.Label(lastRect, previewValue, rightAlignStyle);
                    }

                    EditorGUILayout.EndHorizontal();

                    if (node is INestedSequenceNode nestedSequence && nestedSequence.GetSequence())
                    {
                        //CreateCachedEditor(nestedSequence.GetSequence(), null, ref cachedEditor);
                        //((SequenceEditor)cachedEditor).DrawNodesInOtherEditor(selectedIndex, selectedNodeID, nestedSequence.GetID(), depth + 1, ref cachedEditor);
                        //cachedEditor = this;

                        //Editor newEditor = null;
                        //CreateCachedEditor(nestedSequence.GetSequence(), null, ref newEditor);
                        Editor nestedEditor = CreateEditor(nestedSequence.GetSequence());
                        ((SequenceEditor)nestedEditor).DrawNodesInOtherEditor(selectedIndex, selectedNodeID, nestedSequence.GetID(), depth + 1);
                        DestroyImmediate(nestedEditor);
                    }
                }
            }
        }
#endif
    }
}
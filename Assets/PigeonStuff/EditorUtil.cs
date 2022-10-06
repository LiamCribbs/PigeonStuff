#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace Pigeon.Util
{
    public static class EditorUtil
    {
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        [MenuItem("Edit/Duplicate And Make Child #%D")]
        public static void DuplicateAndMakeChild()
        {
            var selection = Selection.activeGameObject;
            if (PrefabUtility.IsPartOfPrefabAsset(selection))
            {
                return;
            }

            GameObject clone;
            if (PrefabUtility.IsAnyPrefabInstanceRoot(selection))
            {
                clone = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(selection), selection.scene);
                PrefabUtility.SetPropertyModifications(clone, PrefabUtility.GetPropertyModifications(selection));
                clone.transform.parent = selection.transform;
            }
            else
            {
                clone = GameObject.Instantiate(selection, selection.transform);
            }

            clone.name = selection.name;
            GameObjectUtility.EnsureUniqueNameForSibling(clone);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;
            clone.transform.localScale = Vector3.one;

            Selection.activeGameObject = clone;

            Undo.RegisterCreatedObjectUndo(clone, "Duplicate And Make Child");
        }
    }
}
#endif
using UnityEngine;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

namespace Pigeon
{
    /// <summary>
    /// Serializable array with weighted elements for fetching random items with different chances
    /// </summary>
    [System.Serializable]
    public class WeightedArray<T>
    {
        [ContextMenuItem("Print Chances", nameof(ToString))]
        [SerializeField] Node[] items;

        int weightSum;

        public int Length => items.Length;

        public T this[int index]
        {
            get
            {
                if (weightSum == 0)
                {
                    CalculateWeightSum();
                }

                return items[index].value;
            }
            set => items[index].value = value;
        }

        /// <summary>
        /// Get a random element
        /// </summary>
        public T Get()
        {
            if (weightSum == 0)
            {
                CalculateWeightSum();
            }

            int spawnValue = Random.Range(0, weightSum);

            for (byte i = 0; i < items.Length; i++)
            {
                int weight = items[i].weight;
                if (spawnValue < weight)
                {
                    return items[i].value;
                }

                spawnValue -= weight;
            }

            return default;
        }

        /// <summary>
        /// Get a 'random' element using a 0-1 random value as a kind of seed
        /// </summary>
        public T Get(float randomValue)
        {
            if (weightSum == 0)
            {
                CalculateWeightSum();
            }

            int spawnValue = (int)(randomValue * weightSum);

            for (byte i = 0; i < items.Length; i++)
            {
                int weight = items[i].weight;
                if (spawnValue < weight)
                {
                    return items[i].value;
                }

                spawnValue -= weight;
            }

            return default;
        }

        public int IndexOf(T value)
        {
            if (weightSum == 0)
            {
                CalculateWeightSum();
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].value.Equals(value))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Set an item's weight
        /// </summary>
        public void SetWeight(int index, int weight)
        {
            items[index].weight = weight;
        }

        void CalculateWeightSum()
        {
            for (int i = 0; i < items.Length; i++)
            {
                weightSum += items[i].weight;
            }

            if (Application.isPlaying)
            {
                System.Array.Sort(items);
            }
        }

        /// <summary>
        /// Print all items' chances of being picked to the console
        /// </summary>
        public override string ToString()
        {
            CalculateWeightSum();

            string s = "Weighted Array Chances";
            for (int i = 0; i < items.Length; i++)
            {
                s += "\n" + (items[i].value is Object obj ? obj.name : items[i].value.ToString()) + ": " + (items[i].weight / (float)weightSum * 100f).ToString("0.0") + "%";
            }

            return s;
        }

        [System.Serializable]
        class Node : System.IComparable<Node>
        {
            public T value;
            public int weight;

            public int CompareTo(Node other)
            {
                return weight.CompareTo(other.weight);
            }
        }

        //#if UNITY_EDITOR
        //    [CustomPropertyDrawer(typeof(WeightedArray<>))]
        //    public class WeightedArrayDrawer : PropertyDrawer
        //    {
        //        // Draw the property inside the given rect
        //        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //        {
        //            // Using BeginProperty / EndProperty on the parent property means that
        //            // prefab override logic works on the entire property.
        //            EditorGUI.BeginProperty(position, label, property);

        //            // Draw label
        //            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        //            // Don't make child fields be indented
        //            var indent = EditorGUI.indentLevel;
        //            EditorGUI.indentLevel = 0;

        //            // Calculate rects
        //            //var amountRect = new Rect(position.x, position.y, position.width * 0.7f, position.height);
        //            //var unitRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, position.height);

        //            // Draw fields - passs GUIContent.none to each so they are drawn without labels
        //            EditorGUI.PropertyField(position, property.FindPropertyRelative("items"), GUIContent.none);

        //            // Set indent back to what it was
        //            EditorGUI.indentLevel = indent;

        //            EditorGUI.EndProperty();
        //        }
        //    }

        //    [CustomPropertyDrawer(typeof(WeightedArray<ChunkObjectData>.Node))]
        //    public class WeightedArrayNodeDrawer : PropertyDrawer
        //    {
        //        // Draw the property inside the given rect
        //        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //        {
        //            // Using BeginProperty / EndProperty on the parent property means that
        //            // prefab override logic works on the entire property.
        //            EditorGUI.BeginProperty(position, label, property);

        //            // Draw label
        //            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        //            // Don't make child fields be indented
        //            var indent = EditorGUI.indentLevel;
        //            EditorGUI.indentLevel = 0;

        //            // Calculate rects
        //            var amountRect = new Rect(position.x, position.y, position.width * 0.7f, position.height);
        //            var unitRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, position.height);

        //            // Draw fields - passs GUIContent.none to each so they are drawn without labels
        //            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("value"), GUIContent.none);
        //            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("weight"), GUIContent.none);

        //            // Set indent back to what it was
        //            EditorGUI.indentLevel = indent;

        //            EditorGUI.EndProperty();
        //        }
        //    }
        //#endif
    }
}
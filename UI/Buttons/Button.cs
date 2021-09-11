using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pigeon
{
    public class Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [HideInInspector] public bool hovering;
        [HideInInspector] public bool clicking;

        public bool ignoreEvents;
        public Button eventButton;

        public float hoverSpeed = 1f;

        public EaseFunctions.EvaluateMode easingFunctionHover;
        [SerializeField] protected EaseFunctions.EaseMode easingModeHover;
        public EaseFunctions.EaseMode EasingModeHover
        {
            get => easingModeHover;
            set
            {
                easingModeHover = value;
                easingFunctionHover = EaseFunctions.SetEaseMode(value);
            }
        }

        [Space(10f)]
        public float clickSpeed = 1f;

        public EaseFunctions.EvaluateMode easingFunctionClick;
        [SerializeField] protected EaseFunctions.EaseMode easingModeClick;
        public EaseFunctions.EaseMode EasingModeClick
        {
            get => easingModeClick;
            set
            {
                easingModeClick = value;
                easingFunctionClick = EaseFunctions.SetEaseMode(value);
            }
        }

        public IEnumerator hoverCoroutine;
        public IEnumerator clickCoroutine;

        [HideInInspector] public UnityEvent OnHoverEnter;
        [HideInInspector] public UnityEvent OnHoverExit;
        [HideInInspector] public UnityEvent OnClickDown;
        [HideInInspector] public UnityEvent OnClickUp;

#if UNITY_EDITOR
        [HideInInspector] public bool showEvents;
#endif

        protected virtual void OnValidate()
        {
            EasingModeHover = easingModeHover;
            EasingModeClick = easingModeClick;
        }

        public virtual void Awake()
        {
            if (eventButton)
            {
                eventButton.OnHoverEnter.AddListener(() => OnPointerEnter(null));
                eventButton.OnHoverExit.AddListener(() => OnPointerExit(null));
                eventButton.OnClickDown.AddListener(() => OnPointerDown(null));
                eventButton.OnClickUp.AddListener(() => OnPointerUp(null));
            }

            if (easingFunctionHover == null)
            {
                OnValidate();
            }
        }

        protected new virtual void StartCoroutine(IEnumerator routine)
        {
            if (gameObject.activeInHierarchy)
            {
                base.StartCoroutine(routine);
            }
        }

        public virtual void SetHover(bool hover)
        {
            if (hover && !hovering)
            {
                OnPointerEnter(null);
            }
            else if (!hover && hovering)
            {
                OnPointerExit(null);
            }
        }

        public virtual void SetClick(bool click)
        {
            if (click && !clicking)
            {
                OnPointerDown(null);
            }
            else if (!click && clicking)
            {
                OnPointerUp(null);
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }
            
            hovering = true;

            OnHoverEnter?.Invoke();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            if (clicking)
            {
                clicking = false;
                OnClickUp?.Invoke();
            }

            hovering = false;

            OnHoverExit?.Invoke();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }
            
            clicking = true;

            OnClickDown?.Invoke();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            clicking = false;
            
            OnClickUp?.Invoke();
        }

        public virtual void SetToNull(IEnumerator coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Button), true)]
    [CanEditMultipleObjects]
    public class ButtonEditor : Editor
    {
        SerializedProperty OnHoverEnter;
        SerializedProperty OnHoverExit;
        SerializedProperty OnClickDown;
        SerializedProperty OnClickUp;

        SerializedProperty showEvents;

        void OnEnable()
        {
            OnHoverEnter = serializedObject.FindProperty("OnHoverEnter");
            OnHoverExit = serializedObject.FindProperty("OnHoverExit");
            OnClickDown = serializedObject.FindProperty("OnClickDown");
            OnClickUp = serializedObject.FindProperty("OnClickUp");

            showEvents = serializedObject.FindProperty("showEvents");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            showEvents.boolValue = EditorGUILayout.Foldout(showEvents.boolValue, "Events", true);

            if (showEvents.boolValue)
            {
                EditorGUILayout.PropertyField(OnHoverEnter);
                EditorGUILayout.PropertyField(OnHoverExit);
                EditorGUILayout.PropertyField(OnClickDown);
                EditorGUILayout.PropertyField(OnClickUp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Pigeon.UI
{
    public class UICursor : MonoBehaviour
    {
        public enum State
        {
            None, Hover, Click
        }

        public State state;

        public float sensitivity = 0.7f;

        public static GameObject prefab;
        public static EventSystem eventSystem;

        public new RectTransform transform;

        public Button rectButton;
        public GraphicRaycaster raycaster;

        bool foundGraphic;
        IEventSystemHandler selectedGraphic;

        static readonly List<RaycastResult> results = new List<RaycastResult>();

        public InputActionProperty moveInput;
        public InputActionProperty clickInput;
        public InputActionProperty backInput;

        [ContextMenu("PrintSelectedGraphic")]
        public void PrintSelectedGraphic()
        {
            print(selectedGraphic);
            print((selectedGraphic as Component).name);
        }

        public void Setup(Canvas canvas, bool useFullScreen = false)
        {
            //if ((Object)selectedGraphic)
            //{
            //    if (state == State.Click)
            //    {
            //        OnClickUp(new InputAction.CallbackContext());
            //    }

            //    ((IPointerExitHandler)selectedGraphic).OnPointerExit(null);
            //    rectButton.SetHover(false);
            //}

            //if (rectButton.clicking)
            //{
            //    rectButton.SetClick(false);
            //}
            //if (rectButton.hovering)
            //{
            //    rectButton.SetHover(false);
            //}

            selectedGraphic = null;
            state = State.None;

            raycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        void Awake()
        {
            if (!eventSystem)
            {
                eventSystem = FindObjectOfType<EventSystem>();
            }

            moveInput.action.Enable();
            clickInput.action.Enable();
            backInput.action.Enable();
        }

        void OnDestroy()
        {
            moveInput.action.Disable();
            clickInput.action.Disable();
            backInput.action.Disable();
        }

        private void OnEnable()
        {
            clickInput.action.started += OnClickDown;
            clickInput.action.canceled += OnClickUp;
        }

        void OnDisable()
        {
            clickInput.action.started -= OnClickDown;
            clickInput.action.canceled -= OnClickUp;

            if (selectedGraphic != null)
            {
                if (state == State.Click)
                {
                    state = State.None;
                    ((IPointerUpHandler)selectedGraphic).OnPointerUp(null);
                    rectButton.SetClick(false);
                    ((IPointerExitHandler)selectedGraphic).OnPointerExit(null);
                    rectButton.SetHover(false);
                }
                else if (state == State.Hover)
                {
                    state = State.None;
                    ((IPointerExitHandler)selectedGraphic).OnPointerExit(null);
                    rectButton.SetHover(false);
                }

                selectedGraphic = null;
                foundGraphic = false;
            }
        }

        void OnClickDown(InputAction.CallbackContext context)
        {
            if (foundGraphic)
            {
                state = State.Click;
                ((IPointerDownHandler)selectedGraphic).OnPointerDown(null);
                rectButton.SetClick(true);
            }
        }

        void OnClickUp(InputAction.CallbackContext context)
        {
            if (state == State.Click)
            {
                // Getting an error when clicking the respawn button for some reason so we gotta do this
                if (!(Object)selectedGraphic)
                {
                    selectedGraphic = null;
                    rectButton.SetClick(false);
                    rectButton.SetHover(false);
                    return;
                }

                if (foundGraphic)
                {
                    state = State.Hover;
                    ((IPointerUpHandler)selectedGraphic).OnPointerUp(null);
                    rectButton.SetClick(false);
                }
                else
                {
                    state = State.None;
                    ((IPointerUpHandler)selectedGraphic).OnPointerUp(null);
                    rectButton.SetClick(false);
                    rectButton.SetHover(false);
                }
            }
        }

        void Update()
        {
            if (!(Object)selectedGraphic)
            {
                selectedGraphic = null;
            }

            Vector2 position = transform.anchoredPosition + moveInput.action.ReadValue<Vector2>() * sensitivity;

            if (position.x < ScreenLeft)
            {
                position.x = ScreenLeft;
            }
            else if (position.x > ScreenRight)
            {
                position.x = ScreenRight;
            }
            if (position.y < ScreenBottom)
            {
                position.y = ScreenBottom;
            }
            else if (position.y > ScreenTop)
            {
                position.y = ScreenTop;
            }

            transform.anchoredPosition = position;

            //PointerEventData eventData = new PointerEventData(eventSystem)
            //{
            //    position = transform.position
            //};

            //List<RaycastResult> results = new List<RaycastResult>();
            //raycaster.Raycast(eventData, results);

            //bool foundGraphic = false;

            //for (int i = 0; i < results.Count; i++)
            //{
            //    if (results[i].gameObject.TryGetComponent(out IEventSystemHandler button))
            //    {
            //        if (button != selectedGraphic)
            //        {
            //            if (button != null)
            //            {
            //                ((IPointerEnterHandler)button).OnPointerEnter(null);
            //                rectButton.SetHover(true);
            //            }
            //            if (selectedGraphic != null)
            //            {
            //                ((IPointerExitHandler)selectedGraphic).OnPointerExit(null);
            //                rectButton.SetHover(false);
            //                if (rectButton.clicking)
            //                {
            //                    rectButton.SetClick(false);
            //                }
            //            }

            //            selectedGraphic = button;
            //        }

            //        foundGraphic = true;
            //        break;
            //    }
            //}

            //if (!foundGraphic)
            //{
            //    if (selectedGraphic != null)
            //    {
            //        ((IPointerExitHandler)selectedGraphic).OnPointerExit(null);
            //        rectButton.SetHover(false);
            //        if (rectButton.clicking)
            //        {
            //            rectButton.SetClick(false);
            //        }

            //        selectedGraphic = null;
            //    }
            //}



            if (!(Object)selectedGraphic)
            {
                selectedGraphic = null;
            }

            foundGraphic = false;
            bool selectedGraphicIsDifferent = false;
            IEventSystemHandler newGraphic = null;

            PointerEventData eventData = new PointerEventData(null)
            {
                position = transform.position
            };

            //List<RaycastResult> results = new List<RaycastResult>();
            results.Clear();
            raycaster.Raycast(eventData, results);

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.TryGetComponent(out IEventSystemHandler button))
                {
                    selectedGraphicIsDifferent = button != selectedGraphic;
                    if (selectedGraphicIsDifferent)
                    {
                        newGraphic = button;
                    }

                    foundGraphic = true;
                    break;
                }
            }

            if (foundGraphic)
            {
                if (selectedGraphicIsDifferent && state != State.Click)
                {
                    if (selectedGraphic != null)
                    {
                        ((IPointerExitHandler)selectedGraphic).OnPointerExit(null);
                    }

                    selectedGraphic = newGraphic;

                    state = State.Hover;
                    ((IPointerEnterHandler)selectedGraphic).OnPointerEnter(null);
                    rectButton.SetHover(true);
                }
            }
            else if (state != State.Click && selectedGraphic != null)
            {
                state = State.None;
                ((IPointerExitHandler)selectedGraphic).OnPointerExit(null);
                selectedGraphic = null;
                rectButton.SetHover(false);
            }
        }

        float ScreenLeft
        {
            get
            {
                return 0f;
            }
        }

        float ScreenRight
        {
            get
            {
                return Screen.width;
            }
        }

        float ScreenBottom
        {
            get
            {
                return 0f;
            }
        }

        float ScreenTop
        {
            get
            {
                return Screen.height;
            }
        }
    }
}
#endif
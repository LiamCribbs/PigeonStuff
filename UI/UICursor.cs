using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class UICursor : MonoBehaviour
{
    public enum State
    {
        None, Hover, Click
    }

    public State state;

    public float fallbackSensitivity = 0.7f;

    public static GameObject prefab;
    public static EventSystem eventSystem;

    public new RectTransform transform;

    public Pigeon.Button rectButton;
    public GraphicRaycaster raycaster;
    public PlayerInteract playerInteract;
    bool useFullScreen;

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
        this.useFullScreen = useFullScreen;
    }

    void Awake()
    {
        //transform = GetComponent<RectTransform>();

        if (!eventSystem)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }

        if (playerInteract)
        {
            playerInteract.input.controls.Menu.Move.Enable();
            playerInteract.input.controls.Menu.Select.Enable();
            playerInteract.input.controls.Menu.Cancel.Enable();
        }
        else
        {
            moveInput.action.Enable();
            clickInput.action.Enable();
            backInput.action.Enable();
        }
    }

    void OnDestroy()
    {
        if (playerInteract)
        {
            playerInteract.input.controls.Menu.Move.Disable();
            playerInteract.input.controls.Menu.Select.Disable();
            playerInteract.input.controls.Menu.Cancel.Disable();
        }
        else
        {
            moveInput.action.Disable();
            clickInput.action.Disable();
            backInput.action.Disable();
        }
    }

    private void OnEnable()
    {
        if (playerInteract)
        {
            //playerInteract.input.controls.Menu.Move.Enable();
            //playerInteract.input.controls.Menu.Select.Enable();
            //playerInteract.input.controls.Menu.Cancel.Enable();

            playerInteract.input.controls.Menu.Select.started += OnClickDown;
            playerInteract.input.controls.Menu.Select.canceled += OnClickUp;
        }
        else
        {
            //moveInput.action.Enable();
            //clickInput.action.Enable();
            //backInput.action.Enable();

            clickInput.action.started += OnClickDown;
            clickInput.action.canceled += OnClickUp;
        }
    }

    void OnDisable()
    {
        if (playerInteract)
        {
            //playerInteract.input.controls.Menu.Move.Disable();
            //playerInteract.input.controls.Menu.Select.Disable();
            //playerInteract.input.controls.Menu.Cancel.Disable();

            playerInteract.input.controls.Menu.Select.started -= OnClickDown;
            playerInteract.input.controls.Menu.Select.canceled -= OnClickUp;
        }
        else
        {
            //moveInput.action.Disable();
            //clickInput.action.Disable();
            //backInput.action.Disable();

            clickInput.action.started -= OnClickDown;
            clickInput.action.canceled -= OnClickUp;
        }

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

        Vector2 position = transform.anchoredPosition + (playerInteract ? playerInteract.input.controls.Menu.Move.ReadValue<Vector2>() * playerInteract.cursorSensitivity : moveInput.action.ReadValue<Vector2>() * fallbackSensitivity);

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
            return useFullScreen ? 0f : playerInteract.ScreenLeft;
        }
    }

    float ScreenRight
    {
        get
        {
            return useFullScreen ? PlayerInteract.ScreenWidth : playerInteract.ScreenRight;
        }
    }

    float ScreenBottom
    {
        get
        {
            return useFullScreen ? 0f : playerInteract.ScreenBottom;
        }
    }

    float ScreenTop
    {
        get
        {
            return useFullScreen ? PlayerInteract.ScreenHeight : playerInteract.ScreenTop;
        }
    }
}
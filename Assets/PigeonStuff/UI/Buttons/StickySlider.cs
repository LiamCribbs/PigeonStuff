using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StickySlider : MonoBehaviour
{
    [SerializeField] RectTransform handle;
    [SerializeField] RectTransform container;
    [SerializeField] float containerPadding = 8f;

    [Space(20)]
    [SerializeField] bool sticky = true;
    [SerializeField] float stickyResetSpeed = 2.5f;
    [SerializeField] float deadZoneRadius = 15f;
    [SerializeField] Vector2 stickyTarget;

    [Space(20)]
    public UnityEvent<Vector2> onValueChanged;
    public System.Action OnDeactivate { get; set; }

    bool draggingMoveController;

    public RectTransform Handle => handle;
    public RectTransform Container => container;
    public Vector2 StickyTarget => stickyTarget;
    public float ContainerPadding => containerPadding;

    public Vector2 MaxHandlePos
    {
        get
        {
            Vector2 size = container.sizeDelta * 0.5f - handle.sizeDelta * 0.5f;
            size.x -= containerPadding;
            size.y -= containerPadding;
            return size;
        }
    }

    void Reset()
    {
        container = transform as RectTransform;
    }

    void Awake()
    {
        enabled = false;
    }

    public void StartDrag()
    {
        draggingMoveController = true;
        enabled = true;
    }

    public void EndDrag()
    {
        draggingMoveController = false;

        if (!sticky)
        {
            enabled = false;
            OnDeactivate?.Invoke();
        }
    }

    void InvokeOnValueChanged(Vector2 valueNormalized)
    {
        if (onValueChanged.GetPersistentEventCount() > 0)
        {
            if (float.IsNaN(valueNormalized.x))
            {
                valueNormalized.x = 0f;
            }
            if (float.IsNaN(valueNormalized.y))
            {
                valueNormalized.y = 0f;
            }

            onValueChanged.Invoke(valueNormalized);
        }
    }

    void Update()
    {
        if (draggingMoveController)
        {
            Vector2 size = container.rect.size * 0.5f;
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);
            size -= handle.sizeDelta * 0.5f;
            size.x -= containerPadding;
            size.y -= containerPadding;

#if ENABLE_INPUT_SYSTEM
            Vector2 cursorPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#else
            Vector2 cursorPosition = Input.mousePosition;
#endif

            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, cursorPosition, null, out Vector2 position);
            position.x = Mathf.Clamp(position.x, -size.x, size.x);
            position.y = Mathf.Clamp(position.y, -size.y, size.y);

            Vector2 prevValue = handle.anchoredPosition / size;
            handle.anchoredPosition = position;
            Vector2 newValue = position / size;

            if (newValue != prevValue)
            {
                InvokeOnValueChanged(newValue);
            }
        }
        else if (sticky && (handle.anchoredPosition - stickyTarget).sqrMagnitude > deadZoneRadius)
        {
            Vector2 size = container.rect.size * 0.5f;
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);
            size -= handle.sizeDelta * 0.5f;
            size.x -= containerPadding;
            size.y -= containerPadding;

            Vector2 prevValue = handle.anchoredPosition / size;
            handle.anchoredPosition = Vector2.Lerp(handle.anchoredPosition, stickyTarget, stickyResetSpeed * Time.deltaTime);
            Vector2 newValue = handle.anchoredPosition / size;

            if (newValue != prevValue)
            {
                InvokeOnValueChanged(newValue);
            }
        }
        else
        {
            //if (onValueChanged.GetPersistentEventCount() > 0)
            //{
            //    Vector2 size = container.rect.size * 0.5f;
            //    size.x = Mathf.Abs(size.x);
            //    size.y = Mathf.Abs(size.y);
            //    size -= handle.sizeDelta * 0.5f;
            //    size.x -= containerPadding;
            //    size.y -= containerPadding;

            //    Vector2 v = handle.anchoredPosition / size;
            //    if (float.IsNaN(v.x))
            //    {
            //        v.x = 0f;
            //    }
            //    if (float.IsNaN(v.y))
            //    {
            //        v.y = 0f;
            //    }

            //    onValueChanged.Invoke(v);
            //}

            enabled = false;
            OnDeactivate?.Invoke();
        }
    }
}
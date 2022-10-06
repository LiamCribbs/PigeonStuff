using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Pigeon.UI;
using TMPro;

public class HoverInfoDisplay : MonoBehaviour
{
    [SerializeField] RectSizeButton rectButton;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Vector2 padding = new Vector2(40f, 10f);

    [SerializeField] Canvas canvas;

    /// <summary>
    /// Get the canvas
    /// </summary>
    Canvas GetCanvas() => canvas;

    /// <summary>
    /// Get the size of the canvas
    /// </summary>
    Vector2 GetBounds() => ((RectTransform)canvas.transform).sizeDelta;

    void Awake()
    {
        HoverInfo.OnHoverEnter += OnHoverEnter;
        HoverInfo.OnHoverExit += OnHoverExit;
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        HoverInfo.OnHoverEnter -= OnHoverEnter;
        HoverInfo.OnHoverExit += OnHoverExit;
    }

    void OnDisable()
    {
        if (rectButton.hovering)
        {
            rectButton.hovering = false;
            rectButton.rectTransform.sizeDelta = Vector2.zero;
            gameObject.SetActive(false);
            enabled = false;
        }
    }

    void Update()
    {
        if (rectButton.hovering || rectButton.rectTransform.sizeDelta != rectButton.GetDefaultSize())
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#else
            Vector2 mousePos = Input.mousePosition;
#endif

            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)GetCanvas().transform, mousePos, GetCanvas().worldCamera, out Vector2 cursorPosition);
            cursorPosition.y += rectButton.hoverSize.y;

            var bounds = GetBounds();
            const float OutlinePadding = 8f;

            if (cursorPosition.y + rectButton.hoverSize.y * 0.5f + OutlinePadding > bounds.y * 0.5f)
            {
                cursorPosition.y -= cursorPosition.y + rectButton.hoverSize.y * 0.5f + OutlinePadding - bounds.y * 0.5f;
            }

            if (cursorPosition.x + rectButton.hoverSize.x * 0.5f + OutlinePadding > bounds.x * 0.5f)
            {
                cursorPosition.x -= cursorPosition.x + rectButton.hoverSize.x * 0.5f + OutlinePadding - bounds.x * 0.5f;
            }
            else if (cursorPosition.x - rectButton.hoverSize.x * 0.5f - OutlinePadding < -bounds.x * 0.5f)
            {
                cursorPosition.x += -bounds.x * 0.5f - cursorPosition.x + rectButton.hoverSize.x * 0.5f + OutlinePadding;
            }

            rectButton.rectTransform.anchoredPosition = cursorPosition;

            if (transform.GetSiblingIndex() < transform.parent.childCount - 1)
            {
                transform.SetAsLastSibling();
            }
        }
    }

    void OnHoverEnter(HoverInfo info)
    {
        //if (Player.Instance != null && Player.Instance.Data.Click.IsPressed)
        //{
        //    return;
        //}

        gameObject.SetActive(true);
        enabled = true;

        text.text = info.GetInfo();

        text.ForceMeshUpdate();

        Vector2 bounds = text.textBounds.size;
        if (bounds.x < 0f)
        {
            bounds = Vector2.zero;
        }

        bounds += padding;

        rectButton.hoverSize = bounds;

        if (!rectButton.hovering)
        {
            rectButton.EasingModeHover = Pigeon.Math.EaseFunctions.EaseMode.EaseOutElastic;
            rectButton.hoverSpeed = 1.5f;
            rectButton.SetHover(true);
        }
    }

    void OnHoverExit(HoverInfo info)
    {
        if (rectButton.hovering)
        {
            rectButton.EasingModeHover = Pigeon.Math.EaseFunctions.EaseMode.EaseOutQuartic;
            rectButton.hoverSpeed = 3f;
            rectButton.SetHover(false);
            enabled = false;
        }
    }

    public void SetInfo(string info)
    {
        gameObject.SetActive(true);
        enabled = true;

        text.text = info;

        text.ForceMeshUpdate();

        Vector2 bounds = text.textBounds.size;
        if (bounds.x < 0f)
        {
            bounds = Vector2.zero;
        }

        bounds += padding;

        rectButton.hoverSize = bounds;

        if (!rectButton.hovering)
        {
            rectButton.EasingModeHover = Pigeon.Math.EaseFunctions.EaseMode.EaseOutElastic;
            rectButton.hoverSpeed = 1.5f;
            rectButton.SetHover(true);
        }
    }

    public void ClearInfo()
    {
        if (rectButton.hovering)
        {
            rectButton.EasingModeHover = Pigeon.Math.EaseFunctions.EaseMode.EaseOutQuartic;
            rectButton.hoverSpeed = 3f;
            rectButton.SetHover(false);
            enabled = false;
        }
    }
}
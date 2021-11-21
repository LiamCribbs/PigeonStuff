# Documentation for the Buttons

## Pigeon.Button
When referencing the buttons in code, make sure you are `using Pigeon`

* Fields
	* `bool hovering`
		* True if the button is in hover mode
	* `bool clicking`
		* True if the button is in click mode
	* `bool ignoreEvents`
		* If true, pointer events will not be detected
	* `Button eventButton`
		* If `eventButton` is not null, this button will use `eventButton`'s pointer events
	* `float hoverSpeed`
		* Speed of the animation when this button is going to hover mode
	* `EaseFunctions.EvaluateMode easingFunctionHover`
		* Delegate for easing into hover mode
	* `float clickSpeed`
		* Speed of the animation when this button is going to click mode
	* `EaseFunctions.EvaluateMode easingFunctionClick`
		* Delegate for easing into click mode
	* `IEnumerator hoverCoroutine`
		* Holds the coroutine for going into hover mode. If this is not null, the button is still easing into hover mode.
	* `IEnumerator clickCoroutine`
		* Holds the coroutine for going into click mode. If this is not null, the button is still easing into click mode.
	* `UnityEvent OnHoverEnter`
		* Event that is fired when the pointer enters this button
	* `UnityEvent OnHoverExit`
		* Event that is fired when the pointer exits this button
	* `UnityEvent OnClickDown`
		* Event that is fired when the pointer clicks this button
	* `UnityEvent OnClickUp`
		* Event that is fired when the pointer stops clicking this button

* Properties
	* `EaseFunctions.EaseMode EasingModeHover`
		* `get` returns the `evaluateMode` enum value
		* `set` sets the `evaluateMode` delegate from an enum value
	* `EaseFunctions.EaseMode EasingModeClick`
		* `get` returns the `evaluateMode` enum value
		* `set` sets the `evaluateMode` delegate from an enum value

* Methods
	* `void SetHover(bool hover)`
		* Sets hover mode to `hover`. This can be called from code, but it's recommended to let Unity's event system call it on its own.
	* `void SetClick(bool click)`
		* Sets click mode to `click`. This can be called from code, but it's recommended to let Unity's event system call it on its own.
	* `void Toggle()`
		* Toggle this button's state between hovering and clicking
	* `void OnPointerEnter(PointerEventData eventData)`
		* This is implemented by `IPointerEnterHandler` so that Unity's event system can interact with the button. Should not be called directly.
	* `void OnPointerExit(PointerEventData eventData)`
		* This is implemented by `IPointerExitHandler` so that Unity's event system can interact with the button. Should not be called directly.
	* `void OnPointerDown(PointerEventData eventData)`
		* This is implemented by `IPointerDownHandler` so that Unity's event system can interact with the button. Should not be called directly.
	* `void OnPointerUp(PointerEventData eventData)`
		* This is implemented by `IPointerUpHandler` so that Unity's event system can interact with the button. Should not be called directly.

## ColorButton
Animates the gameObject's Graphic's color

* Fields
	* `Graphic mainGraphic`
		* This is the graphic that should change color
	* `Color hoverColor`
		* The color to animate to on hover
	* `Color clickColor`
		* The color to animate to on click

* Methods
	* `void SetDefaultColor(Color value)`
		* Use this to manually set the color that the button should animate to when not clicking or hovering

## OutlineThicknessButton
Animates the OutlineGraphic's thickness

* Fields
	* `OutlineGraphic mainGraphic`
		* This is the graphic that should animate
	* `float hoverOutlineThickness`
		* The thickness to animate to on hover
	* `float clickOutlineThickness`
		* The thickness to animate to on click

* Methods
	* `void SetDefaultOutlineThickness(float value)`
		* Use this to manually set the thickness that the button should animate to when not clicking or hovering

## PositionButton
Animates the RectTransform's localPosition

* Fields
	* `RectTransform mainGraphic`
		* This is the object that should move
	* `float hoverPosition`
		* The position to animate to on hover
	* `float clickPosition`
		* The position to animate to on click

* Methods
	* `void SetDefaultPosition(Vector2 value)`
		* Use this to manually set the position that the button should animate to when not clicking or hovering

## RectSizeButton
Animates the RectTransform's sizeDelta

* Fields
	* `RectTransform rectTransform`
		* This is the object that should change size
	* `float hoverSize`
		* The size to animate to on hover
	* `float clickSize`
		* The size to animate to on click
	* `bool relative`
		* Are `hoverSize` and `clickSize` relative to the default size?

* Methods
	* `void SetDefaultSize(Vector2 value)`
		* Use this to manually set the size that the button should animate to when not clicking or hovering

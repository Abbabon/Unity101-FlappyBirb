using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// Single logical "flap" action: Space/Enter/W/Up, left click, or any touch.
public static class FlapInput
{
    public static bool Pressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame
            || kb.enterKey.wasPressedThisFrame
            || kb.wKey.wasPressedThisFrame
            || kb.upArrowKey.wasPressedThisFrame))
            return true;
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;
        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            return true;
        var pad = Gamepad.current;
        if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            return true;
        return false;
#else
        return Input.GetKeyDown(KeyCode.Space)
            || Input.GetMouseButtonDown(0)
            || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
#endif
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

namespace XiHUtil
{
    public partial class InputUtil
    {
        static StandaloneInputModule Input;
        public static void InitInputMoudle(StandaloneInputModule input)
        {
            Input = input;
            //input.inputOverride = this;//其他渠道会重载新的baseinput，所以直接input.input获取重载的input即可
#if UNITY_WX
            InitWxInput();
#endif
        }
        public static bool GetMouseButtonDown(int button)
        {
            return Input.input.GetMouseButtonDown(button);
        }
        public static bool GetMouseButton(int button)
        {
            return Input.input.GetMouseButton(button);
        }
        public static bool GetMouseButtonUp(int button)
        {
            return Input.input.GetMouseButtonUp(button);
        }
        public static int TouchCount
        {
            get { return Input.input.touchCount; }
        }
        public static Touch GetTouch(int index)
        {
            return Input.input.GetTouch(index);
        }
        public static Vector3 GetTouchPos()
        {
#if (UNITY_EDITOR || UNITY_WEBGL)
            return Input.input.mousePosition;
#else
        if (touchCount > 0)
        {
            return GetTouch(0).position;
        }
        return Vector2.zero;
#endif
        }
    }
}


/* 新输入系统，兼容抖音的，但不适合微信
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace XiHUtil
{
    public class InputUtil
    {
        static BaseInputModule Input;
        public static void InitInputMoudle(BaseInputModule input)
        {
            Input = input;
            //input.inputOverride = this;//其他渠道会重载新的baseinput，所以直接input.input获取重载的input即可
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        }
        #region 仅推荐编辑器或纯WebGL使用
        public static bool GetMouseButtonDown(int button)
        {
#if FAIRYGUI_INPUT_SYSTEM
            switch (button)
            {
                case 0:
                    return Mouse.current.leftButton.wasPressedThisFrame;
                case 1:
                    return Mouse.current.rightButton.wasPressedThisFrame;
                case 2:
                    return Mouse.current.middleButton.wasPressedThisFrame;
                default:
                    return Mouse.current.backButton.wasPressedThisFrame;
            }
#else
            return Input.input.GetMouseButtonDown(button);
#endif
        }
        public static bool GetMouseButton(int button)
        {
#if FAIRYGUI_INPUT_SYSTEM
            switch (button)
            {
                case 0:
                    return Mouse.current.leftButton.isPressed;
                case 1:
                    return Mouse.current.rightButton.isPressed;
                case 2:
                    return Mouse.current.middleButton.isPressed;
                default:
                    return Mouse.current.backButton.isPressed;
            }
#else
            return Input.input.GetMouseButton(button);
#endif
        }
        public static bool GetMouseButtonUp(int button)
        {
#if FAIRYGUI_INPUT_SYSTEM
            switch (button) {
                case 0:
                    return Mouse.current.leftButton.wasReleasedThisFrame;
                case 1:
                    return Mouse.current.rightButton.wasReleasedThisFrame;
                case 2:
                    return Mouse.current.middleButton.wasReleasedThisFrame;
                default:
                    return Mouse.current.backButton.wasReleasedThisFrame;
            }
#else
            return Input.input.GetMouseButtonUp(button);
#endif
        }
        public static Vector2 GetMousePos()
        {
#if FAIRYGUI_INPUT_SYSTEM
            return Mouse.current.position.ReadValue();
#else
            return UnityEngine.Input.mousePosition;
#endif
        }
        public static Vector2 MouseScrollDelta()
        {
#if FAIRYGUI_INPUT_SYSTEM
            return Mouse.current.scroll.ReadValue();
#else
            return UnityEngine.Input.mouseScrollDelta;
#endif
        }
        public static int touchCount
        {
            get
            {
                return Input.input.touchCount;
            }
        }
        public static Touch GetTouch(int index)
        {
            return Input.input.GetTouch(index);
        }
        #endregion
        public static int newInputTouchCount
        {
            get
            {
                return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
            }
        }
        public static UnityEngine.InputSystem.EnhancedTouch.Touch GetNewInputTouch(int index)
        {
            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > index) return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[index];
            return default(UnityEngine.InputSystem.EnhancedTouch.Touch);
        }
        public static bool GetTouchScreenDown()
        {
            if (Pointer.current == null) return false;
            return Pointer.current.press.wasPressedThisFrame;
        }
        public static bool GetTouchScreen()
        {
            if (Pointer.current == null) return false;
            return Pointer.current.press.isPressed;
        }
        public static bool GetTouchScreenUp()
        {
            if (Pointer.current == null) return false;
            return Pointer.current.press.wasReleasedThisFrame;
        }
        public static Vector2 GetTouchScreenPostion()
        {
            if (Pointer.current == null) return Vector2.zero;
            return Pointer.current.position.ReadValue();
        }
    }
}
*/
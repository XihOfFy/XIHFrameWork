#if UNITY_WX
using System;
using WeChatWASM;

namespace XiHUtil
{
    public partial class InputUtil
    {
        static Action<OnTouchStartListenerResult> OnTouchStartAct;
        static Action<OnTouchStartListenerResult> OnTouchMoveAct;
        static Action<OnTouchStartListenerResult> OnTouchEndAct;
        static Action<OnTouchStartListenerResult> OnTouchCancelAct;
        static void InitWxInput() {
            WX.OnTouchStart(res => {
                OnTouchStartAct?.Invoke(res);
            });
            WX.OnTouchMove(res => {
                OnTouchMoveAct?.Invoke(res);
            });
            WX.OnTouchEnd(res => {
                OnTouchEndAct?.Invoke(res);
            });
            WX.OnTouchCancel(res => {
                OnTouchCancelAct?.Invoke(res);
            });
        }
        public static void RegistWxInput(Action<OnTouchStartListenerResult> onTouchStartAct, Action<OnTouchStartListenerResult> onTouchMoveAct, Action<OnTouchStartListenerResult> onTouchEndAct, Action<OnTouchStartListenerResult> onTouchCancelAct) {
            OnTouchStartAct = onTouchStartAct;
            OnTouchMoveAct = onTouchMoveAct;
            OnTouchEndAct = onTouchEndAct;
            OnTouchCancelAct = onTouchCancelAct;
        }
        public static void UnRegistWxInput()
        {
            OnTouchStartAct = null;
            OnTouchMoveAct = null;
            OnTouchEndAct = null;
            OnTouchCancelAct = null;
        }
    }
}
#endif
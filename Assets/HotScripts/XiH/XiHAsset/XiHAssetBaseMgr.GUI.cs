using UnityEngine;

namespace XiHAsset
{
    public partial class XiHAssetBaseMgr
    {
        [HideInInspector] public bool isGmShow = false;
        protected Rect gmRect;
        protected float guiH = 64;
        protected GUIStyle gmBtnstyle;
        protected bool gmInited = false;
        protected string gmSpecNum;
        void InitGUI()
        {
            gmInited = true;
            gmBtnstyle = GUI.skin.button;
            gmRect = new Rect(Screen.width / 3 * 2 - 32, Screen.height / 8, Screen.width / 3, Screen.height);
#if UNITY_WEBGL && !UNITY_EDITOR && UNITY_2021_2_5
            //WebGLInput.mobileKeyboardSupport = true;
#endif
        }
        protected void OnGUI()
        {
            if (!isGmShow) return;
            if (!gmInited)
            {
                InitGUI();
            }
            GUILayout.BeginArea(gmRect);
            if (GUILayout.Button("Close GM", gmBtnstyle, GUILayout.Height(guiH), GUILayout.Width(gmRect.width)))
            {
                isGmShow = false;
            }
            guiH = GUILayout.HorizontalSlider(guiH, 32, 128, GUILayout.Height(guiH));
            gmBtnstyle.fontSize = (int)guiH;
            GUILayout.Space(guiH/2);
            GUILayout.BeginVertical();

            DrawGUI();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        protected virtual void DrawGUI()
        {

        }
    }
}

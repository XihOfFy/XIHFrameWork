using UnityEngine;
using XiHUtil;

namespace Hot
{
    public partial class HomeMgr
    {
        protected override void DrawGUI()
        {
            if (GUILayout.Button("Reset To First", gmBtnstyle, GUILayout.Height(guiH), GUILayout.Width(gmRect.width)))
            {
                PlayerPrefsUtil.DeleteAllKey();
                DataSaveAgent.ClearAllData();
                DataSave.Instance.SaveData();
            }
            GUILayout.Space(guiH / 2);
            GUILayout.BeginHorizontal();
#if USE_GM

            gmSpecNum = GUILayout.TextField(gmSpecNum, gmBtnstyle, GUILayout.Height(guiH), GUILayout.Width(gmRect.width / 2));
            if (GUILayout.Button("Arrive Spec Stage", gmBtnstyle, GUILayout.Height(guiH), GUILayout.Width(gmRect.width / 2)))
            {
                int.TryParse(gmSpecNum, out var stageNum);
                DataSave.Instance.stageId = stageNum;
                DataSave.Instance.SaveData();
            }
#else 
            if (GUILayout.Button("+1 Stage", gmBtnstyle, GUILayout.Height(guiH), GUILayout.Width(gmRect.width / 2)))
            {
                DataSave.Instance.stageId += 1;
                DataSave.Instance.SaveData();
            }
            if (GUILayout.Button("+10 Stage", gmBtnstyle, GUILayout.Height(guiH), GUILayout.Width(gmRect.width / 2)))
            {
                DataSave.Instance.stageId += 10;
                DataSave.Instance.SaveData();
            }
#endif
            GUILayout.EndHorizontal();
        }
    }
}

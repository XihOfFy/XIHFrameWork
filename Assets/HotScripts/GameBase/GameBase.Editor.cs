#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Hot
{
    public partial class GameBase
    {
        protected override void UpdateEditor()
        {
        }
    }
    [CustomEditor(typeof(GameBase))]
    public partial class GameBaseEditor:Editor
    {
        GameBase gameMgr;
        private void OnEnable()
        {
            gameMgr = this.target as GameBase;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!Application.isPlaying) return;
        }
    }
}
#endif
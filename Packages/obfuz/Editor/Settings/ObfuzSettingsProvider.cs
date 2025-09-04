using UnityEditor;
using UnityEngine.UIElements;

namespace Obfuz.Settings
{
    public class ObfuzSettingsProvider : SettingsProvider
    {

        private static ObfuzSettingsProvider s_provider;

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (s_provider == null)
            {
                s_provider = new ObfuzSettingsProvider();
                using (var so = new SerializedObject(ObfuzSettings.Instance))
                {
                    s_provider.keywords = GetSearchKeywordsFromSerializedObject(so);
                }
            }
            return s_provider;
        }


        private SerializedObject _serializedObject;
        private SerializedProperty _buildPipelineSettings;

        private SerializedProperty _assemblySettings;
        private SerializedProperty _obfuscationPassSettings;
        private SerializedProperty _secretSettings;
        private SerializedProperty _encryptionVMSettings;

        private SerializedProperty _symbolObfusSettings;
        private SerializedProperty _constEncryptSettings;
        private SerializedProperty _removeConstFieldSettings;
        private SerializedProperty _evalStackObfusSettings;
        private SerializedProperty _fieldEncryptSettings;
        private SerializedProperty _callObfusSettings;
        private SerializedProperty _exprObfusSettings;
        private SerializedProperty _controlFlowObfusSettings;

        private SerializedProperty _garbageCodeGenerationSettings;
        private SerializedProperty _watermarkSettings;

        private SerializedProperty _polymorphicDllSettings;

        public ObfuzSettingsProvider() : base("Project/Obfuz", SettingsScope.Project)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            InitGUI();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            ObfuzSettings.Save();
        }

        private void InitGUI()
        {
            var setting = ObfuzSettings.Instance;
            _serializedObject?.Dispose();
            _serializedObject = new SerializedObject(setting);
            _buildPipelineSettings = _serializedObject.FindProperty("buildPipelineSettings");

            _assemblySettings = _serializedObject.FindProperty("assemblySettings");
            _obfuscationPassSettings = _serializedObject.FindProperty("obfuscationPassSettings");
            _secretSettings = _serializedObject.FindProperty("secretSettings");

            _encryptionVMSettings = _serializedObject.FindProperty("encryptionVMSettings");

            _symbolObfusSettings = _serializedObject.FindProperty("symbolObfusSettings");
            _constEncryptSettings = _serializedObject.FindProperty("constEncryptSettings");
            _removeConstFieldSettings = _serializedObject.FindProperty("removeConstFieldSettings");
            _evalStackObfusSettings = _serializedObject.FindProperty("evalStackObfusSettings");
            _exprObfusSettings = _serializedObject.FindProperty("exprObfusSettings");
            _fieldEncryptSettings = _serializedObject.FindProperty("fieldEncryptSettings");
            _callObfusSettings = _serializedObject.FindProperty("callObfusSettings");
            _controlFlowObfusSettings = _serializedObject.FindProperty("controlFlowObfusSettings");

            _garbageCodeGenerationSettings = _serializedObject.FindProperty("garbageCodeGenerationSettings");
            _watermarkSettings = _serializedObject.FindProperty("watermarkSettings");

            _polymorphicDllSettings = _serializedObject.FindProperty("polymorphicDllSettings");
        }

        public override void OnGUI(string searchContext)
        {
            if (_serializedObject == null || !_serializedObject.targetObject)
            {
                InitGUI();
            }
            _serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_buildPipelineSettings);

            EditorGUILayout.PropertyField(_assemblySettings);
            EditorGUILayout.PropertyField(_obfuscationPassSettings);
            EditorGUILayout.PropertyField(_secretSettings);

            EditorGUILayout.PropertyField(_encryptionVMSettings);

            EditorGUILayout.PropertyField(_symbolObfusSettings);
            EditorGUILayout.PropertyField(_constEncryptSettings);
            EditorGUILayout.PropertyField(_removeConstFieldSettings);
            EditorGUILayout.PropertyField(_evalStackObfusSettings);
            EditorGUILayout.PropertyField(_exprObfusSettings);
            EditorGUILayout.PropertyField(_fieldEncryptSettings);
            EditorGUILayout.PropertyField(_callObfusSettings);
            EditorGUILayout.PropertyField(_controlFlowObfusSettings);

            EditorGUILayout.PropertyField(_garbageCodeGenerationSettings);
            EditorGUILayout.PropertyField(_watermarkSettings);

            EditorGUILayout.PropertyField(_polymorphicDllSettings);

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                ObfuzSettings.Save();
            }
        }
    }
}
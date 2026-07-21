using UnityEditor;

namespace TTSDK.Tool
{
    [InitializeOnLoad]
    public class TTEditorSupportProviderRegister
    {
        static TTEditorSupportProviderRegister()
        {
            if (TTEditorSupportProvider.MiniGame == null)
                TTEditorSupportProvider.RegisterMiniGameSupportProvider(new TTMiniGameSupportProvider());
        }
        
    }
}
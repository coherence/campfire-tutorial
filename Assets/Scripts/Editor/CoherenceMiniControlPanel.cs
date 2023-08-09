using UnityEditor;
using UnityEngine;
using Coherence.Editor;
using Coherence.Editor.ReplicationServer;
using Coherence.Editor.Toolkit;

namespace Editor
{
    public class CoherenceMiniControlPanel : EditorWindow
    {
        [MenuItem("coherence/Mini Control Panel")]
        private static void ShowWindow()
        {
            var window = GetWindow<CoherenceMiniControlPanel>();
            window.titleContent = new GUIContent("coherence");
            window.Show();
        }

        private void OnGUI()
        {
            float buttonHeight = 24f;
            GUIStyle boxStyle = new GUIStyle();
            int pad = 6;
            boxStyle.padding = new RectOffset(pad, pad, pad, 0);
            
            EditorGUILayout.BeginVertical(boxStyle);
            if (GUILayout.Button("Bake", GUILayout.Height(buttonHeight)))
            {
                BakeUtil.Bake();
            }
            EditorGUILayout.Space(0f);
            if (GUILayout.Button("Run World RS", GUILayout.Height(buttonHeight)))
            {
                EditorLauncher.RunWorldsReplicationServerInTerminal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
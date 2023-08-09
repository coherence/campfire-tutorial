using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(SFXPreset)), CanEditMultipleObjects]
    public class SFXPresetEditor : UnityEditor.Editor
    {
        [SerializeField] private AudioSource _previewAudioSource;

        public void OnEnable()
        {
            _previewAudioSource = EditorUtility.CreateGameObjectWithHideFlags("Preview Audio Source: SFX Clip Settings",
                HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        }

        public void OnDisable()
        {
            DestroyImmediate(_previewAudioSource.gameObject);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(30);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            float height = 24f;
            if (GUILayout.Button("Play random", GUILayout.Height(height)))
            {
                ((SFXPreset) target).PlayPreview(_previewAudioSource);
            }
            if (GUILayout.Button("Stop", GUILayout.Height(height)))
            {
                ((SFXPreset) target).Stop(_previewAudioSource);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }
    }
}
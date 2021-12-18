using UnityEngine;
using UnityEditor;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [CustomEditor(typeof(ToggleSwitch))]
    sealed class ToggleSwitchEditor : UdonEditorBase
    {
        static class Styles
        {
            public static GUIContent switchIconLabel = EditorGUIUtility.TrTextContent("Switch Icon");
        }

        SerializedProperty _interactTextProperty;
        SerializedProperty _isOnProperty;
        SerializedProperty _useSyncProperty;
        SerializedProperty _switchAudioProperty;
        SerializedProperty _spriteProperty;
        SerializedProperty _animatorProperty;
        SerializedProperty _audioSourceProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _interactTextProperty = _udonSerializedObject.FindProperty("interactText");
            _isOnProperty = serializedObject.FindProperty("_isOn");
            _useSyncProperty = serializedObject.FindProperty("_useSync");
            _switchAudioProperty = serializedObject.FindProperty("_switchAudio");
            var spriteRenderer = _udonBehaviour.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                _spriteProperty = new SerializedObject(spriteRenderer).FindProperty("m_Sprite");
            }
            _animatorProperty = serializedObject.FindProperty("_animator");
            _audioSourceProperty = serializedObject.FindProperty("_audioSource");
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _interactTextProperty = null;
            _isOnProperty = null;
            _useSyncProperty = null;
            _switchAudioProperty = null;
            _spriteProperty = null;
            _animatorProperty = null;
            _audioSourceProperty = null;
        }

        protected override void DrawInspector()
        {
            EditorGUILayout.PropertyField(_interactTextProperty);
            EditorGUILayout.PropertyField(_isOnProperty);
            EditorGUILayout.PropertyField(_useSyncProperty);
            EditorGUILayout.PropertyField(_switchAudioProperty);
            if (_spriteProperty != null)
            {
                _spriteProperty.serializedObject.Update();
                EditorGUILayout.PropertyField(_spriteProperty, Styles.switchIconLabel);
                _spriteProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        protected override void DrawDeveloperInspector()
        {
            EditorGUILayout.PropertyField(_animatorProperty);
            EditorGUILayout.PropertyField(_audioSourceProperty);
            EditorGUILayout.Separator();
        }
    }
}

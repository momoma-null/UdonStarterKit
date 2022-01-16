using System.IO;
using UnityEngine;
using UnityEditor;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [CustomEditor(typeof(ToggleSwitch))]
    sealed class ToggleSwitchEditor : UdonEditorBase
    {
        static class Styles
        {
            public static GUIContent switchIconText = EditorGUIUtility.TrTextContent("Switch Icon");
            public static GUIContent bgmText = EditorGUIUtility.TrTextContent("BGM");
        }

        interface ISwitch
        {
            void DrawInspector();
        }

        sealed class BGMSwitch : ISwitch
        {
            readonly SerializedObject _audioSourceSerializedObject;
            readonly SerializedProperty m_audioClipProperty;

            public BGMSwitch(AudioSource audioSource)
            {
                _audioSourceSerializedObject = new SerializedObject(audioSource);
                m_audioClipProperty = _audioSourceSerializedObject.FindProperty("m_audioClip");
            }

            public void DrawInspector()
            {
                _audioSourceSerializedObject.Update();
                EditorGUILayout.PropertyField(m_audioClipProperty, Styles.bgmText);
                _audioSourceSerializedObject.ApplyModifiedProperties();
            }
        }

        SerializedProperty _interactTextProperty;
        SerializedProperty _isOnProperty;
        SerializedProperty _useSyncProperty;
        SerializedProperty _switchAudioProperty;
        SerializedProperty _spriteProperty;
        SerializedProperty _animatorProperty;
        SerializedProperty _audioSourceProperty;
        SerializedProperty m_ControllerProperty;
        ISwitch _specialSwitch;

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
            if (_animatorProperty.objectReferenceValue != null)
                m_ControllerProperty = new SerializedObject(_animatorProperty.objectReferenceValue).FindProperty("m_Controller");
            _audioSourceProperty = serializedObject.FindProperty("_audioSource");
            var audioSource = _udonBehaviour.transform.Find("Controller/BGM")?.GetComponent<AudioSource>();
            if (audioSource != null)
                _specialSwitch = new BGMSwitch(audioSource);
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
            m_ControllerProperty = null;
            _audioSourceProperty = null;
            _specialSwitch = null;
        }

        protected override void DrawInspector()
        {
            EditorGUILayout.PropertyField(_interactTextProperty);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_isOnProperty);
            if (EditorGUI.EndChangeCheck())
            {
                m_ControllerProperty.serializedObject.Update();
                if (m_ControllerProperty != null && m_ControllerProperty.objectReferenceValue != null)
                {
                    var path = AssetDatabase.GetAssetPath(m_ControllerProperty.objectReferenceValue);
                    path = Path.Combine(Path.GetDirectoryName(path), _isOnProperty.boolValue ? "ToggleSwitch_On.controller" : "ToggleSwitch_Off.controller");
                    m_ControllerProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
                    m_ControllerProperty.serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.PropertyField(_useSyncProperty);
            EditorGUILayout.PropertyField(_switchAudioProperty);
            if (_spriteProperty != null)
            {
                _spriteProperty.serializedObject.Update();
                EditorGUILayout.PropertyField(_spriteProperty, Styles.switchIconText);
                _spriteProperty.serializedObject.ApplyModifiedProperties();
            }
            _specialSwitch?.DrawInspector();
        }

        protected override void DrawDeveloperInspector()
        {
            EditorGUILayout.PropertyField(_animatorProperty);
            EditorGUILayout.PropertyField(_audioSourceProperty);
            EditorGUILayout.Separator();
        }
    }
}

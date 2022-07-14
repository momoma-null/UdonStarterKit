using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ReorderableList = UnityEditorInternal.ReorderableList;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [CustomEditor(typeof(ToggleSwitch))]
    sealed class ToggleSwitchEditor : UdonEditorBase
    {
        static class Styles
        {
            public static GUIContent switchIconText = EditorGUIUtility.TrTextContent("Switch Icon");
            public static GUIContent bgmOptionText = EditorGUIUtility.TrTextContent("BGM Option");
            public static GUIContent bgmText = EditorGUIUtility.TrTextContent("BGM");
            public static GUIContent volumeText = EditorGUIUtility.TrTextContent("Volume");
        }

        interface ISwitch
        {
            void DrawInspector();
        }

        sealed class BGMSwitch : ISwitch
        {
            readonly SerializedObject audioSourceSerializedObject;
            readonly SerializedObject sliderSerializedObject;
            readonly SerializedProperty m_audioClipProperty;
            readonly SerializedProperty m_MaxValueProperty;

            public BGMSwitch(AudioSource audioSource, Slider slider)
            {
                audioSourceSerializedObject = new SerializedObject(audioSource);
                m_audioClipProperty = audioSourceSerializedObject.FindProperty("m_audioClip");
                sliderSerializedObject = new SerializedObject(slider);
                m_MaxValueProperty = sliderSerializedObject.FindProperty("m_MaxValue");
            }

            public void DrawInspector()
            {
                audioSourceSerializedObject.Update();
                sliderSerializedObject.Update();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Styles.bgmOptionText, EditorStyles.boldLabel);
                using (new EditorGUI.IndentLevelScope(1))
                {
                    EditorGUILayout.PropertyField(m_audioClipProperty, Styles.bgmText);
                    EditorGUILayout.Slider(m_MaxValueProperty, 0f, 1f, Styles.volumeText);
                }
                audioSourceSerializedObject.ApplyModifiedProperties();
                sliderSerializedObject.ApplyModifiedProperties();
            }
        }

        readonly List<SerializedProperty> toggleObjectProperties = new List<SerializedProperty>();

        SerializedProperty _isOnProperty;
        SerializedProperty _useSyncProperty;
        SerializedProperty _switchAudioProperty;
        SerializedProperty _spriteProperty;
        SerializedProperty _toggleAnimatorProperty;
        SerializedProperty _toggleObjectsProperty;
        SerializedProperty _animatorProperty;
        SerializedProperty _audioSourceProperty;
        SerializedProperty m_ControllerProperty;
        ISwitch _specialSwitch;
        ReorderableList _toggleObjectsList;

        protected override void OnEnable()
        {
            base.OnEnable();
            _isOnProperty = serializedObject.FindProperty("_isOn");
            _useSyncProperty = serializedObject.FindProperty("_useSync");
            _switchAudioProperty = serializedObject.FindProperty("_switchAudio");
            var spriteRenderer = _udonSharpBehaviour.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                _spriteProperty = new SerializedObject(spriteRenderer).FindProperty("m_Sprite");
            }
            _toggleAnimatorProperty = serializedObject.FindProperty("_toggleAnimator");
            _toggleObjectsProperty = serializedObject.FindProperty("_toggleObjects");
            _animatorProperty = serializedObject.FindProperty("_animator");
            if (_animatorProperty.objectReferenceValue != null)
                m_ControllerProperty = new SerializedObject(_animatorProperty.objectReferenceValue).FindProperty("m_Controller");
            _audioSourceProperty = serializedObject.FindProperty("_audioSource");
            var audioSource = transform.Find("BGMController/BGM")?.GetComponent<AudioSource>();
            var slider = transform.Find("BGMController/FloatProxy")?.GetComponent<Slider>();
            if (audioSource != null && slider != null)
                _specialSwitch = new BGMSwitch(audioSource, slider);
            _toggleObjectsList = new ReorderableList(serializedObject, _toggleObjectsProperty);
            _toggleObjectsList.drawHeaderCallback = r => EditorGUI.LabelField(r, _toggleObjectsProperty.displayName);
            _toggleObjectsList.drawElementCallback = (r, i, a, f) => EditorGUI.PropertyField(r, _toggleObjectsProperty.GetArrayElementAtIndex(i));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isOnProperty = null;
            _useSyncProperty = null;
            _switchAudioProperty = null;
            _spriteProperty = null;
            _toggleAnimatorProperty = null;
            _toggleObjectsProperty = null;
            _animatorProperty = null;
            m_ControllerProperty = null;
            _audioSourceProperty = null;
            _specialSwitch = null;
            _toggleObjectsList = null;
        }

        protected override void DrawInspector()
        {
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
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_toggleAnimatorProperty);
            _toggleObjectsList.DoLayoutList();
            _specialSwitch?.DrawInspector();
        }

        protected override void DrawDeveloperInspector()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_animatorProperty);
            if (EditorGUI.EndChangeCheck())
                m_ControllerProperty = _animatorProperty.objectReferenceValue != null ? new SerializedObject(_animatorProperty.objectReferenceValue).FindProperty("m_Controller") : null;
            EditorGUILayout.PropertyField(_audioSourceProperty);
        }
    }
}

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
            public static GUIContent toggleObjectsText = EditorGUIUtility.TrTextContent("Toggle Objects");
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
        SerializedProperty _animatorProperty;
        SerializedProperty _audioSourceProperty;
        SerializedProperty m_ControllerProperty;
        SerializedObject toggleSerializedObject;
        SerializedProperty toggleCallsProperty;
        ISwitch _specialSwitch;
        ReorderableList toggleObjectsList;

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
            _animatorProperty = serializedObject.FindProperty("_animator");
            if (_animatorProperty.objectReferenceValue != null)
                m_ControllerProperty = new SerializedObject(_animatorProperty.objectReferenceValue).FindProperty("m_Controller");
            _audioSourceProperty = serializedObject.FindProperty("_audioSource");
            var toggle = transform.Find("Controller/BoolProxy")?.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggleSerializedObject = new SerializedObject(toggle);
                toggleCallsProperty = toggleSerializedObject.FindProperty("onValueChanged.m_PersistentCalls.m_Calls");
                toggleObjectsList = new ReorderableList(toggleObjectProperties, typeof(SerializedProperty));
                toggleObjectsList.onChangedCallback += _ => ReloadToggleObjectProperties();
                toggleObjectsList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, Styles.toggleObjectsText);
                toggleObjectsList.onAddCallback = _ => AddToggleObjectProperty();
                toggleObjectsList.onRemoveCallback = _ => --toggleCallsProperty.arraySize;
                toggleObjectsList.drawElementCallback = DrawToggleObjectProperty;
                ReloadToggleObjectProperties();
            }
            var audioSource = transform.Find("Controller/BGM")?.GetComponent<AudioSource>();
            var slider = transform.Find("Controller/FloatProxy")?.GetComponent<Slider>();
            if (audioSource != null && slider != null)
                _specialSwitch = new BGMSwitch(audioSource, slider);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isOnProperty = null;
            _useSyncProperty = null;
            _switchAudioProperty = null;
            _spriteProperty = null;
            _animatorProperty = null;
            m_ControllerProperty = null;
            _audioSourceProperty = null;
            _specialSwitch = null;
            toggleSerializedObject?.Dispose();
            toggleSerializedObject = null;
            toggleCallsProperty = null;
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
            toggleObjectsList?.DoLayoutList();
            _specialSwitch?.DrawInspector();
        }

        void ReloadToggleObjectProperties()
        {
            toggleObjectProperties.Clear();
            for (var i = 0; i < toggleCallsProperty.arraySize; ++i)
            {
                var element = toggleCallsProperty.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("m_MethodName").stringValue == nameof(GameObject.SetActive))
                {
                    toggleObjectProperties.Add(element.FindPropertyRelative("m_Target"));
                }
            }
        }

        void AddToggleObjectProperty()
        {
            ++toggleCallsProperty.arraySize;
            var element = toggleCallsProperty.GetArrayElementAtIndex(toggleCallsProperty.arraySize - 1);
            element.FindPropertyRelative("m_MethodName").stringValue = nameof(GameObject.SetActive);
        }

        void DrawToggleObjectProperty(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.PropertyField(rect, toggleObjectProperties[index], GUIContent.none);
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

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
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
            void OnSceneGUI();
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

            public void OnSceneGUI() { }
        }

        sealed class MirrorSwitch : ISwitch
        {
            readonly Transform scaleTarget;
            readonly SerializedObject scaleTargetObject;
            readonly SerializedProperty m_LocalPositionProperty;
            readonly SerializedProperty m_LocalScaleProperty;
            readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle() { axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y };

            public MirrorSwitch(Transform scaleTarget)
            {
                this.scaleTarget = scaleTarget;
                this.scaleTargetObject = new SerializedObject(scaleTarget);
                this.m_LocalPositionProperty = scaleTargetObject.FindProperty("m_LocalPosition");
                this.m_LocalScaleProperty = scaleTargetObject.FindProperty("m_LocalScale");
            }

            public void DrawInspector() { }

            public void OnSceneGUI()
            {
                scaleTargetObject.Update();
                var handleMatrix = scaleTarget.parent == null ? Matrix4x4.identity : Matrix4x4.TRS(scaleTarget.parent.position, scaleTarget.parent.rotation, Vector3.one);
                using (new Handles.DrawingScope(handleMatrix))
                {
                    boundsHandle.size = m_LocalScaleProperty.vector3Value;
                    boundsHandle.center = m_LocalPositionProperty.vector3Value;
                    boundsHandle.handleColor = Handles.color;
                    EditorGUI.BeginChangeCheck();
                    boundsHandle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        var scale = boundsHandle.size;
                        scale.z = 1f;
                        m_LocalScaleProperty.vector3Value = scale;
                        m_LocalPositionProperty.vector3Value = boundsHandle.center;
                        scaleTargetObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        SerializedProperty _isOnProperty;
        SerializedProperty _useSyncProperty;
        SerializedProperty _switchAudioProperty;
        SerializedProperty _switchColorProperty;
        SerializedProperty m_SpriteProperty;
        SerializedProperty _toggleAnimatorProperty;
        SerializedProperty _toggleObjectsProperty;
        SerializedProperty _animatorProperty;
        SerializedProperty _audioSourceProperty;
        SerializedProperty _sliderProperty;
        SerializedProperty _rendererProperty;
        ISwitch _specialSwitch;
        ReorderableList _toggleObjectsList;

        protected override void OnEnable()
        {
            base.OnEnable();
            _isOnProperty = serializedObject.FindProperty("_isOn");
            _useSyncProperty = serializedObject.FindProperty("_useSync");
            _switchAudioProperty = serializedObject.FindProperty("_switchAudio");
            _switchColorProperty = serializedObject.FindProperty("_switchColor");
            _toggleAnimatorProperty = serializedObject.FindProperty("_toggleAnimator");
            _toggleObjectsProperty = serializedObject.FindProperty("_toggleObjects");
            _animatorProperty = serializedObject.FindProperty("_animator");
            _audioSourceProperty = serializedObject.FindProperty("_audioSource");
            _sliderProperty = serializedObject.FindProperty("_slider");
            _rendererProperty = serializedObject.FindProperty("_renderer");
            UpdateSpriteProperties();
            var audioSource = transform.Find("BGMController/BGM")?.GetComponent<AudioSource>();
            var slider = transform.Find("BGMController/FloatProxy")?.GetComponent<Slider>();
            var mirrorController = transform.parent?.Find("MirrorController");
            if (audioSource != null && slider != null)
                _specialSwitch = new BGMSwitch(audioSource, slider);
            else if (mirrorController != null)
                _specialSwitch = new MirrorSwitch(mirrorController);
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
            _switchColorProperty = null;
            m_SpriteProperty = null;
            _toggleAnimatorProperty = null;
            _toggleObjectsProperty = null;
            _animatorProperty = null;
            _sliderProperty = null;
            _sliderProperty = null;
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
                if (_animatorProperty.objectReferenceValue is Animator animator)
                {
                    var animatorSO = new SerializedObject(animator);
                    var m_ControllerProperty = animatorSO.FindProperty("m_Controller");
                    var path = AssetDatabase.GetAssetPath(m_ControllerProperty.objectReferenceValue);
                    path = Path.Combine(Path.GetDirectoryName(path), _isOnProperty.boolValue ? "ToggleSwitch_On.controller" : "ToggleSwitch_Off.controller");
                    m_ControllerProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
                    animatorSO.ApplyModifiedProperties();
                }
                if (_sliderProperty.objectReferenceValue is Slider slider)
                {
                    var sliderSO = new SerializedObject(slider);
                    var valueProperty = sliderSO.FindProperty("m_Value");
                    valueProperty.floatValue = _isOnProperty.boolValue ? 1f : 0f;
                    sliderSO.ApplyModifiedProperties();
                    UpdateColor();
                }
            }
            EditorGUILayout.PropertyField(_useSyncProperty);
            EditorGUILayout.PropertyField(_switchAudioProperty);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_switchColorProperty);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateColor();
            }
            if (m_SpriteProperty != null)
            {
                m_SpriteProperty.serializedObject.Update();
                EditorGUILayout.PropertyField(m_SpriteProperty, Styles.switchIconText);
                m_SpriteProperty.serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_toggleAnimatorProperty);
            _toggleObjectsList.DoLayoutList();
            _specialSwitch?.DrawInspector();
        }

        protected override void DrawDeveloperInspector()
        {
            EditorGUILayout.PropertyField(_animatorProperty);
            EditorGUILayout.PropertyField(_audioSourceProperty);
            EditorGUILayout.PropertyField(_sliderProperty);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_rendererProperty);
            if (EditorGUI.EndChangeCheck())
                UpdateSpriteProperties();
        }

        void UpdateSpriteProperties()
        {
            m_SpriteProperty = _rendererProperty.objectReferenceValue is SpriteRenderer spriteRenderer ? new SerializedObject(spriteRenderer).FindProperty("m_Sprite") : null;
        }

        void UpdateColor()
        {
            if (_rendererProperty.objectReferenceValue is SpriteRenderer spriteRenderer)
            {
                Undo.RecordObject(spriteRenderer, "Sprite color");
                if (_udonSharpBehaviour is ToggleSwitch toggleSwitch)
                    toggleSwitch.UpdateColor();
            }
        }

        void OnSceneGUI()
        {
            _specialSwitch?.OnSceneGUI();
        }
    }
}

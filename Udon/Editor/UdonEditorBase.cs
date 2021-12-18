using System;
using UnityEngine;
using UnityEditor;
using VRC.Udon;
using UdonSharp;
using UdonSharpEditor;

namespace MomomaAssets.UdonStarterKit.Udon
{
    abstract class UdonEditorBase : Editor
    {
        static class Styles
        {
            public static readonly GUIContent developerText = EditorGUIUtility.TrTextContent("Developer");
        }

        [Serializable]
        sealed class DummyClass { }

        [SerializeField]
        DummyClass _openDeveloperInspector;

        protected UdonBehaviour _udonBehaviour;
        protected UdonSharpBehaviour _udonSharpBehaviour;
        protected SerializedObject _udonSerializedObject;
        SerializedProperty _openDeveloperInspectorProperty;

        protected virtual void OnEnable()
        {
            _udonSharpBehaviour = target as UdonSharpBehaviour;
            _udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(_udonSharpBehaviour);
            _udonSerializedObject = new SerializedObject(_udonBehaviour);
            _openDeveloperInspectorProperty = new SerializedObject(this).FindProperty(nameof(_openDeveloperInspector));
        }

        protected virtual void OnDisable()
        {
            _udonBehaviour = null;
            _udonSharpBehaviour = null;
            _udonSerializedObject = null;
        }

        public sealed override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawProgramSource(target, true))
                return;
            serializedObject.Update();
            EditorGUILayout.Space();
            DrawInspector();
            EditorGUILayout.Space();
            try
            {
                _openDeveloperInspectorProperty.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_openDeveloperInspectorProperty.isExpanded, Styles.developerText);
                if (_openDeveloperInspectorProperty.isExpanded)
                {
                    DrawDeveloperInspector();
                    UdonSharpGUI.DrawInteractSettings(target);
                    UdonSharpGUI.DrawUtilities(target);
                }
            }
            finally
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            if (serializedObject.hasModifiedProperties)
            {
                Undo.RecordObject(_udonBehaviour, "change UdonBehaviour values");
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected abstract void DrawInspector();
        protected virtual void DrawDeveloperInspector() { }
    }
}

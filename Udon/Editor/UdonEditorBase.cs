using System;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace MomomaAssets.UdonStarterKit.Udon
{
    abstract class UdonEditorBase : Editor
    {
        static class Styles
        {
            public static readonly GUIContent developerOptionText = EditorGUIUtility.TrTextContent(ObjectNames.NicifyVariableName(nameof(developerOption)));
        }

        [Serializable]
        sealed class DummyClass { }

        [SerializeField]
        DummyClass developerOption;

        protected UdonSharpBehaviour _udonSharpBehaviour;
        protected Transform transform;

        SerializedProperty developerOptionProperty;

        protected virtual void OnEnable()
        {
            _udonSharpBehaviour = target as UdonSharpBehaviour;
            developerOptionProperty = new SerializedObject(this).FindProperty(nameof(developerOption));
            transform = _udonSharpBehaviour.transform;
        }

        protected virtual void OnDisable()
        {
            _udonSharpBehaviour = null;
        }

        public sealed override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawProgramSource(target, true))
                return;
            UdonSharpGUI.DrawInteractSettings(target);
            serializedObject.Update();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawInspector();
            }
            developerOptionProperty.isExpanded = EditorGUILayout.Foldout(developerOptionProperty.isExpanded, Styles.developerOptionText, true);
            if (developerOptionProperty.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope(1))
                {
                    DrawDeveloperInspector();
                }
            }
            UdonSharpGUI.DrawUtilities(target);
            serializedObject.ApplyModifiedProperties();
        }

        protected abstract void DrawInspector();
        protected virtual void DrawDeveloperInspector() { }
    }
}

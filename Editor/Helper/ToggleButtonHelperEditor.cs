using UnityEngine;
using UnityEditor;

namespace MomomaAssets.UdonStarterKit.Helper
{
    [CustomEditor(typeof(ToggleButtonHelper))]
    sealed class ToggleButtonHelperEditor : UdonHelperEditorBase
    {
        static class Styles
        {
            public static GUIContent switchIconLabel = EditorGUIUtility.TrTextContent("Switch Icon");
        }

        SerializedProperty _isOnProperty;
        SerializedProperty _useSyncProperty;
        SerializedProperty _spriteProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _isOnProperty = serializedObject.FindProperty("_isOn");
            _useSyncProperty = _udonSharpSerializedObject.FindProperty("_useSync");
            var spriteRenderer = _udonBehaviour.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                _spriteProperty = new SerializedObject(spriteRenderer).FindProperty("m_Sprite");
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isOnProperty = null;
            _useSyncProperty = null;
            _spriteProperty = null;
        }

        protected override void DrawInspector()
        {
            EditorGUILayout.PropertyField(_isOnProperty);
            EditorGUILayout.PropertyField(_useSyncProperty);
            if (_spriteProperty != null)
            {
                _spriteProperty.serializedObject.Update();
                EditorGUILayout.PropertyField(_spriteProperty, Styles.switchIconLabel);
                _spriteProperty.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

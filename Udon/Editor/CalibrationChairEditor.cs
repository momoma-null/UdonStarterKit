using UnityEditor;
using UnityEngine;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [CustomEditor(typeof(CalibrationChair))]
    sealed class CalibrationChairEditor : UdonEditorBase
    {
        static class Styles
        {
            public static GUIContent kneeText = EditorGUIUtility.TrTextContent("Knee Position");
        }

        SerializedProperty _seatPositionProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _seatPositionProperty = serializedObject.FindProperty("_seatPosition");
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _seatPositionProperty = null;
        }

        protected override void DrawInspector() { }

        protected override void DrawDeveloperInspector()
        {
            EditorGUILayout.PropertyField(_seatPositionProperty);
        }

        void OnSceneGUI()
        {
            if (!(_seatPositionProperty.objectReferenceValue is Transform seatTransform))
                return;
            if (Event.current.type == EventType.Repaint)
            {
                var kneesTransform = seatTransform.parent;
                var position = kneesTransform.position;
                Handles.DrawLine(position + kneesTransform.right * 0.2f, position - kneesTransform.right * 0.2f);
            }
        }
    }
}

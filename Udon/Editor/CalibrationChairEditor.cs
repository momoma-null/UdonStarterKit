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
            var kneesTransform = seatTransform.parent;
            EditorGUI.BeginChangeCheck();
            var position = Handles.PositionHandle(kneesTransform.position, kneesTransform.rotation);
            Handles.Label(position, Styles.kneeText);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(kneesTransform, "Change Knees Position");
                kneesTransform.position = position;
            }
            if (Event.current.type == EventType.Repaint)
            {
                Handles.DrawLine(position + kneesTransform.right * 0.2f, position - kneesTransform.right * 0.2f);
            }
        }
    }
}

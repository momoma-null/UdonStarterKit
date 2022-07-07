using UnityEngine;
using UnityEditor;
using UdonSharpEditor;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [CustomEditor(typeof(CalibrationChair))]
    sealed class CalibrationChairEditor : UdonEditorBase
    {
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
            EditorGUILayout.Separator();
        }

        void OnSceneGUI()
        {
            if (!(_seatPositionProperty.objectReferenceValue is Transform seatTransform))
                return;
            var kneesTransform = seatTransform.parent;
            EditorGUI.BeginChangeCheck();
            var position = Handles.PositionHandle(kneesTransform.position, kneesTransform.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(kneesTransform, "Change Knees Position");
                kneesTransform.position = position;
            }
            if (Event.current.type == EventType.Repaint)
            {
                Handles.SphereHandleCap(0, position, Quaternion.identity, 0.01f, EventType.Repaint);
            }
        }
    }
}

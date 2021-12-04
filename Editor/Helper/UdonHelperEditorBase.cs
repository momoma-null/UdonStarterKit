using UnityEditor;
using VRC.Udon;
using UdonSharp;
using UdonSharpEditor;

namespace MomomaAssets.UdonStarterKit.Helper
{
    abstract class UdonHelperEditorBase : Editor
    {
        protected UdonBehaviour _udonBehaviour;
        protected UdonSharpBehaviour _udonSharpBehaviour;
        protected SerializedObject _udonSharpSerializedObject;

        protected virtual void OnEnable()
        {
            _udonBehaviour = serializedObject.FindProperty("_udonBehaviour").objectReferenceValue as UdonBehaviour;
            _udonSharpBehaviour = UdonSharpEditorUtility.FindProxyBehaviour(_udonBehaviour);
            _udonSharpSerializedObject = new SerializedObject(_udonSharpBehaviour);
        }

        protected virtual void OnDisable()
        {
            _udonBehaviour = null;
            _udonSharpSerializedObject?.Dispose();
            _udonSharpSerializedObject = null;
        }

        public sealed override void OnInspectorGUI()
        {
            _udonSharpBehaviour.UpdateProxy();
            _udonSharpSerializedObject.Update();
            serializedObject.Update();
            DrawInspector();
            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
            if (_udonSharpSerializedObject.hasModifiedProperties)
            {
                Undo.RecordObject(_udonBehaviour, "change UdonBehaviour values");
                _udonSharpSerializedObject.ApplyModifiedProperties();
                _udonSharpBehaviour.ApplyProxyModifications();
            }
        }

        protected abstract void DrawInspector();
    }
}

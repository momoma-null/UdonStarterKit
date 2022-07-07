using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [CustomPropertyDrawer(typeof(CustomEventPopupAttribute))]
    public sealed class CustomEventPopupDrawer : PropertyDrawer
    {
        static readonly HashSet<string> baseMethodNames = new HashSet<string>(typeof(UdonSharpBehaviour)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(e => e.GetParameters().Length == 0)
            .Select(e => e.Name));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var targetFieldName = (attribute as CustomEventPopupAttribute).TargetFieldName;
            var targetUdonSharpBehaviour = property.serializedObject.FindProperty(targetFieldName).objectReferenceValue as UdonSharpBehaviour;
            var options = targetUdonSharpBehaviour == null ?
                Array.Empty<GUIContent>() :
                targetUdonSharpBehaviour
                    .GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(e => !baseMethodNames.Contains(e.Name) && e.GetParameters().Length == 0)
                    .Select(e => new GUIContent(e.Name))
                    .ToArray();
            var index = Array.FindIndex(options, o => o.text == property.stringValue);
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                EditorGUI.BeginChangeCheck();
                index = EditorGUI.Popup(position, label, index, options);
                if (EditorGUI.EndChangeCheck())
                {
                    property.stringValue = options[index].text;
                }
            }
        }
    }
}

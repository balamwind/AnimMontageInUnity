using System;
using UnityEditor;
using UnityEngine;

namespace FC_Editor
{
    [CustomPropertyDrawer(typeof(DisableAttribute))]
    public class DisableAttaributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }
    }
}
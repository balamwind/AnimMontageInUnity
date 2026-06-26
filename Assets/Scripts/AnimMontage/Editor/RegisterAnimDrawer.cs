using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;


namespace FC_Editor
{
    [CustomPropertyDrawer(typeof(AnimMontage.RegisterAnim))]
    public class RegisterAnimDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect oneLine = position;
            oneLine.height = EditorGUIUtility.singleLineHeight;

            if (EditorGUI.PropertyField(oneLine, property) == false)
                return;

            oneLine.y += oneLine.height;
            EditorGUI.indentLevel++;

            //steteName
            property.Next(true);
            EditorGUI.PropertyField(oneLine, property);
            oneLine.y += oneLine.height + 2f;

            //clip
            property.Next(false);
            EditorGUI.PropertyField(oneLine, property);
            oneLine.y += oneLine.height + 2f;

            //animEvents
            property.Next(false);
            EditorGUI.PropertyField(oneLine, property, new GUIContent("Event"));
            oneLine.y += EditorGUI.GetPropertyHeight(property) + 2;
            EditorGUI.indentLevel--;

            if (Application.isPlaying == false)
                return;

            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(true);
            {
                //state name Hash
                property.Next(false);
                EditorGUI.PropertyField(oneLine, property, new GUIContent("Linked state hash"));
                oneLine.y += oneLine.height + 2f;

                //run count
                property.Next(false);
                EditorGUI.PropertyField(oneLine, property, new GUIContent("Run count"));
                oneLine.y += oneLine.height + 2f;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded == false)
                return EditorGUI.GetPropertyHeight(property);

            var animEvents = property.FindPropertyRelative("animEvents");
            if (animEvents.isExpanded)
                return Application.isPlaying ?
                    EditorGUI.GetPropertyHeight(property) - 42f:
                    EditorGUI.GetPropertyHeight(property) - 80f;
            else
                return Application.isPlaying ?
                    EditorGUI.GetPropertyHeight(property) - 40f :
                    EditorGUI.GetPropertyHeight(property) - 82f;
        }
    }
}
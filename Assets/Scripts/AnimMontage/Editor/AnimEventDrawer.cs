using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;

namespace FC_Editor
{
    [CustomPropertyDrawer(typeof(AnimMontage.AnimEvent))]
    public class AnimEventDrawer : PropertyDrawer
    {
        static GUIContent notifyLabel = new GUIContent("Notify");


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect oneLine = position;
            oneLine.height = EditorGUIUtility.singleLineHeight;

            //notifyScirpt 
            property.Next(true);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(oneLine, property, notifyLabel);
            bool change = EditorGUI.EndChangeCheck();

            oneLine.y += oneLine.height + 2f;
            var notify = property.Copy();
            notify.Next(false); //notify
            
            //if change notify, is runs
            if (change)
                if (property.objectReferenceValue != null)
                    CreateNotify(property, notify);
                else
                    notify.managedReferenceValue = null;
            
            if (property.objectReferenceValue != null)
            {
                DrawNotify(notify, property, ref oneLine);
                EditorGUI.LabelField(oneLine, "", GUI.skin.horizontalSlider);
                oneLine.y += oneLine.height + 2f;
            }
            
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            property.isExpanded = true;
            property.Next(true);    //mono

            if (property.objectReferenceValue != null)
            {
                property.Next(false);   //notify
                property.isExpanded = true;

                float notifyHeight = EditorGUI.GetPropertyHeight(property);

                property.Next(true);    //isAnimNotify
                if (property.boolValue == false)
                    notifyHeight -= EditorGUIUtility.singleLineHeight + 2f;

                return notifyHeight;
            }
            else
                return EditorGUI.GetPropertyHeight(property);
        }

        bool CanCasting(Type type, Type castingType)
        {
            if (type == null || type.BaseType == null)
                return false;

            if (type == castingType)
                return true;
            else
                return CanCasting(type.BaseType, castingType);
        }

        void DrawNotify(SerializedProperty notify, SerializedProperty mono, ref Rect pos)
        {
            int inElementDepth = notify.depth;

            notify.Next(true);  //isAnimNotify
            if (notify.boolValue)
            {
                //start Frame
                notify.Next(false);
                EditorGUI.PropertyField(pos, notify);
            }
            else
            {
                notify.Next(false);
                var start = notify.Copy();  //startFrame
                notify.Next(false);
                var end = notify.Copy();    //endFrame
                Vector2Int frame = new Vector2Int(start.intValue, end.intValue);

                frame = EditorGUI.Vector2IntField(pos, "Frame", frame);
                start.intValue = frame.x;
                end.intValue = frame.y;
            }
            
            pos.y += pos.height + 2f;

            EditorGUI.indentLevel++;
            {
                while (notify.NextVisible(false) && notify.depth > inElementDepth)
                {
                    pos.height = EditorGUI.GetPropertyHeight(notify);
                    EditorGUI.PropertyField(pos, notify);
                    pos.y += pos.height + 2f;
                }
            }
            EditorGUI.indentLevel--;
        }

        void CreateNotify(SerializedProperty monoScript, SerializedProperty notify)
        {
            Type type = Type.GetType("Notify." + monoScript.objectReferenceValue.name + ", Assembly-CSharp");
            if (type == null)
            {
                monoScript.objectReferenceValue = null;
                Debug.LogError("Script does not exist in 'Notify' namespace.");
                return;
            }

            if (CanCasting(type, typeof(AnimNotify)))
                notify.managedReferenceValue = Activator.CreateInstance(type);
            else
                Debug.LogError("Only scripts inherited from AnimNotify are available");
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SMUI
{
    [CustomPropertyDrawer(typeof(SMUIState))]
    public class SMUIStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Indent correctly
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect lineRect = new Rect(position.x, position.y, position.width, lineHeight);

            // Draw "name" as dropdown
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            List<string> stateNames = GetStateNames(property);
            int currentIndex = Mathf.Max(0, stateNames.IndexOf(nameProp.stringValue));
            int newIndex = EditorGUI.Popup(lineRect, "State", currentIndex, stateNames.ToArray());
            if (newIndex != currentIndex)
            {
                nameProp.stringValue = stateNames[newIndex];
            }

            // Move to next line
            lineRect.y += lineHeight + 2;

            // Draw other fields normally
            SerializedProperty elementsProp = property.FindPropertyRelative("elementsToShow");
            EditorGUI.PropertyField(lineRect, elementsProp, true);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty elementsProp = property.FindPropertyRelative("elementsToShow");
            return EditorGUIUtility.singleLineHeight + 2 + EditorGUI.GetPropertyHeight(elementsProp, true);
        }

        private List<string> GetStateNames(SerializedProperty property)
        {
            var list = new List<string>();

            SerializedProperty parentControllerProp = property.FindPropertyRelative("parentController");
            SMUIController parentController = parentControllerProp?.objectReferenceValue as SMUIController;

            if (parentController == null || !parentController.HasAnimator)
            {
                list.Add("(No Animator Controller)");
                return list;
            }

            // Collect all state names from the animator
            StateCollector collector = new StateCollector();
            collector.CollectStates(parentController.AnimationController, parentController.layerIndex);

            // Gather names already used by other SMUIState objects in the same controller
            HashSet<string> usedNames = new HashSet<string>();
            SerializedProperty statesProp = parentControllerProp.serializedObject.FindProperty("states");
            if (statesProp != null)
            {
                for (int i = 0; i < statesProp.arraySize; i++)
                {
                    var otherState = statesProp.GetArrayElementAtIndex(i);
                    if (otherState != property)
                    {
                        string otherName = otherState.FindPropertyRelative("name").stringValue;
                        usedNames.Add(otherName);
                    }
                }
            }

            // Only add names not already used
            foreach (var stateName in collector.StateNames)
            {
                if (!usedNames.Contains(stateName))
                    list.Add(stateName);
            }

            // Ensure the current name is always included so it doesn't disappear from the dropdown
            string currentName = property.FindPropertyRelative("name").stringValue;
            if (!string.IsNullOrEmpty(currentName) && !list.Contains(currentName))
                list.Add(currentName);

            return list;
        }

    }
}

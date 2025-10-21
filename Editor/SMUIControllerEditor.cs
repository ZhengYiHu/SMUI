using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SMUI
{
    [CustomEditor(typeof(SMUIController))]
    public class SMUIControllerEditor : Editor
    {
        private SerializedProperty animatorProp;
        private SerializedProperty layerIndexProp;
        private SerializedProperty statesProp;

        private void OnEnable()
        {
            animatorProp = serializedObject.FindProperty("animator");
            layerIndexProp = serializedObject.FindProperty("layerIndex");
            statesProp = serializedObject.FindProperty("states");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Animator field
            EditorGUILayout.PropertyField(animatorProp);

            var controller = (SMUIController)target;
            var animator = controller.animator;

            // Layer dropdown
            if (animator != null && controller.AnimationController != null)
            {
                var layers = controller.AnimationController.layers;
                List<string> layerNames = new List<string>();
                foreach (var layer in layers)
                    layerNames.Add(layer.name);

                int newIndex = EditorGUILayout.Popup("Layer Index", layerIndexProp.intValue, layerNames.ToArray());
                if (newIndex != layerIndexProp.intValue)
                    layerIndexProp.intValue = newIndex;
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Assign an Animator with a Controller to enable layer selection.",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space();

            // Handle adding new states with unique names
            int oldSize = statesProp.arraySize;
            EditorGUILayout.PropertyField(statesProp, true);
            int newSize = statesProp.arraySize;

            if (newSize > oldSize) // A new element was added
            {
                for (int i = oldSize; i < newSize; i++)
                {
                    var newStateProp = statesProp.GetArrayElementAtIndex(i);
                    SerializedProperty nameProp = newStateProp.FindPropertyRelative("name");

                    // Assign a unique name
                    nameProp.stringValue = GetUniqueStateName(controller, nameProp.stringValue);
                }
            }

            // Check for duplicate names
            HashSet<string> seenNames = new HashSet<string>();
            bool hasDuplicates = false;
            for (int i = 0; i < statesProp.arraySize; i++)
            {
                var nameProp = statesProp.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                string name = nameProp.stringValue;
                if (!string.IsNullOrEmpty(name))
                {
                    if (seenNames.Contains(name))
                    {
                        hasDuplicates = true;
                        break;
                    }
                    seenNames.Add(name);
                }
            }

            if (hasDuplicates)
            {
                EditorGUILayout.HelpBox(
                    "Warning: Two or more states share the same name. This may cause unexpected behavior.",
                    MessageType.Warning
                );
            }

            serializedObject.ApplyModifiedProperties();

            // Force update hashes when modified
            if (GUI.changed)
            {
                controller.OnValidate();
            }
        }

        private string GetUniqueStateName(SMUIController controller, string currentName)
        {
            if (controller == null || !controller.HasAnimator)
                return "(No Animator Controller)";

            // Collect all state names from the animator
            StateCollector collector = new StateCollector();
            collector.CollectStates(controller.AnimationController, controller.layerIndex);

            HashSet<string> usedNames = new HashSet<string>();
            foreach (var state in controller.states)
                usedNames.Add(state.name);

            // Find the first unused name
            foreach (var name in collector.StateNames)
            {
                if (!usedNames.Contains(name))
                    return name;
            }

            // If all names are used, just return current name or fallback
            return string.IsNullOrEmpty(currentName) ? "(None)" : currentName;
        }
    }
}

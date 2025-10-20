using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace SMUI
{
    [Serializable]
    public class SMUIState
    {
        [HideInInspector]
        public SMUIController parentController;
        [Dropdown("GetStateNames")]
        public string name;
        [HideInInspector]
        public int fullNameHash;
        public List<SMUIElement> elementsToShow = new();

        // Get the names of all the available states to put populate the dropdown
        private DropdownList<string> GetStateNames()
        {
            var list = new DropdownList<string>();
            if (!parentController.hasAnimator)
            {
                list.Add("(No Animator Controller)", "(No Animator Controller)");
                return list;
            }
            StateCollector stateCollector = new();
            stateCollector.CollectStates(parentController.AnimationController, parentController.layerIndex);
            foreach (var stateName in stateCollector.StateNames)
            {
                list.Add(stateName, stateName);
            }
            return list;
        }

        public void UpdateHash()
        {
            string layerName = parentController.animator.GetLayerName(parentController.layerIndex);
            int fullHash = Animator.StringToHash($"{layerName}.{name}");
            fullNameHash = fullHash;
        }
    }
}
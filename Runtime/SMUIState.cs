using System;
using System.Collections.Generic;
using UnityEngine;

namespace SMUI
{
    [Serializable]
    public class SMUIState
    {
        [HideInInspector]
        public SMUIController parentController;
        public string name;
        [HideInInspector]
        public int fullNameHash;
        public List<SMUIElement> elementsToShow = new();


        public void UpdateHash()
        {
            string layerName = parentController.animator.GetLayerName(parentController.layerIndex);
            int fullHash = Animator.StringToHash($"{layerName}.{name}");
            fullNameHash = fullHash;
        }
    }
}
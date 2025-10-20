using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor.Animations;
using UnityEngine;

namespace SMUI
{
    public class SMUIController : MonoBehaviour
    {
        [SerializeField, Required] public Animator animator;
        [SerializeField,Dropdown("GetLayerNames")] public int layerIndex = 0;
        [SerializeField, OnValueChanged("OnValidate")] private List<SMUIState> states = new();
        private HashSet<SMUIElement> allElements = new();
        private SMUIState currentState;

        public AnimatorController AnimationController => animator?.runtimeAnimatorController as AnimatorController;
        public bool hasAnimator => animator != null;

        private void Awake()
        {
            foreach (var state in states)
            {
                foreach (var element in state.elementsToShow)
                {
                    allElements.Add(element);
                }
            }
            UpdateSMUIElements();
        }
        
        private void OnValidate()
        {
            // ensure every state knows its parent
            foreach (var s in states)
            {
                s.parentController = this;
                s.UpdateHash();
            }
        }

        private DropdownList<int> GetLayerNames()
        {
            DropdownList<int> list = new DropdownList<int>();
            if (!hasAnimator)
            {
                list.Add("(No Animator Controller)", -1);
                return list;
            }
            for (int i = 0; i < AnimationController.layers.Length; i++)
            {
                list.Add(AnimationController.layers[i].name, i);
            }
            return list;
        }

        /// <summary>
        /// Return the current SMUI State if it is defined in the list
        /// </summary>
        /// <returns></returns>
        SMUIState GetSMUIState(int hash)
        {
            foreach (var state in states)
            {
                if (hash== state.fullNameHash)
                {
                    return state;
                }
            }
            return null;
        }

        void Update()
        {
            SMUIState nextState;
            if (animator.IsInTransition(layerIndex))
            {
                AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(layerIndex);
                nextState = GetSMUIState(nextStateInfo.fullPathHash);
            }
            else
            {
                AnimatorStateInfo currentStateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
                nextState = GetSMUIState(currentStateInfo.fullPathHash);
            }
            if(nextState != currentState)
            {
                currentState = nextState;
                UpdateSMUIElements();
            }
        }

        void UpdateSMUIElements()
        {
            if (currentState == null)
            {
                foreach (SMUIElement element in allElements)
                {
                    element.SetActiveState(false);
                }
            }
            else
            {
                foreach (SMUIElement element in allElements)
                {
                    element.SetActiveState(currentState.elementsToShow.Contains(element));
                }
            }
        }
    }
}
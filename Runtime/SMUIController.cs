using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace SMUI
{
    public class SMUIController : MonoBehaviour
    {
        [SerializeField] public Animator animator;
        [SerializeField] public int layerIndex = 0;
        [SerializeField] public List<SMUIState> states = new();
        private HashSet<SMUIElement> allElements = new();
        private SMUIState currentState;

        public AnimatorController AnimationController => animator?.runtimeAnimatorController as AnimatorController;
        public bool HasAnimator => animator != null;

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
        
        public void OnValidate()
        {
            // ensure every state knows its parent
            foreach (var s in states)
            {
                s.parentController = this;
                s.UpdateHash();
            }
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
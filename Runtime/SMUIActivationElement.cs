using UnityEngine;

namespace SMUI
{
    public class SMUIActivationElement : SMUIElement
    {
        public override void SetActiveState(bool active)
        {
            gameObject.SetActive(active);
        }

        public override void Tick()
        {
        }
    }
}
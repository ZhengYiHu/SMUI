using UnityEngine;

namespace SMUI
{
    public class SMUIActivationElement : SMUIElement
    {
        public override void SetActiveState(bool active)
        {
            Debug.Log("Setting " + gameObject.name + " to " + active);
            gameObject.SetActive(active);
        }

        public override void Tick()
        {
        }
    }
}
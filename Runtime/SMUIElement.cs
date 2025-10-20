using UnityEngine;

namespace SMUI
{
    public abstract class SMUIElement : MonoBehaviour
    {
        public abstract void SetActiveState(bool active);
        public abstract void Tick();
    }
}
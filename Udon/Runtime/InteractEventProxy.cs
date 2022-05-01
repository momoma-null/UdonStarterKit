using UdonSharp;
using UnityEngine;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class InteractEventProxy : UdonSharpBehaviour
    {
        [SerializeField]
        UdonSharpBehaviour interactReceiver;
        [SerializeField]
        string interactSendEventName;

        public override void Interact()
        {
            if (interactReceiver != null)
                interactReceiver.SendCustomEvent(interactSendEventName);
        }
    }
}

using UdonSharp;
using UnityEngine;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class PickupEventProxy : UdonSharpBehaviour
    {
        [SerializeField]
        UdonSharpBehaviour onPickupReceiver;
        [SerializeField]
        string onPickupSendEventName;

        [SerializeField]
        UdonSharpBehaviour onDropReceiver;
        [SerializeField]
        string onDropSendEventName;

        [SerializeField]
        UdonSharpBehaviour onPickupUseDownReceiver;
        [SerializeField]
        string onPickupUseDownSendEventName;

        [SerializeField]
        UdonSharpBehaviour onPickupUseUpReceiver;
        [SerializeField]
        string onPickupUseUpSendEventName;

        public override void OnPickup()
        {
            if (onPickupReceiver != null)
                onPickupReceiver.SendCustomEvent(onPickupSendEventName);
        }

        public override void OnDrop()
        {
            if (onDropReceiver != null)
                onDropReceiver.SendCustomEvent(onDropSendEventName);
        }

        public override void OnPickupUseDown()
        {
            if (onPickupUseDownReceiver != null)
                onPickupUseDownReceiver.SendCustomEvent(onPickupUseDownSendEventName);
        }

        public override void OnPickupUseUp()
        {
            if (onPickupUseUpReceiver != null)
                onPickupUseUpReceiver.SendCustomEvent(onPickupUseUpSendEventName);
        }
    }
}

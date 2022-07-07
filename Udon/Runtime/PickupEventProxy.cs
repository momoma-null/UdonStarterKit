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
        [CustomEventPopup(nameof(onPickupReceiver))]
        string onPickupSendEventName;

        [SerializeField]
        UdonSharpBehaviour onDropReceiver;
        [SerializeField]
        [CustomEventPopup(nameof(onDropReceiver))]
        string onDropSendEventName;

        [SerializeField]
        UdonSharpBehaviour onPickupUseDownReceiver;
        [SerializeField]
        [CustomEventPopup(nameof(onPickupUseDownReceiver))]
        string onPickupUseDownSendEventName;

        [SerializeField]
        UdonSharpBehaviour onPickupUseUpReceiver;
        [SerializeField]
        [CustomEventPopup(nameof(onPickupUseUpReceiver))]
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

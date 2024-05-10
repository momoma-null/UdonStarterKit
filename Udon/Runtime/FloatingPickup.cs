
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [RequireComponent(typeof(Rigidbody))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    sealed class FloatingPickup : UdonSharpBehaviour
    {
        [SerializeField]
        Rigidbody targetRigidbody;
        [SerializeField]
        float forceOnDrop = -50f;
        [SerializeField]
        Joint springPrefab;

        Joint spring;

        void Start()
        {
            spring = Instantiate(springPrefab.gameObject).GetComponent<Joint>();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (!player.isLocal)
                spring.connectedBody = default;
        }

        public override void OnPickup()
        {
            spring.connectedBody = default;
        }

        public override void OnDrop()
        {
            targetRigidbody.AddForce(0f, forceOnDrop, 0f, ForceMode.Acceleration);
            spring.transform.position = transform.position;
            spring.connectedBody = targetRigidbody;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        void Reset()
        {
            targetRigidbody = GetComponent<Rigidbody>();
        }
#endif
    }
}

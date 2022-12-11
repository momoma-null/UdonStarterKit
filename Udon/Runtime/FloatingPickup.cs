
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class FloatingPickup : UdonSharpBehaviour
    {
        [SerializeField]
        PositionConstraint constraint;
        [SerializeField]
        Transform puller;
        [SerializeField]
        Transform anchor;
        [SerializeField]
        float offset = 0.05f;

        public override void OnPickup()
        {
            constraint.constraintActive = false;
        }

        public override void OnDrop()
        {
            puller.position += Vector3.down * offset;
            anchor.position = constraint.transform.position;
            constraint.constraintActive = true;
        }
    }
}

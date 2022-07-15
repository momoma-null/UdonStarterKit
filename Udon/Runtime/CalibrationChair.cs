using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class CalibrationChair : UdonSharpBehaviour
    {
        [SerializeField]
        Transform _seatPosition = null;

        VRCPlayerApi _seatedPlayer;

        public override void Interact()
        {
            Networking.LocalPlayer.UseAttachedStation();
        }

        public void UpdateSeatPosition()
        {
            if (_seatedPlayer == null)
                return;
            var inverseRot = Quaternion.Inverse(_seatedPlayer.GetRotation());
            var playerPos = inverseRot * _seatedPlayer.GetPosition();
            var leftLowerLegPos = inverseRot * _seatedPlayer.GetBonePosition(HumanBodyBones.LeftLowerLeg);
            var rightLowerLegPos = inverseRot * _seatedPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg);
            var height = (Mathf.Min(leftLowerLegPos.y, rightLowerLegPos.y) - playerPos.y) * -0.9f;
            var depth = (Mathf.Min(leftLowerLegPos.z, rightLowerLegPos.z) - playerPos.z) * -0.9f;
            _seatPosition.localPosition = new Vector3(0, height, depth);
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            _seatedPlayer = player;
            _seatPosition.localPosition = new Vector3(0f, -_seatPosition.parent.localPosition.y, 0f);
            SendCustomEventDelayedSeconds(nameof(UpdateSeatPosition), 0.01f);
            SendCustomEventDelayedSeconds(nameof(UpdateSeatPosition), 0.5f);
            SendCustomEventDelayedSeconds(nameof(UpdateSeatPosition), 1f);
            SendCustomEventDelayedSeconds(nameof(UpdateSeatPosition), 1.5f);
            SendCustomEventDelayedSeconds(nameof(UpdateSeatPosition), 2f);
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            _seatedPlayer = null;
        }
    }
}

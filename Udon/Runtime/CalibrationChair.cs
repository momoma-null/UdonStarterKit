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
            var playerPos = _seatedPlayer.GetPosition();
            var leftLowerLegPos = _seatedPlayer.GetBonePosition(HumanBodyBones.LeftLowerLeg);
            var rightLowerLegPos = _seatedPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg);
            var rightUpperLegPos = _seatedPlayer.GetBonePosition(HumanBodyBones.RightUpperLeg);
            var height = (Mathf.Min(leftLowerLegPos.y, rightLowerLegPos.y) - playerPos.y) * -0.9f;
            var depth = Vector3.Distance(rightLowerLegPos, rightUpperLegPos) * -0.6f;
            _seatPosition.localPosition = new Vector3(0, height, depth);
            SendCustomEventDelayedSeconds(nameof(UpdateSeatPosition), 0.5f);
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            _seatedPlayer = player;
            _seatPosition.localPosition = new Vector3(0f, -_seatPosition.parent.localPosition.y, 0f);
            SendCustomEventDelayedSeconds(nameof(UpdateSeatPosition), 0.05f);
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            _seatedPlayer = null;
        }
    }
}

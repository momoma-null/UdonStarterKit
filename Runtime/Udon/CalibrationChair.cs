using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    sealed class CalibrationChair : UdonSharpBehaviour
    {
        [SerializeField]
        Transform _seatPosition = null;

        VRCPlayerApi _seatedPlayer;
        int _progressCount;

        public override void Interact()
        {
            Networking.LocalPlayer.UseAttachedStation();
        }

        void Update()
        {
            if (_seatedPlayer != null && _seatPosition != null)
            {
                var playerPos = _seatedPlayer.GetPosition();
                var leftLowerLegPos = _seatedPlayer.GetBonePosition(HumanBodyBones.LeftLowerLeg);
                var rightLowerLegPos = _seatedPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg);
                var rightUpperLegPos = _seatedPlayer.GetBonePosition(HumanBodyBones.RightUpperLeg);
                var height = (Mathf.Min(leftLowerLegPos.y, rightLowerLegPos.y) - playerPos.y) * -0.9f;
                var depth = Vector3.Distance(rightLowerLegPos, rightUpperLegPos) * -0.6f;
                _seatPosition.localPosition = new Vector3(0, height, depth);
                ++_progressCount;
                if (_progressCount > 30)
                    _seatedPlayer = null;
            }
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            _seatedPlayer = player;
            _progressCount = 0;
        }
    }
}

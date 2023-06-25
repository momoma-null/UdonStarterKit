
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
#endif

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class CalibrationChair : UdonSharpBehaviour
    {
        [SerializeField]
        Transform _seatPosition = null;
        [SerializeField]
        bool _disabledWhenVR = true;

        VRCPlayerApi _seatedPlayer;
        int count;

        void Start()
        {
            if (_disabledWhenVR && Networking.LocalPlayer != null && Networking.LocalPlayer.IsUserInVR())
            {
                var collider = GetComponent<Collider>();
                collider.enabled = false;
            }
        }

        public override void Interact()
        {
            Networking.LocalPlayer.UseAttachedStation();
        }

        public void UpdateSeatPosition()
        {
            if (_seatedPlayer == null)
                return;
            var localMatrix = _seatPosition.worldToLocalMatrix;
            var leftLowerLegPos = localMatrix.MultiplyPoint3x4(_seatedPlayer.GetBonePosition(HumanBodyBones.LeftLowerLeg));
            var leftUpperLegPos = localMatrix.MultiplyPoint3x4(_seatedPlayer.GetBonePosition(HumanBodyBones.LeftUpperLeg));
            var rightLowerLegPos = localMatrix.MultiplyPoint3x4(_seatedPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg));
            var upperLegRadius = Vector3.Distance(leftLowerLegPos, leftUpperLegPos) * 0.1f;
            var height = -Mathf.Min(leftLowerLegPos.y, rightLowerLegPos.y) + upperLegRadius;
            var depth = -Mathf.Min(leftLowerLegPos.z, rightLowerLegPos.z) + upperLegRadius;
            _seatPosition.localPosition = new Vector3(0, height, depth);
            if (++count < 150)
                SendCustomEventDelayedFrames(nameof(UpdateSeatPosition), 1, EventTiming.LateUpdate);
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            _seatedPlayer = player;
            count = 0;
            UpdateSeatPosition();
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            _seatedPlayer = null;
            count = 0;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player == _seatedPlayer)
                OnStationEntered(player);
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        static class Styles
        {
            public static GUIContent kneeText = EditorGUIUtility.TrTextContent("Knee Position");
        }

        void OnDrawGizmosSelected()
        {
            var kneesTransform = _seatPosition.parent;
            var position = kneesTransform.position;
            Gizmos.DrawLine(position + kneesTransform.right * 0.2f, position - kneesTransform.right * 0.2f);
            Handles.Label(position, Styles.kneeText);
        }
#endif
    }
}

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
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

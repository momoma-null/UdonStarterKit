using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public sealed class ToggleButton : UdonSharpBehaviour
    {
        [SerializeField]
        AudioSource _audioSource = null;
        [SerializeField]
        bool _useSync;
        [SerializeField]
        Collider _collider;

        float _VRInteractInterval = 0f;

        void FixedUpdate()
        {
            if (_VRInteractInterval > 0f)
                _VRInteractInterval -= Time.fixedDeltaTime;
            if (_VRInteractInterval <= 0f && Networking.LocalPlayer != null && Networking.LocalPlayer.IsUserInVR() && _collider != null)
            {
                var rightFingerPosition = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightIndexDistal);
                var leftFingerPosition = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.LeftIndexDistal);
                CheckCollision(rightFingerPosition);
                CheckCollision(leftFingerPosition);
            }
        }

        void CheckCollision(Vector3 position)
        {
            if (0.004f > Vector3.SqrMagnitude(_collider.ClosestPointOnBounds(position) - position))
            {
                Interact();
                _VRInteractInterval = 1f;
            }
        }

        public override void Interact()
        {
            if (0f < _VRInteractInterval)
                return;
            if (_audioSource != null && _audioSource.clip != null)
                _audioSource.PlayOneShot(_audioSource.clip);
        }
    }
}

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public sealed class ToggleSwitch : UdonSharpBehaviour
    {
        const string k_toggleParameterName = "IsOn";

        [SerializeField]
        bool _isOn;
        [SerializeField]
        bool _useSync;
        [SerializeField]
        AudioClip _switchAudio = null;
        [SerializeField]
        Animator _toggleAnimator = null;

        [SerializeField]
        Animator _animator = null;
        [SerializeField]
        AudioSource _audioSource = null;

        [UdonSynced]
        bool _syncedIsOn;

        Collider _collider = null;
        VRCPlayerApi _localPlayer;

        // GCAlloc measures
        Vector3 _tempPosition;

        void Start()
        {
            _collider = GetComponent<Collider>();
            if (_collider == null)
                enabled = false;
            _localPlayer = Networking.LocalPlayer;
            if (_localPlayer != null && _localPlayer.IsUserInVR())
            {
                DeciUpdate();
            }
        }

        public void DeciUpdate()
        {
            _tempPosition = _localPlayer.GetBonePosition(HumanBodyBones.RightIndexDistal);
            if (CheckCollision(_tempPosition))
                return;
            _tempPosition = _localPlayer.GetBonePosition(HumanBodyBones.LeftIndexDistal);
            if (CheckCollision(_tempPosition))
                return;
            SendCustomEventDelayedSeconds(nameof(DeciUpdate), 0.1f);
        }

        bool CheckCollision(Vector3 position)
        {
            if (0.004f > Vector3.SqrMagnitude(_collider.ClosestPointOnBounds(position) - position))
            {
                Use();
                SendCustomEventDelayedSeconds(nameof(DeciUpdate), 1f);
                return true;
            }
            return false;
        }

        public override void Interact()
        {
            Use();
        }

        void Use()
        {
            _isOn = !_isOn;
            if (_useSync)
            {
                var go = this.gameObject;
                if (!Networking.IsOwner(go))
                {
                    Networking.SetOwner(Networking.LocalPlayer, go);
                }
                _syncedIsOn = _isOn;
                RequestSerialization();
            }
            Apply();
        }

        public override void OnDeserialization()
        {
            _isOn = _syncedIsOn;
            Apply();
        }

        void Apply()
        {
            if (_audioSource != null && _switchAudio != null)
                _audioSource.PlayOneShot(_switchAudio);
            if (_animator != null)
            {
                _animator.SetBool(k_toggleParameterName, _isOn);
            }
            if (_toggleAnimator != null)
            {
                _toggleAnimator.SetBool(k_toggleParameterName, _isOn);
            }
        }
    }
}

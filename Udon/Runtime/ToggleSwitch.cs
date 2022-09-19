using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
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
        Gradient _switchColor = new Gradient();
        [SerializeField]
        Animator _toggleAnimator = null;
        [SerializeField]
        GameObject[] _toggleObjects = new GameObject[0];

        [SerializeField]
        Animator _animator = null;
        [SerializeField]
        AudioSource _audioSource = null;
        [SerializeField]
        Slider _slider = null;
        [SerializeField]
        SpriteRenderer _renderer = null;

        [UdonSynced]
        bool _syncedIsOn;

        Collider _collider = null;
        VRCPlayerApi _localPlayer;
        bool isLocked = false;

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
            if (CheckCollision(_localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position))
                return;
            if (CheckCollision(_localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position))
                return;
            SendCustomEventDelayedSeconds(nameof(DeciUpdate), 0.1f);
        }

        bool CheckCollision(Vector3 position)
        {
            if (!isLocked && 0.004f > Vector3.SqrMagnitude(_collider.ClosestPointOnBounds(position) - position))
            {
                Use();
                SendCustomEventDelayedSeconds(nameof(DeciUpdate), 1f);
                isLocked = true;
                SendCustomEventDelayedSeconds(nameof(Unlock), 0.5f);
                return true;
            }
            return false;
        }

        public void Unlock()
        {
            isLocked = false;
        }

        public override void Interact()
        {
            if (!isLocked)
            {
                Use();
                isLocked = true;
                SendCustomEventDelayedSeconds(nameof(Unlock), 0.12f);
            }
        }

        void Use()
        {
            _isOn = !_isOn;
            if (_useSync)
            {
                if (!Networking.IsOwner(this.gameObject))
                {
                    Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                }
                _syncedIsOn = _isOn;
                RequestSerialization();
            }
            Apply();
        }

        public override void OnDeserialization()
        {
            if (_useSync)
            {
                _isOn = _syncedIsOn;
                Apply();
            }
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
            foreach (var obj in _toggleObjects)
            {
                if (obj != null)
                    obj.SetActive(_isOn);
            }
        }

        public void UpdateColor()
        {
            _renderer.color = _switchColor.Evaluate(_slider.value);
        }
    }
}

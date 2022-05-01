using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public sealed class AreaSound : UdonSharpBehaviour
    {
        [SerializeField]
        bool _useSync;
        [SerializeField]
        bool _onceOnly;
        [SerializeField]
        AudioClip _sound;
        [SerializeField]
        AudioSource _audioSource = null;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                if (_useSync)
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlaySound));
                else
                    PlaySound();
                if (_onceOnly)
                    GetComponent<Collider>().enabled = false;
            }
        }

        public void PlaySound()
        {
            _audioSource.PlayOneShot(_sound);
        }
    }
}

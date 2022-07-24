using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class Teleporter : UdonSharpBehaviour
    {
        [SerializeField]
        Transform teleportPoint = null;
        [SerializeField]
        VRC_SceneDescriptor.SpawnOrientation orientation = VRC_SceneDescriptor.SpawnOrientation.Default;
        [SerializeField]
        AudioClip teleportAudio = null;
        [SerializeField, HideInInspector]
        AudioSource audioSource = null;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                player.TeleportTo(teleportPoint.position, teleportPoint.rotation, orientation, false);
                if (audioSource != null && teleportAudio != null)
                    audioSource.PlayOneShot(teleportAudio);
            }
        }
    }
}

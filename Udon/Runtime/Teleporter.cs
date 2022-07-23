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

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal)
                player.TeleportTo(teleportPoint.position, teleportPoint.rotation, orientation, false);
        }
    }
}

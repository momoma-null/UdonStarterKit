
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class PlayerMods : UdonSharpBehaviour
    {
        [SerializeField]
        float _walkSpeed = 2f;
        [SerializeField]
        float _runSpeed = 4f;
        [SerializeField]
        float _strafeSpeed = 2f;
        [SerializeField]
        float _jumpImpulse = 3f;

        void Start()
        {
            var player = Networking.LocalPlayer;
            if (Utilities.IsValid(player))
            {
                player.SetWalkSpeed(_walkSpeed);
                player.SetRunSpeed(_runSpeed);
                player.SetStrafeSpeed(_strafeSpeed);
                player.SetJumpImpulse(_jumpImpulse);
            }
            Destroy(this);
        }
    }
}

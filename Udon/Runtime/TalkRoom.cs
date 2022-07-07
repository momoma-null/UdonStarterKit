using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class TalkRoom : UdonSharpBehaviour
    {
        const string IN_ROOM_TAG_NAME = "In";

        [SerializeField]
        float defaultVoiceDistanceFar = 25f;
        [SerializeField]
        float defaultAvatarAudioFarRadius = 40f;

        bool _inRoomSelf;
        VRCPlayerApi[] _players = new VRCPlayerApi[32];
        string tagName;

        void Start()
        {
            tagName = $"{nameof(TalkRoom)}_{GetInstanceID()}";
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                _inRoomSelf = true;
                GetPlayers();
                foreach (var p in _players)
                {
                    if (p.IsValid() && !p.isLocal)
                    {
                        SetAudioRange(player, p.GetPlayerTag(tagName) == IN_ROOM_TAG_NAME);
                    }
                }
            }
            else
            {
                player.SetPlayerTag(tagName, IN_ROOM_TAG_NAME);
                SetAudioRange(player, _inRoomSelf);
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                _inRoomSelf = false;
                GetPlayers();
                foreach (var p in _players)
                {
                    if (p.IsValid() && !p.isLocal)
                    {
                        SetAudioRange(player, p.GetPlayerTag(tagName) != IN_ROOM_TAG_NAME);
                    }
                }
            }
            else
            {
                player.SetPlayerTag(tagName, string.Empty);
                SetAudioRange(player, !_inRoomSelf);
            }
        }

        void SetAudioRange(VRCPlayerApi player, bool active)
        {
            if (active)
            {
                player.SetVoiceDistanceFar(defaultVoiceDistanceFar);
                player.SetAvatarAudioFarRadius(defaultAvatarAudioFarRadius);
            }
            else
            {
                player.SetVoiceDistanceFar(0f);
                player.SetAvatarAudioFarRadius(0f);
            }
        }

        void GetPlayers()
        {
            var playerCount = VRCPlayerApi.GetPlayerCount();
            if (playerCount > _players.Length)
            {
                _players = new VRCPlayerApi[playerCount + 4];
            }
            VRCPlayerApi.GetPlayers(_players);
        }
    }
}

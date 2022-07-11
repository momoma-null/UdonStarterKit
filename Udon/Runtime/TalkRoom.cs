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
        VRCPlayerApi[] players = new VRCPlayerApi[1];
        string tagName;

        void Start()
        {
            tagName = $"{nameof(TalkRoom)}_{GetInstanceID()}";
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            GetPlayers();
            if (_inRoomSelf && player.GetPlayerTag(tagName) != IN_ROOM_TAG_NAME)
                SetAudioRange(player, false);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            GetPlayers();
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                _inRoomSelf = true;
                foreach (var p in players)
                {
                    if (p != null && p.IsValid() && !p.isLocal)
                    {
                        SetAudioRange(p, p.GetPlayerTag(tagName) == IN_ROOM_TAG_NAME);
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
                foreach (var p in players)
                {
                    if (p != null && p.IsValid() && !p.isLocal)
                    {
                        SetAudioRange(p, p.GetPlayerTag(tagName) != IN_ROOM_TAG_NAME);
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
            if (playerCount != players.Length)
                players = new VRCPlayerApi[playerCount];
            VRCPlayerApi.GetPlayers(players);
        }
    }
}

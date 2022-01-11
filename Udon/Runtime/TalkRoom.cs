using System;
using UdonSharp;
using VRC.SDKBase;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class TalkRoom : UdonSharpBehaviour
    {
        bool _inRoomSelf;
        VRCPlayerApi[] _players = new VRCPlayerApi[80];

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                _inRoomSelf = true;
                VRCPlayerApi.GetPlayers(_players);
                foreach (var p in _players)
                {
                    if (p.IsValid() && !p.isLocal)
                    {
                        SetAudioRange(player, Contains(p.playerId));
                    }
                }
            }
            else
            {
                SetAudioRange(player, _inRoomSelf);
                Add(player.playerId);
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                _inRoomSelf = false;
                VRCPlayerApi.GetPlayers(_players);
                foreach (var p in _players)
                {
                    if (p.IsValid() && !p.isLocal)
                    {
                        SetAudioRange(player, !Contains(p.playerId));
                    }
                }
            }
            else
            {
                SetAudioRange(player, !_inRoomSelf);
                Remove(player.playerId);
            }
        }

        void SetAudioRange(VRCPlayerApi player, bool active)
        {
            if (active)
            {
                player.SetVoiceDistanceFar(25f);
                player.SetAvatarAudioFarRadius(40f);
            }
            else
            {
                player.SetVoiceDistanceFar(0);
                player.SetAvatarAudioFarRadius(0);
            }
        }

        int[] _buckets;
        int[] _slotsValue;
        int[] _slotsNext;
        int[] _slotsHashCode;
        int _count = 0;
        int _lastIndex = 0;
        int _freeList = -1;

        void Initialize()
        {
            _buckets = new int[3];
            _slotsValue = new int[3];
            _slotsNext = new int[3];
            _slotsHashCode = new int[3];
        }

        void IncreaseCapacity()
        {
            var newSize = _count * 2 - 1;
            var newSlotsValue = new int[newSize];
            Array.Copy(_slotsValue, 0, newSlotsValue, 0, _lastIndex);
            var newSlotsNext = new int[newSize];
            Array.Copy(_slotsNext, 0, newSlotsNext, 0, _lastIndex);
            var newSlotsHashCode = new int[newSize];
            Array.Copy(_slotsHashCode, 0, newSlotsHashCode, 0, _lastIndex);

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < _lastIndex; i++)
            {
                int bucket = newSlotsHashCode[i] % newSize;
                newSlotsNext[i] = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }
            _slotsValue = newSlotsValue;
            _slotsNext = newSlotsNext;
            _slotsHashCode = newSlotsHashCode;
            _buckets = newBuckets;
        }

        bool Contains(int item)
        {
            if (_buckets != null)
            {
                int hashCode = InternalGetHashCode(item);
                for (int i = _buckets[hashCode % _buckets.Length] - 1; i >= 0; i = _slotsNext[i])
                {
                    if (_slotsHashCode[i] == hashCode && _slotsValue[i] == item)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void Add(int value)
        {
            if (_buckets == null)
            {
                Initialize();
            }

            int hashCode = InternalGetHashCode(value);
            int bucket = hashCode % _buckets.Length;
            for (int i = _buckets[hashCode % _buckets.Length] - 1; i >= 0; i = _slotsNext[i])
            {
                if (_slotsHashCode[i] == hashCode && _slotsValue[i] == value)
                {
                    return;
                }
            }

            int index;
            if (_freeList >= 0)
            {
                index = _freeList;
                _freeList = _slotsNext[index];
            }
            else
            {
                if (_lastIndex == _slotsValue.Length)
                {
                    IncreaseCapacity();
                    bucket = hashCode % _buckets.Length;
                }
                index = _lastIndex;
                _lastIndex++;
            }
            _slotsHashCode[index] = hashCode;
            _slotsValue[index] = value;
            _slotsNext[index] = _buckets[bucket] - 1;
            _buckets[bucket] = index + 1;
            _count++;
        }

        void Remove(int item)
        {
            if (_buckets != null)
            {
                int hashCode = InternalGetHashCode(item);
                int bucket = hashCode % _buckets.Length;
                int last = -1;
                for (int i = _buckets[bucket] - 1; i >= 0; last = i, i = _slotsNext[i])
                {
                    if (_slotsHashCode[i] == hashCode && _slotsValue[i] == item)
                    {
                        if (last < 0)
                        {
                            _buckets[bucket] = _slotsNext[i] + 1;
                        }
                        else
                        {
                            _slotsNext[last] = _slotsNext[i];
                        }
                        _slotsHashCode[i] = -1;
                        _slotsValue[i] = 0;
                        _slotsNext[i] = _freeList;

                        _count--;
                        if (_count == 0)
                        {
                            _lastIndex = 0;
                            _freeList = -1;
                        }
                        else
                        {
                            _freeList = i;
                        }
                        return;
                    }
                }
            }
        }

        int InternalGetHashCode(int item)
        {
            return item.GetHashCode() & 0x7FFFFFFF;
        }

    }
}

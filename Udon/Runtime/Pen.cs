using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public sealed class Pen : UdonSharpBehaviour
    {
        [SerializeField]
        TrailRenderer trailRenderer;
        [SerializeField]
        PositionConstraint constraint;
        [SerializeField]
        AudioSource writingAudio;
        [SerializeField]
        Transform farPoint;
        [SerializeField]
        Transform penPoint;

        [UdonSynced]
        Vector3[] syncedPositions = new Vector3[0];
        [UdonSynced]
        int syncedPositionCount = 0;
        [UdonSynced]
        int syncedStartIndex = 0;

        ConstraintSource constraintSource;
        Transform trailTransform;
        bool isDrawing;
        bool waitingData;
        int syncRetryCount;
        int lastPositionCount = 0;
        Vector3[] localPositions = new Vector3[0];

        void Start()
        {
            constraintSource = constraint.GetSource(0);
            trailTransform = trailRenderer.transform;
            writingAudio.Play();
            writingAudio.Pause();
            var farParent = farPoint.parent;
            farParent.SetParent(null);
            farParent.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            constraint.gameObject.SetActive(true);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsOwner(gameObject) && !player.isLocal && localPositions.Length != 0)
            {
                lastPositionCount = 0;
                RequestSerialization();
            }
        }

        public override void OnPreSerialization()
        {
            syncedPositionCount = localPositions.Length;
            syncedStartIndex = lastPositionCount;
            var copyLength = Mathf.Min(syncedPositionCount - syncedStartIndex, 1000);
            if (syncedPositions.Length != copyLength)
                syncedPositions = new Vector3[copyLength];
            if (copyLength > 0)
                Array.Copy(localPositions, syncedStartIndex, syncedPositions, 0, copyLength);
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (!result.success)
            {
                if (syncRetryCount < 3)
                {
                    RequestSerialization();
                    ++syncRetryCount;
                    return;
                }
            }
            syncRetryCount = 0;
            lastPositionCount = syncedStartIndex + syncedPositions.Length;
            if (lastPositionCount < syncedPositionCount)
                RequestSerialization();
        }

        public override void OnDeserialization()
        {
            if (localPositions.Length != syncedPositionCount)
            {
                var newLocalPositions = new Vector3[syncedPositionCount];
                Array.Copy(localPositions, newLocalPositions, Mathf.Min(localPositions.Length, syncedPositionCount));
                localPositions = newLocalPositions;
            }
            var copyLength = syncedPositions.Length;
            if (copyLength > 0)
                Array.Copy(syncedPositions, 0, localPositions, syncedStartIndex, copyLength);
            lastPositionCount = syncedStartIndex + copyLength;

            if (isDrawing)
                waitingData = true;
            else
                ApplyReceivedData();
        }

        void ApplyReceivedData()
        {
            trailRenderer.Clear();
            if (localPositions.Length > 0)
                trailRenderer.AddPositions(localPositions);
            waitingData = false;
        }

        public void OnPickupPen()
        {
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void BeginUsing()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(BeginUsingAll));
        }

        public void BeginUsingAll()
        {
            constraintSource.sourceTransform = penPoint;
            constraint.SetSource(0, constraintSource);
            trailTransform.position = penPoint.position;
            writingAudio.UnPause();
            isDrawing = true;
        }

        public void EndUsing()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EndUsingAll));

            var count = trailRenderer.positionCount;
            localPositions = new Vector3[count];
            if (count > 0)
                trailRenderer.GetPositions(localPositions);
            RequestSerialization();
        }

        public void EndUsingAll()
        {
            constraintSource.sourceTransform = farPoint;
            constraint.SetSource(0, constraintSource);
            trailTransform.position = farPoint.position;
            writingAudio.Pause();
            isDrawing = false;
            if (waitingData)
                ApplyReceivedData();
        }

        public void Clear()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ClearAll));
        }

        public void ClearAll()
        {
            trailRenderer.Clear();
            localPositions = new Vector3[0];
            lastPositionCount = 0;
            if (Networking.IsOwner(gameObject))
            {
                RequestSerialization();
            }
        }

        public void Undo()
        {
            if (!Networking.IsOwner(gameObject))
            {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Undo));
                return;
            }
            if (localPositions.Length == 0 || isDrawing)
                return;
            var farPosition = farPoint.position;
            if (localPositions[localPositions.Length - 1] == farPosition)
                localPositions[localPositions.Length - 1] += Vector3.up;
            var newLength = Array.LastIndexOf(localPositions, farPosition) + 1;
            trailRenderer.Clear();
            var newPositions = new Vector3[newLength];
            if (newLength > 0)
            {
                Array.Copy(localPositions, newPositions, newLength);
                trailRenderer.AddPositions(newPositions);
            }

            lastPositionCount = newLength;
            localPositions = newPositions;
            RequestSerialization();
        }
    }
}

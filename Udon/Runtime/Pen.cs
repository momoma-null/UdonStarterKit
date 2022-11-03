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

        ConstraintSource constraintSource;
        Transform trailTransform;
        bool isDrawing;
        bool waitingData;
        int syncRetryCount;

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
            if (Networking.IsOwner(gameObject) && !player.isLocal)
            {
                SendData();
            }
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (!result.success)
            {
                if (syncRetryCount < 3)
                {
                    SendCustomEventDelayedSeconds(nameof(SendData), 1.84f);
                    ++syncRetryCount;
                    return;
                }
            }
            syncRetryCount = 0;
        }

        public void SendData()
        {
            var count = trailRenderer.positionCount;
            syncedPositions = new Vector3[count];
            if (count > 0)
                trailRenderer.GetPositions(syncedPositions);
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            if (isDrawing)
                waitingData = true;
            else
                ApplyReceivedData();
        }

        void ApplyReceivedData()
        {
            trailRenderer.Clear();
            if (syncedPositions.Length > 0)
                trailRenderer.AddPositions(syncedPositions);
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
            SendData();
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
            syncedPositions = new Vector3[0];
        }

        public void Undo()
        {
            if (!Networking.IsOwner(gameObject))
            {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(Undo));
                return;
            }
            if (syncedPositions.Length == 0 || isDrawing)
                return;
            var farPosition = farPoint.position;
            if (syncedPositions[syncedPositions.Length - 1] == farPosition)
                syncedPositions[syncedPositions.Length - 1] += Vector3.up;
            var newLength = Array.LastIndexOf(syncedPositions, farPosition);
            trailRenderer.Clear();
            if (newLength > 0)
            {
                var newPositions = new Vector3[newLength];
                Array.Copy(syncedPositions, newPositions, newLength);
                trailRenderer.AddPositions(newPositions);
            }
            SendData();
        }
    }
}

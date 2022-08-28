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
            if (count > 0)
            {
                syncedPositions = new Vector3[count];
                trailRenderer.GetPositions(syncedPositions);
                RequestSerialization();
            }
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
            trailRenderer.AddPositions(syncedPositions);
            waitingData = false;
        }

        public void OnPickupPen()
        {
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
    }
}

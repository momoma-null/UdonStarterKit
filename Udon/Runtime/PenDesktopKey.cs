
using UdonSharp;
using UnityEngine;

namespace MomomaAssets.UdonStarterKit.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class PenDesktopKey : UdonSharpBehaviour
    {
        [SerializeField]
        Pen pen;
        [SerializeField]
        KeyCode undoKey;
        [SerializeField]
        KeyCode clearKey;

        void Update()
        {
            if (Input.GetKeyDown(undoKey))
            {
                pen.Undo();
            }
            else if (Input.GetKeyDown(clearKey))
            {
                pen.Clear();
            }
        }
    }
}

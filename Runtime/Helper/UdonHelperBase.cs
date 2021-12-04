using UnityEngine;
using VRC.Udon;

namespace MomomaAssets.UdonStarterKit.Helper
{
    [DisallowMultipleComponent]
    public abstract class UdonHelperBase : MonoBehaviour
    {
        [SerializeField]
        UdonBehaviour _udonBehaviour;

        void Reset()
        {
            hideFlags = HideFlags.DontSaveInBuild;
            _udonBehaviour = GetComponentInChildren<UdonBehaviour>();
        }
    }
}

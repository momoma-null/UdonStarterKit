using UnityEngine;

namespace MomomaAssets.UdonStarterKit.Helper
{
    public sealed class ToggleButtonHelper : UdonHelperBase
    {
        [SerializeField]
        bool _isOn;
        [SerializeField]
        GameObject _targetObject;
        [SerializeField]
        Sprite _buttonIcon;
        [SerializeField]
        AudioSource _buttonSound;
    }
}

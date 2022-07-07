using System;
using UnityEngine;

namespace MomomaAssets.UdonStarterKit.Udon
{
    public sealed class CustomEventPopupAttribute : PropertyAttribute
    {
        public readonly string TargetFieldName;

        public CustomEventPopupAttribute(string targetFieldName)
        {
            TargetFieldName = targetFieldName;
        }
    }
}

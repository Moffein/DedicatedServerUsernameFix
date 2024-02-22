using BepInEx;
using System;
using RoR2;
using Facepunch.Steamworks;
using UnityEngine;

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute
    {
    }
}

namespace DedicatedServerUsernameFix
{
    [BepInPlugin("com.Moffein.DedicatedServerUsernameFix", "DedicatedServerUsernameFix", "1.0.0")]
    public class DedicatedServerUsernameFix : BaseUnityPlugin
    {
        private void Awake()
        {

        }

    }
}

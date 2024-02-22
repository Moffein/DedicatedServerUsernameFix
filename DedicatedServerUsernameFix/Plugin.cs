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
            On.RoR2.NetworkPlayerName.GetResolvedName += FixGetResolvedName;
            On.RoR2.SteamworksLobbyManager.GetUserDisplayName += FixGetUserDisplayName;
        }

        //Noticed that usernames can show on client even if server's list_players always shows ???
        //Maybe the right way to go is to fix this clientside?
        private string FixGetUserDisplayName(On.RoR2.SteamworksLobbyManager.orig_GetUserDisplayName orig, SteamworksLobbyManager self, UserID user)
        {
            if (!(NetworkSession.instance && NetworkSession.instance.flags.HasFlag(NetworkSession.Flags.IsDedicatedServer)))
            {
                return orig(self, user);
            }

            //Client.Instance seems to be null on the server, casuing clients to show up as "none" internally
            string result = "none";
            if (Client.Instance != null)
            {
                result = Client.Instance.Friends.GetName(user.CID.steamValue);
            }
            else if (Server.Instance != null && user.CID.isSteam)
            {
                //This errors because internal platform pointer is 0
                /*Server.Instance.native.friends.RequestUserInformation(user.CID.steamValue, true);
                result = Server.Instance.native.friends.GetFriendPersonaName(user.CID.steamValue);*/
            }
            return result;
        }

        private string FixGetResolvedName(On.RoR2.NetworkPlayerName.orig_GetResolvedName orig, ref NetworkPlayerName self)
        {
            //Don't do anything on non-dedicated servers.
            if (!(NetworkSession.instance && NetworkSession.instance.flags.HasFlag(NetworkSession.Flags.IsDedicatedServer)))
            {
                return orig(ref self);
            }

            //Check override first
            if (!string.IsNullOrEmpty(self.nameOverride))
            {
                if (PlatformSystems.ShouldUseEpicOnlineSystems)
                {
                    return (PlatformSystems.lobbyManager as EOSLobbyManager).GetUserDisplayNameFromProductIdString(self.nameOverride);
                }
                return self.nameOverride;
            }

            //Get username from LobbyManager
            string resolvedName = string.Empty;
            LobbyManager lobbyManager;
            if (PlatformSystems.ShouldUseEpicOnlineSystems)
            {
                lobbyManager = PlatformSystems.lobbyManager as EOSLobbyManager;
            }
            else
            {
                lobbyManager = PlatformSystems.lobbyManager as SteamworksLobbyManager;
            }
            UserID user = new UserID(self.steamId);
            resolvedName = lobbyManager.GetUserDisplayName(user);

            //If the LobbyManager can't find a username, then directly check here.
            if (string.IsNullOrEmpty(resolvedName))
            {
                //Copy this fix from the FixGetUserDisplayName Hook
                if (Client.Instance != null)
                {
                    resolvedName = Client.Instance.Friends.GetName(self.steamId.steamValue);
                }

                //if It's still no username by this point, just say ???
                if (string.IsNullOrEmpty(resolvedName)) resolvedName = "???";
            }

            return resolvedName;
        }
    }
}
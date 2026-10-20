using CodeRebirth.src.Content.Maps;
using Dawn.Utils;
using GameNetcodeStuff;
using LethalNetworkAPI;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CoolerOxydeShip
{
    public static class OxydeShipNetwork
    {
        public static LNetworkMessage<MuteShipMessage> MuteShipRequest;
        public static LNetworkMessage<MuteShipMessage> MuteShipBroadcast;
        public static LNetworkMessage<ShipPositionMessage> ShipPositionBroadcast;
        public static LNetworkMessage<MistressKillMessage> MistressKillBroadcast;
        public static readonly Dictionary<ulong, Vector3> ShipPositions = new();
        public static void Init()
        {
            MuteShipRequest = LNetworkMessage<MuteShipMessage>.Create("OxydeMuteShipRequest", onServerReceived: OnMuteShipRequest);
            MuteShipBroadcast = LNetworkMessage<MuteShipMessage>.Create("OxydeMuteShipBroadcast", onClientReceived: OnMuteShipReceived);
            ShipPositionBroadcast = LNetworkMessage<ShipPositionMessage>.Create("OxydeShipPosition", onClientReceived: OnShipPositionReceived);
            MistressKillBroadcast = LNetworkMessage<MistressKillMessage>.Create("MistressRealKill", onClientReceived: OnMistressKillReceived);
        }
        private static void OnMuteShipRequest(MuteShipMessage message, ulong senderClientId)
        {
            if (StartOfRound.Instance == null) return;
            if (!NetworkManager.Singleton.IsServer) return;
            MuteShipBroadcast.SendClients(message, NetworkManager.Singleton.ConnectedClientsIds.ToArray());
            OnMuteShipReceived(message);
        }
        private static void OnMuteShipReceived(MuteShipMessage message)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(message.shipNetworkId, out NetworkObject networkObject)) return;
            OxydeCrashingShip ship = networkObject.GetComponent<OxydeCrashingShip>();
            if (ship == null) return;
            CoolerOxydeShipPatch.OxydeCrashingShipImpactPatch.StopAlarm(ship);
        }
        public static void SendMuteShip(ulong shipNetworkId)
        {
            if (!NetworkManager.Singleton.IsListening) return;
            MuteShipRequest.SendServer(new MuteShipMessage { shipNetworkId = shipNetworkId });
        }
        public static void SendShipPosition(ulong shipNetworkId, Vector3 position)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            ShipPositions[shipNetworkId] = position;
            ShipPositionBroadcast.SendClients(new ShipPositionMessage{shipNetworkId = shipNetworkId,x = position.x,y = position.y,z = position.z}, NetworkManager.Singleton.ConnectedClientsIds.ToArray());
        }
        private static void OnShipPositionReceived(ShipPositionMessage message)
        {
            ShipPositions[message.shipNetworkId] = new Vector3(message.x, message.y, message.z);
            CoolerOxydeShipPatch.Logger.LogDebug($"Received ship position: {ShipPositions[message.shipNetworkId]}");
        }
        public static void SendMistressKill(ulong playerId)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            MistressKillBroadcast.SendClients(new MistressKillMessage { playerId = playerId }, NetworkManager.Singleton.ConnectedClientsIds.ToArray());
            OnMistressKillReceived(new MistressKillMessage { playerId = playerId });
        }
        private static void OnMistressKillReceived(MistressKillMessage message)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerClientId != message.playerId) continue;
                if (!player.IsLocalPlayer()) return;
                CoolerOxydeShipPatch.Logger.LogDebug("Mistress killing local player!");
                player.KillPlayer(Vector3.zero, false, CauseOfDeath.Snipping, 0);
                return;
            }
        }
    }
    public class MuteShipMessage
    {
        public ulong shipNetworkId;
    }
    public class ShipPositionMessage
    {
        public ulong shipNetworkId;
        public float x;
        public float y;
        public float z;
    }
    public class MistressKillMessage
    {
        public ulong playerId;
    }
}
using Godot;
using System;

public partial class Packets
{
    public static void HandlePackets(ByteBuf b, NetworkManager net)
    {

        byte t = b.ReadByte();

        // common variables
        int incomingUid;
        string incomingUsername;
        int incomingModelIdx;
        bool incomingHeadlightsState;

        long pingStartTime;
        long serverReceivedPingTime;

        Kart remoteKart;

        switch (t) // handle each type of incoming packet
        {
            case 0:
                break;
            case 1: // get uid
                net.LocalUid = b.ReadInt();
                net.NetLog($"Received UID ~ {net.LocalUid}");
                break;
            case 2: // other player connection
                incomingUid = b.ReadInt();

                NetworkManager.NetworkPlayerData player = new()
                {
                    PeerUid = incomingUid,
                    Username = "",
                    ModelIndex = 0
                };

                net.NetworkedPlayers[incomingUid] = player;
                net.NetLog($"Player {incomingUid} Connected");

                GlobalManager.Instance.Lobby.CreateNewKartFromUid(incomingUid, new Vector3(-280, 50, 30));
                break;
            case 3: // other player disconnection
                incomingUid = b.ReadInt();
                net.NetworkedPlayers.Remove(incomingUid);
                net.NetLog($"Player {incomingUid} Disconnected");

                GlobalManager.Instance.Lobby.RemoveKart(incomingUid);
                break;
            case 4: // remote visual update
                incomingUid = b.ReadInt();
                incomingUsername = b.ReadStringUtf8();
                incomingModelIdx = b.ReadInt();
                incomingHeadlightsState = b.ReadBool();

                remoteKart = GlobalManager.Instance.Lobby.RemoteKarts[incomingUid];
                remoteKart.NetworkData.Username = incomingUsername;
                remoteKart.NetworkData.ModelIndex = incomingModelIdx;
                remoteKart.NetworkData.HeadlightsState = incomingHeadlightsState;

                remoteKart.SetupModel();

                break;
            case 5: // server shutdown - doesnt work rn

                net.NetLog("Server Shutdown");
                foreach (var kvp in NetworkManager.Instance.NetworkedPlayers)
                {
                    GlobalManager.Instance.Lobby.RemoveKart(kvp.Key);
                }
                NetworkManager.Instance.NetworkedPlayers.Clear();

                NetworkManager.Instance.Disconnect(false);
                break;
            case 6:
                incomingUid = b.ReadInt();

                // position and rotation set
                GlobalManager.Instance.Lobby.RemoteKarts[incomingUid].GlobalPosition = b.ReadVec3();
                GlobalManager.Instance.Lobby.RemoteKarts[incomingUid].ModelRotation = b.ReadVec3();
                break;
            case 7: // ping
                pingStartTime = b.ReadLong();

                net.LastLatency = DateTimeOffset.Now.ToUnixTimeMilliseconds() - pingStartTime;
                break;
            case 8:
                GlobalManager.Instance.Chat.Messages.Enqueue(new Chat.Msg { From = b.ReadStringUtf8(), Content = b.ReadStringUtf8() });
                break;
        }
    }


    // C2S Packets
    public static ByteBuf Packet_Disconnect()
    {
        var b = ByteBuf.WriteEmpty();
        b.WriteByte(1);
        return b;
    }
    public static ByteBuf Packet_Msg(string from, string msg)
    {
        var b = ByteBuf.WriteEmpty();
        b.WriteByte(2);
        b.WriteStringUtf8(from);
        b.WriteStringUtf8(msg);
        return b;
    }
    public static ByteBuf Packet_AskForUid()
    {
        var b = ByteBuf.WriteEmpty();
        b.WriteByte(3);
        return b;
    }
    public static ByteBuf Packet_UpdateServerOnPlayerVisuals(Kart kart)
    {
        var b = ByteBuf.WriteEmpty();
        b.WriteByte(4);
        b.WriteStringUtf8(kart.NetworkData.Username);
        b.WriteInt(kart.NetworkData.ModelIndex);
        b.WriteBool(kart.NetworkData.HeadlightsState);
        return b;
    }
    public static ByteBuf Packet_UpdateServerOnPosition(Kart kart)
    {
        var b = ByteBuf.WriteEmpty();
        b.WriteByte(5);

        b.WriteVec3(kart.GlobalPosition);
        b.WriteVec3(kart.ModelRotation);

        return b;
    }
    public static ByteBuf Packet_Ping()
    {
        var b = ByteBuf.WriteEmpty();
        b.WriteByte(6);

        b.WriteLong(DateTimeOffset.Now.ToUnixTimeMilliseconds());

        return b;
    }

    public static ByteBuf Packet_Login()
    {
        var b = ByteBuf.WriteEmpty();
        b.WriteByte(7);
        return b;
    }
}

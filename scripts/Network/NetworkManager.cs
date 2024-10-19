using Godot;
using System.Collections.Generic;

public partial class NetworkManager : Node
{

    [Signal] public delegate void DisconnectedEventHandler();
    [Signal] public delegate void ConnectedEventHandler();
    [Signal] public delegate void RecvEventHandler(byte[] buf);

    public static NetworkManager Instance { get; private set; }

    public Dictionary<int, NetworkPlayerData> NetworkedPlayers { get; private set; }


    public override void _Ready()
    {
        Instance = this;
        NetworkedPlayers = new Dictionary<int, NetworkPlayerData>();
    }

    public override void _Process(double delta)
    {
        
    }

    


    public void Connect(string ip, int port)
    {
        return;
    }

    public void Disconnect()
    {
        return;
    }

    public void Send(ByteBuf buf)
    {
        return;
    }


    //
    public bool HasControlAuthority(NetworkPlayerData networkData)
    {
        return true; // temp
    }


    public void NetLog(string s)
    {
        GD.Print("Network:" + s);
    }

    // PEERS
    public struct NetworkPlayerData
    {
        public int PeerUid;

        public string Username;
        public int ModelIndex;
        public bool HeadlightsState;
    }
}

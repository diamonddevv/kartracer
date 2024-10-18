using Godot;
using System.Collections.Generic;

public partial class NetworkManager : Node
{

    [Signal] public delegate void DisconnectedEventHandler();
    [Signal] public delegate void ConnectedEventHandler();
    [Signal] public delegate void RecvEventHandler(byte[] buf);

    public static NetworkManager Instance { get; private set; }

    public Dictionary<int, NetworkPlayerData> NetworkedPlayers { get; private set; }

    public PacketPeerUdp Peer;
    public bool ConnectedToServer;
    public string ServerIp;
    public int ServerPort;
    public int LocalUid;

    public long LastLatency;

    public int PacketCount;

    private bool _lastStatus;

    public override void _Ready()
    {
        Instance = this;
        NetworkedPlayers = new Dictionary<int, NetworkPlayerData>();

        DebugOverlay.Instance.DebugLines["net_secheader"] = "\n-- Network --";
        DebugOverlay.Instance.DebugLines["status"] = "";
        DebugOverlay.Instance.DebugLines["server_address"] = "";
        DebugOverlay.Instance.DebugLines["local_uid"] = "";
        DebugOverlay.Instance.DebugLines["ping"] = "";
        DebugOverlay.Instance.DebugLines["playercount"] = "";
        DebugOverlay.Instance.DebugLines["unprocessed_packets"] = "";

        Connected += () =>
        {
            NetLog("Asking for Uid");
            Send(Packets.Packet_AskForUid());
        };

        Recv += buf => Packets.HandlePackets(ByteBuf.ReadFrom(buf), this);


        // ping timer
        Timer timer = new Timer();
        timer.Autostart = true;
        timer.WaitTime = 3;
        timer.Timeout += () => Send(Packets.Packet_Ping());
        AddChild(timer);
    }

    public override void _Process(double delta)
    {
        DebugOverlay.Instance.DebugLines["status"] = "Status: " + _lastStatus.ToString();
        DebugOverlay.Instance.DebugLines["server_address"] = "Server Address: " + $"{ServerIp}:{ServerPort}";
        DebugOverlay.Instance.DebugLines["local_uid"] = "Network UID: " + (ConnectedToServer ? LocalUid : "N/A");
        DebugOverlay.Instance.DebugLines["ping"] = "Latency (Ping): " + LastLatency + "ms";
        DebugOverlay.Instance.DebugLines["playercount"] = "Player Count: " + (NetworkedPlayers.Count + 1); // add one for self
        DebugOverlay.Instance.DebugLines["unprocess_packets"] = "Unprocessed Packets: " + PacketCount;

        if (Peer == null) return;

        ConnectedToServer = Peer.IsSocketConnected();

        if (ConnectedToServer != _lastStatus)
        {
            switch (ConnectedToServer)
            {
                case false: // disconnected
                    NetLog("Disconnected");
                    EmitSignal(SignalName.Disconnected);
                    break;
                case true: // connected
                    NetLog("Connected");
                    EmitSignal(SignalName.Connected);
                    break;
            }
        }
        _lastStatus = ConnectedToServer;


        if (ConnectedToServer)
        {
            PacketCount = Peer.GetAvailablePacketCount();
            if (PacketCount > 0)
            {
                var data = Peer.GetPacket(); // only get what we need
                EmitSignal(SignalName.Recv, data);
            }
        }
    }

    public bool HasControlAuthority(NetworkPlayerData networkData)
    {
        return networkData.PeerUid == LocalUid || !ConnectedToServer;
    }


    public void Connect(string ip, int port)
    {
        Peer = new PacketPeerUdp();
        Peer.ConnectToHost(ip, port);
        Peer.Bind(port, ip, ByteBuf.PACKET);

        ServerIp = ip;
        ServerPort = port;
    }

    public void Disconnect(bool sendPacket = true)
    {
        // send dc packet
        if (sendPacket) Send(Packets.Packet_Disconnect());

        // remove peer
        Peer.Close();
        Peer = null;

        // reset other vars
        ServerIp = "";
        ServerPort = 0;
    }

    public void Send(ByteBuf buf)
    {
        if (Peer == null) return;

        byte[] b = buf.Compiled();
        Peer.PutPacket(b);
    }


    //
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

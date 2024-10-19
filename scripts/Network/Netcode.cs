using Godot;
using System;
using System.Collections.Generic;

public partial class Netcode : Node
{

    public struct Packet
    {
        public Action<ByteBuf, object[]> Build;
        public Action<ByteBuf> Handle;
    }

    public static List<Packet> S2C;
    public static List<Packet> C2S;


    // s2c
    public static Packet S2C_PING = new()
    {
        Build = (b, p) => b.WriteLong((long)p[0]),
        Handle = b => { },
    };

    // c2s
    public static Packet C2S_PING = new()
    {
        Build = (b, p) => b.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
        Handle = b => S2C_PING.Build(ByteBuf.WriteEmpty(), new object[] { b.ReadLong() })
    };

    public override void _Ready()
    {
        S2C = new()
        {
            S2C_PING
        };

        C2S = new()
        {
            C2S_PING
        };
    }

    public static void RawHandle(ByteBuf buf, bool isServer = false)
    {
        int type = buf.ReadByte() - 1;
        if (type < 0) return; // invalid; packet 0 doesnt exist but idx 0 does

        if (isServer)
        {
            C2S[type].Handle(buf);
        } else
        {
            S2C[type].Handle(buf);
        }
    }
}

using Godot;
using System;
using System.Text;

public partial class ByteBuf
{
    public const int PACKET = 512;

    public const int SIZE_BYTE = 1;
    public const int SIZE_BOOL = 1;
    public const int SIZE_INT = 4;
    public const int SIZE_FLOAT = 4;
    public const int SIZE_LONG = 8;

    public enum Mode
    {
        Read, Write
    }

    private static readonly InvalidOperationException BAD_READ = new InvalidOperationException("Tried to read on write-mode buf.");
    private static readonly InvalidOperationException BAD_WRITE = new InvalidOperationException("Tried to write on read-mode buf.");

    public Mode _mode;
    private int _idx;
    private byte[] _buf;

    private ByteBuf(byte[] buf, Mode mode)
    {
        _idx = 0;
        _buf = buf;
        _mode = mode;
    }

    public static ByteBuf ReadFrom(byte[] bytes)
    {
        return new ByteBuf(bytes, Mode.Read);
    }
    public static ByteBuf ReadEmpty()
    {
        return ReadFrom(new byte[PACKET]);
    }
    public static ByteBuf WriteEmpty()
    {
        return new ByteBuf(new byte[PACKET], Mode.Write);
    }

    public byte[] Compiled()
    {
        return _buf;
    }

    //

    public void RawWrite(byte[] b)
    {
        if (_mode == Mode.Read) throw BAD_READ;

        b.CopyTo(_buf, _idx);
        _idx += b.Length;
    }

    public byte[] RawRead(int size)
    {
        if (_mode == Mode.Write) throw BAD_WRITE;

        byte[] b = new byte[size];
        Array.Copy(_buf, _idx, b, 0, size);
        _idx += size;

        return b;
    }

    // simple datatypes

    public void WriteByte(byte b) => RawWrite(new[] { b });
    public byte ReadByte() => RawRead(SIZE_BYTE)[0];

    public void WriteInt(int b) => RawWrite(BitConverter.GetBytes(b));
    public int ReadInt() => BitConverter.ToInt32(RawRead(SIZE_INT));

    public void WriteFloat(float b) => RawWrite(BitConverter.GetBytes(b));
    public float ReadFloat() => BitConverter.ToSingle(RawRead(SIZE_FLOAT));

    public void WriteBool(bool b) => RawWrite(BitConverter.GetBytes(b));
    public bool ReadBool() => BitConverter.ToBoolean(RawRead(SIZE_BOOL));

    public void WriteLong(long b) => RawWrite(BitConverter.GetBytes(b));
    public long ReadLong() => BitConverter.ToInt64(RawRead(SIZE_LONG));


    // complex datatypes
    public void WriteStringUtf8(string s)
    {
        int l = s.Length;
        byte[] b = Encoding.UTF8.GetBytes(s);

        WriteInt(l);
        RawWrite(b);
    }
    public string ReadStringUtf8()
    {
        int l = ReadInt();
        byte[] b = RawRead(l);

        return Encoding.UTF8.GetString(b);
    }


    public void WriteVec3(Vector3 vec)
    {
        WriteFloat(vec.X);
        WriteFloat(vec.Y);
        WriteFloat(vec.Z);
    }

    public Vector3 ReadVec3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    }
}
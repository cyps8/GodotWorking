using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;

public enum ServerPackets
{
    welcome = 1,
    playerDisconnected,
    chatMsg,
    newPlayer,
    playerMovement
}

public enum ClientPackets
{
    welcomeReceived = 1,
    chatMsg,
    playerMovement,
    ignoreNext
}

public class Packet : IDisposable
{
    private List<byte> buffer;
    private byte[] readableBuffer;
    private int readPos;

    public Packet()
    {
        buffer = new List<byte>();
        readPos = 0;
    }

    public Packet(int _id)
    {
        buffer = new List<byte>();
        readPos = 0;

        Write(_id);
    }

    public Packet(byte[] _data)
    {
        buffer = new List<byte>();
        readPos = 0;

        SetBytes(_data);
    }

    public void SetBytes(byte[] _data)
    {
        Write(_data);
        readableBuffer = buffer.ToArray();
    }

    public void WriteLength()
    {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
    }

    public void InsertInt(int _value)
    {
        buffer.InsertRange(0, BitConverter.GetBytes(_value));
    }

    public byte[] ToArray()
    {
        readableBuffer = buffer.ToArray();
        return readableBuffer;
    }

    public int Length()
    {
        return buffer.Count;
    }

    public int UnreadLength()
    {
        return Length() - readPos;
    }

    public void Reset(bool _shouldReset = true)
    {
        if (_shouldReset)
        {
            buffer.Clear();
            readableBuffer = null;
            readPos = 0;
        }
        else
        {
            readPos -= 4;
        }        
    }

    public void Write(byte _value)
    {
        buffer.Add(_value);
    }

    public void Write(byte[] _value)
    {
        buffer.AddRange(_value);
    }

    public void Write(int _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    public void Write(float _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    public void Write(long _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    public void Write(bool _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    public void Write(string _value)
    {
        Write(_value.Length);
        buffer.AddRange(Encoding.UTF8.GetBytes(_value));
    }

    public void Write(Vector2 _value)
    {
        Write(_value.x);
        Write(_value.y);
    }

    public byte ReadByte(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            byte _value = readableBuffer[readPos];
            if (_moveReadPos)
            {
                readPos += 1;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte'");
        }
    }

     public byte[] ReadBytes(int _length, bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            byte[] _value = buffer.GetRange(readPos, _length).ToArray();
            if (_moveReadPos)
            {
                readPos += _length;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte[]'");
        }
    }

    public int ReadInt(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            int _value = BitConverter.ToInt32(readableBuffer,readPos);
            if (_moveReadPos)
            {
                readPos += 4;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'int'");
        }
    }

    public float ReadFloat(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            float _value = BitConverter.ToSingle(readableBuffer,readPos);
            if (_moveReadPos)
            {
                readPos += 4;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'float'");
        }
    }
    public long ReadLong(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            long _value = BitConverter.ToInt64(readableBuffer, readPos);
            if (_moveReadPos)
            {
                readPos += 8;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'long'!");
        }
    }

    public bool ReadBool(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            bool _value = BitConverter.ToBoolean(readableBuffer,readPos);
            if (_moveReadPos)
            {
                readPos += 1;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'bool'");
        }
    }

    public string ReadString(bool _moveReadPos = true)
    {
        try
        {
            int _length = ReadInt();
            string _value = Encoding.UTF8.GetString(readableBuffer, readPos, _length);
            if (_moveReadPos && _value.Length > 0)
            {
                readPos += _length;
            }
            return _value;
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'");
        }
    }

    public Vector2 ReadVector2(bool _moveReadPos = true)
    {
        return new Vector2(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
    }

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                buffer = null;
                readableBuffer = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

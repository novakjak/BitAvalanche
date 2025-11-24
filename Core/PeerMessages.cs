using System;

// TODO: implement bitfield

public static class PeerMessageParser
{
	public static IPeerMessage? Parse(byte[] message)
	{
        var len = Util.FromNetworkOrderBytes(message, 0);
		if (len == 0)
			return new KeepAlive();
		var type = message[4];
		switch (type)
		{
			case 0: {
				if (len != 1) return null;
				return new Choke();
			}
			case 1: {
				if (len != 1) return null;
				return new Unchoke();
			}
			case 2: {
				if (len != 1) return null;
				return new Interested();
			}
			case 3: {
				if (len != 1) return null;
				return new NotInterested();
			}
			case 4: {
				if (len != 5) return null;
		        var idx = Util.FromNetworkOrderBytes(message, 5);
				return new Have(idx);
			}
			case 5: {
				// TODO: implement bitfield
				return null;
			}
			case 6: {
				if (len != 13) return null;
		        var idx = Util.FromNetworkOrderBytes(message, 5);
		        var begin = Util.FromNetworkOrderBytes(message, 9);
		        var length = Util.FromNetworkOrderBytes(message, 13);
				return new Request(idx, begin, length);
			}
			case 7: {
				if (len <= 9) return null;
		        var idx = Util.FromNetworkOrderBytes(message, 5);
		        var begin = Util.FromNetworkOrderBytes(message, 9);
				if (message.Length != len + 4) return null;
				var chunkLen = len - 9;
				var chunk = new byte[chunkLen];
				Array.Copy(message, 13, chunk, 0, chunkLen);
				return new Piece(idx, begin, chunk);
			}
			case 8: {
				if (len != 13) return null;
		        var idx = Util.FromNetworkOrderBytes(message, 5);
		        var begin = Util.FromNetworkOrderBytes(message, 9);
		        var length = Util.FromNetworkOrderBytes(message, 13);
				return new Cancel(idx, begin, length);
			}
		}
		return null;
	}
}

public interface IPeerMessage
{
	public byte[] ToBytes();
}

public class KeepAlive : IPeerMessage
{
	public byte[] ToBytes() => [0, 0, 0, 0];
}
public class Choke : IPeerMessage
{
	public byte[] ToBytes() => [0, 0, 0, 1, 0];
}
public class Unchoke : IPeerMessage
{
	public byte[] ToBytes() => [0, 0, 0, 1, 1];
}
public class Interested : IPeerMessage
{
	public byte[] ToBytes() => [0, 0, 0, 1, 2];
}
public class NotInterested : IPeerMessage
{
	public byte[] ToBytes() => [0, 0, 0, 1, 3];
}
public class Have : IPeerMessage
{
	public UInt32 Piece { get; set; }
	public byte[] ToBytes()
	{
		var buf = new byte[9];
		byte[] header = [0, 0, 0, 5, 4];
		header.CopyTo(buf, 0);
		Util.GetNetworkOrderBytes(Piece).CopyTo(buf, 5);
		return buf;
	}
	public Have(UInt32 piece)
	{
		Piece = piece;
	}
	
}
public class Request : IPeerMessage
{
	public UInt32 Idx { get; set; }
	public UInt32 Begin { get; set; }
	public UInt32 Length { get; set; }
	public byte[] ToBytes()
	{
		var buf = new byte[17];
		byte[] header = [0, 0, 0, 13, 6];
		header.CopyTo(buf, 0);
		Util.GetNetworkOrderBytes(Idx).CopyTo(buf, 5);
		Util.GetNetworkOrderBytes(Begin).CopyTo(buf, 9);
		Util.GetNetworkOrderBytes(Length).CopyTo(buf, 13);
		return buf;
	}
	public Request(UInt32 idx, UInt32 begin, UInt32 length)
	{
		Idx = idx;
		Begin = begin;
		Length = length;
	}
}
public class Piece : IPeerMessage {
	public UInt32 Idx { get; set; }
	public UInt32 Begin { get; set; }
	public byte[] Chunk { get; set; }
	public byte[] ToBytes()
	{
		var buf = new byte[13 + Chunk.Length];
		Util.GetNetworkOrderBytes(9 + (UInt32)Chunk.Length).CopyTo(buf, 0);
		buf[4] = 7;
		Util.GetNetworkOrderBytes(Idx).CopyTo(buf, 5);
		Util.GetNetworkOrderBytes(Begin).CopyTo(buf, 9);
		Chunk.CopyTo(buf, 13);
		return buf;
	}
	public Piece(UInt32 idx, UInt32 begin, byte[] chunk)
	{
		Idx = idx;
		Begin = begin;
		Chunk = chunk;
	}
}
public class Cancel : IPeerMessage
{
	public UInt32 Idx { get; set; }
	public UInt32 Begin { get; set; }
	public UInt32 Length { get; set; }
	public byte[] ToBytes()
	{
		var buf = new byte[17];
		byte[] header = [0, 0, 0, 13, 6];
		header.CopyTo(buf, 0);
		Util.GetNetworkOrderBytes(Idx).CopyTo(buf, 5);
		Util.GetNetworkOrderBytes(Begin).CopyTo(buf, 9);
		Util.GetNetworkOrderBytes(Length).CopyTo(buf, 13);
		return buf;
	}
	public Cancel(UInt32 idx, UInt32 begin, UInt32 length)
	{
		Idx = idx;
		Begin = begin;
		Length = length;
	}
}

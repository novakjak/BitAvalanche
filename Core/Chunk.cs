using System;
public sealed class Chunk
{
	public UInt32 Idx { get; }
	public UInt32 Begin { get; }
	public byte[] Data { get; }

	public Chunk(UInt32 idx, UInt32 begin, byte[] data)
	{
		Idx = idx;
		Begin = begin;
		Data = data;
	}
}

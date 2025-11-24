using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bittorrent.Models;

public class PeerConnection
{
    public Peer Peer { get; private set; }
    public byte[] InfoHash { get; private set; }
    public byte[] PeerId { get; private set; }

    private bool _amChoked = true;
    private bool _isChoked = true;
    private bool _amInterested = false;
    private bool _isInterested = false;

    private readonly TcpClient _client;

    private PeerConnection(Peer peer, byte[] infoHash, byte[] peerId)
    {
        if (infoHash.Length != 20)
        {
            throw new ArgumentException("Info hash is of incorrect length.");
        }
        if (peerId.Length != 20)
        {
            throw new ArgumentException("Peer id is of incorrect length.");
        }

        Peer = peer;
        InfoHash = infoHash;
        PeerId = peerId;
        _client = new TcpClient();
    }

    public async Task<IEnumerable<IPeerMessage>> RecieveMessages()
    {
        var stream = _client.GetStream();
        var messages = new List<IPeerMessage>();
        while (stream.DataAvailable)
        {
            var lenBuf = new byte[4];
            await stream.ReadExactlyAsync(lenBuf, 0, 4);
            var len = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf));
            var msgBuf = new byte[4 + len];
            lenBuf.CopyTo(msgBuf, 0);
            await stream.ReadExactlyAsync(msgBuf, 4, len);
            var msg = PeerMessageParser.Parse(msgBuf);
            if (msg is null) continue;
            messages.Add(msg);
        }
        return messages;
    }

    public static async Task<PeerConnection?> CreateAsync(Peer peer, byte[] infoHash, byte[] peerId)
    {
        var pc = new PeerConnection(peer, infoHash, peerId);
        try
        {
            await pc._client.ConnectAsync(pc.Peer.Ip, pc.Peer.Port);
            await pc.HandShake();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Handshake failed for peer {peer}: {e.Message}");
            return null;
        }
        return pc;
    }

    private async Task HandShake()
    {
        var stream = _client.GetStream();
        byte[] protocolName = "BitTorrent protocol"u8.ToArray();
        var len = protocolName.Length;
        byte[] reserved = new byte[8];

        List<byte> buffer = new();
        buffer.Add((byte)protocolName.Length);
        buffer.AddRange(protocolName);
        buffer.AddRange(reserved);
        buffer.AddRange(InfoHash);
        buffer.AddRange(PeerId);

        await stream.WriteAsync(buffer.ToArray());
        await stream.FlushAsync();

        // var nread = 0;
        // while (nread < handshake.Length)
        // {
        //     nread += await stream.ReadAsync(handshake);
        //     Console.WriteLine(nread);
        // }
        var messageBuf = new Byte[49 + len];
        var handshake = new Memory<byte>(messageBuf);
        await stream.ReadAtLeastAsync(messageBuf, 4);

        Console.WriteLine(handshake.Span[0]);
        Console.WriteLine(handshake.Span[1]);
        Console.WriteLine(handshake.Span[2]);
        Console.WriteLine(handshake.Span[3]);
        if (handshake.Span[0] != len)
            throw new HandShakeException("Recieved invalid protocol name length from peer.");
        if (handshake.Slice(1, len).ToArray() != protocolName)
            throw new HandShakeException("Recieved invalid protocol name from peer.");
        // Skip checking reserved bytes (20..27)
        if (handshake.Slice(1 + len + 8, 20).ToArray() != InfoHash)
            throw new HandShakeException("Info hash recieved from peer differs from the one sent.");
        if (Peer.PeerId is not null && handshake.Slice(1 + len + 8 + 20, 20).ToArray() != Peer.PeerId)
            throw new HandShakeException("Peer's id mismatched.");
    }

    ~PeerConnection()
    {
        _client.Dispose();
    }
}

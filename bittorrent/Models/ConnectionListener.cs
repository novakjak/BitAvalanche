using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using bittorrent.Core;
using bittorrent.Models;

namespace bittorrent.Models;

public class ConnectionListener
{
	public event EventHandler<(TcpClient, Peer, byte[] InfoHash)>? NewPeer;

	public int Port { get; set; }

	private TcpListener _listener;
	private Task _listenerTask;
    private CancellationTokenSource _cancellation = new();

	public ConnectionListener(int port)
	{
		Port = port;
		_listener = TcpListener.Create(port);
	}

	public ConnectionListener(TcpListener listener)
	{
		_listener = listener;
		Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
	}

	public void Start()
	{
		_listenerTask ??= Task.Run(async () => await Listen(), _cancellation.Token);
	}

	public async Task Listen()
	{
		_listener.Start();
		while (!_cancellation.IsCancellationRequested)
		{
			var client = await _listener.AcceptTcpClientAsync(_cancellation.Token);
	        var messageBuf = new Byte[Handshake.MessageLength];
	        var messageMem = new Memory<byte>(messageBuf);
	        await client
				.GetStream()
				.ReadExactlyAsync(messageBuf, 0, messageBuf.Length, _cancellation.Token);
	        var handshake = Handshake.Parse(messageMem);
			var clientEndPoint = (IPEndPoint)client.Client.LocalEndPoint;
			var peer = new Peer(clientEndPoint.Address, clientEndPoint.Port, handshake.PeerId);
			var args = (client, peer, handshake.InfoHash);
			NewPeer?.Invoke(this, args);
		}
		_listener.Stop();
	}

	~ConnectionListener()
	{
		_cancellation.Cancel();
		_cancellation.Dispose();
		_listener.Dispose();
	}
}

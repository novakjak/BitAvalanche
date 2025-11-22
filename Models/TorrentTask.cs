using System;
using System.Threading.Tasks;
using BT = BencodeNET.Torrents;

namespace bittorrent.Models;

public class TorrentTask
{
    public BT.Torrent Torrent { get; private set; }

    public int PeerCount { get; private set; } = 0;

    public TorrentTask(BT.Torrent torrent)
    {
        Torrent = torrent;
    }
}

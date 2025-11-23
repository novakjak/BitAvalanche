using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using BencodeNET.Parsing;
using BencodeNET.Objects;
using BT = BencodeNET.Torrents;

namespace bittorrent.Models;

public class TorrentTask
{
    private static readonly HttpClient client = new();

    public BT.Torrent Torrent { get; private set; }
    public int PeerCount { get; private set; } = 0;
    public int Uploaded { get; private set; } = 0;
    public int Downloaded { get; private set; } = 0;
    public int DownloadedValid { get; private set; } = 0;


    private Thread? _thread;
    private string _peerId = Util.GenerateRandomString(20);

    public void Start()
    {
        _thread ??= new Thread(new ThreadStart(this.Work));
        _thread.Start();
    }

    public TorrentTask(BT.Torrent torrent)
    {
        Torrent = torrent;
    }

    private async void Work()
    {
        var tracker = Torrent.Trackers[0][0];
        var urlencoded = System.Web.HttpUtility.UrlEncode(Torrent.OriginalInfoHashBytes);

        var query = $"info_hash={urlencoded}";
        query += $"&peer_id={_peerId}";
        query += $"&port=8085";
        query += $"&downloaded={Downloaded}";
        query += $"&uploaded={Uploaded}";
        query += $"&left={Torrent.TotalSize - DownloadedValid}";

        var message = new HttpRequestMessage(HttpMethod.Get, $"{tracker}?{query}");
        
        // TODO: exception handling
        BDictionary body;
        try
        {
            using var response = await client.SendAsync(message);
            var parser = new BencodeParser();
            body = parser.Parse<BDictionary>(await response.Content.ReadAsStreamAsync());
            BString failure;
            if ((failure = body.Get<BString>(new BString("failure reason"))) is not null)
            {
                Console.WriteLine(failure.ToString());
                return;
            }
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("bad :(");
                return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        foreach (var key in body.Keys)
            Console.WriteLine(key);
        // foreach (var p in body.Get<BList>("peers"))
        //     Console.WriteLine(p);
        foreach (var p in ParsePeers(body.Get<BList>("peers")))
            Console.WriteLine(p.PeerId);
    }

    private static IEnumerable<Peer> ParsePeers(BList peersList)
    {
        if (peersList is null)
        {
            Console.WriteLine("no peers");
            return new List<Peer>();
        }

        List<Peer> peers = new();
        foreach (var peerDict in peersList.Value)
        {
            if (peerDict is not BDictionary peer)
                continue;
            var id = peer.Get<BString>("id").ToString();
            var ip = peer.Get<BString>("ip");
            var port = peer.Get<BNumber>("port").Value;
            Console.WriteLine(id);
        }
        return peers;
    }
    private static string InterspersePercent(string input)
    {
        string res = "";
        for (int i = 0; i < input.Length; i++)
        {
            if (i % 2 == 0)
            {
                res += "%";
            }
            res += input[i];
        }
        return res;
    }
}

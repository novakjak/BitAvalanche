using System;
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
        var urlencoded = System.Net.WebUtility.UrlEncodeToBytes(Torrent.OriginalInfoHashBytes, 0, 20);
        var infoHash = System.Text.Encoding.Default.GetString(urlencoded);

        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        query.Add("info_hash", infoHash);
        query.Add("peer_id", _peerId);
        query.Add("port", 8085.ToString());

        var uri = new Uri($"{tracker}?{query}");
        Console.WriteLine(uri);
        var message = new HttpRequestMessage(HttpMethod.Get, uri);
        
        // TODO: exception handling
        using var response = await client.SendAsync(message);
        var parser = new BencodeParser();
        System.Console.WriteLine(await response.Content.ReadAsStringAsync());
        var body = parser.Parse<BDictionary>(await response.Content.ReadAsStreamAsync());

        // System.Console.WriteLine(body.Get<BNumber>(new BString("complete")).Value);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("bad :(");
            return;
        }
        Console.WriteLine(response.StatusCode);
    }
}

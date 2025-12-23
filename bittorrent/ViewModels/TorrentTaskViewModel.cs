using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using BT = BencodeNET.Torrents;
using bittorrent.Models;

namespace bittorrent.ViewModels;

public partial class TorrentTaskViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private byte[] _infoHash = new byte[0];

    [ObservableProperty]
    private double _percentComplete = 0.0;

    public TorrentTask Task { get; private set; }

    public TorrentTaskViewModel(BT.Torrent t)
    {
        Name = t.DisplayName;
        InfoHash = t.OriginalInfoHashBytes;
        Task = new TorrentTask(t);
        Task.DownloadedPiece += HandleDownloadedPiece;
        Task.Start();
    }

    public void HandleDownloadedPiece(object? sender, (int pieceIdx, double completion) args)
    {
        PercentComplete = args.completion;
    }
}

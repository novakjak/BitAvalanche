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
    private double _percentComplete = 0.0;

    private TorrentTask _task;

    private TorrentTaskViewModel(BT.Torrent t, TorrentTask task)
    {
        _name = t.DisplayName;
        _task = task;
        _task.Start();
    }
    public static async Task<TorrentTaskViewModel> CreateAsync(BT.Torrent t)
    {
        var task = await TorrentTask.CreateAsync(t);
        var viewModel = new TorrentTaskViewModel(t, task);
        System.Console.WriteLine("added viewmodel");
        return viewModel;
    }
}

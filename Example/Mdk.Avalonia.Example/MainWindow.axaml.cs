using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MDK.SDK.NET;

namespace Mdk.Avalonia.Example;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private static FilePickerFileType VideoFileType { get; } = new("Video Files")
    {
        Patterns = ["*.mp4", "*.avi", "*.mkv", "*.wmv", "*.mov", "*.flv", "*.webm", "*.mkv"],
        AppleUniformTypeIdentifiers = ["public.video"],
        MimeTypes = ["video/*"]
    };

    private static FilePickerFileType AudioFileType { get; } = new("Audio Files")
    {
        Patterns = ["*.mp3", "*.wav", "*.ogg", "*.flac", "*.m4a", "*.aac", "*.wma"],
        AppleUniformTypeIdentifiers = ["public.audio"],
        MimeTypes = ["audio/*"]
    };

    /// <summary>
    ///     Play or Pause the video
    /// </summary>
    private void Play(object? sender, RoutedEventArgs e)
    {
        Player.MdkState = Player.MdkState switch
        {
            State.Playing => State.Paused,
            State.Paused => State.Playing,
            _ => State.Playing
        };
    }

    private async void SelectFile(object? sender, RoutedEventArgs e)
    {
        var storage = StorageProvider;
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = [VideoFileType, AudioFileType]
        });
        if (files.Count == 0)
        {
            return;
        }
        var file = files[0];
        Player.MediaPath = file.Path.LocalPath;
        FilePath.Text = file.Path.LocalPath;
    }
}
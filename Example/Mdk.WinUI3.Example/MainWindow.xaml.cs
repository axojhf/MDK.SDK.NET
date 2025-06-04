using System;
using MDK.SDK.NET;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mdk.WinUI3.Example
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly MDKPlayer _player = new();
        private readonly D3D11RenderAPI _ra = new();
        public MainWindow()
        {
            this.InitializeComponent();
            _player.SetDecoders(MediaType.Video, ["MFT:d3d=11", "hap", "D3D11", "DXVA", "FFmpeg"]);
            _player.OnMediaStatus((old, @new) => old == @new || @new != MediaStatus.Loaded || true);
            var vid = ((IWinRTObject)swapChainPanel).NativeObject;
            _player.SetRenderAPI(_ra, vid.ThisPtr);
            _player.UpdateNativeSurface(vid.ThisPtr);
        }

        // Play
        private void Play(object? sender, RoutedEventArgs e)
        {
            var state = _player.State;
            _player.Set(state == PlaybackState.NotRunning ? State.Playing : State.Stopped);
        }

        // Select File
        private async void SelectFileAsync(object? sender, RoutedEventArgs e)
        {

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".flv");
            picker.FileTypeFilter.Add(".webm");
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".m4a");
            picker.FileTypeFilter.Add(".flac");
            picker.FileTypeFilter.Add(".wav");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var file = await picker.PickSingleFileAsync();
            if (file == null) return;
            _player.SetMedia(file.Path);
            FilePath.Text = file.Path;
        }
    }
}
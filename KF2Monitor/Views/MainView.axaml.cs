using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KF2Monitor.Views;

public partial class MainView : UserControl
{
    private CancellationTokenSource? _longPressCts;
    private Avalonia.Point _pressPos;

    public MainView()
    {
        InitializeComponent();
        
        ViewModels.MainViewModel.PickImageAction = async () =>
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return false;
                
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Image",
                    AllowMultiple = false
                });
                
                if (files.Count > 0)
                {
                    var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    var targetPath = Path.Combine(dir, "widget_bg.png");
                    
                    await using (var stream = await files[0].OpenReadAsync())
                    {
                        using (var fs = File.Create(targetPath))
                        {
                            await stream.CopyToAsync(fs);
                            await fs.FlushAsync();
                        }
                    }
                    
                    await Task.Delay(100);
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (ViewModels.MainViewModel.Instance != null)
                    ViewModels.MainViewModel.Instance.UpdateMessage = "Error: " + ex.Message;
            }
            return false;
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        var insetsManager = TopLevel.GetTopLevel(this)?.InsetsManager;
        if (insetsManager != null)
        {
            insetsManager.SystemBarColor = Colors.Transparent;
        }
    }

    private async void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _pressPos = e.GetPosition(this);
        _longPressCts?.Cancel();
        _longPressCts = new CancellationTokenSource();
        
        try
        {
            await Task.Delay(500, _longPressCts.Token);
            if (ViewModels.MainViewModel.Instance != null)
            {
                ViewModels.MainViewModel.VibrateAction?.Invoke();
                ViewModels.MainViewModel.Instance.IsEditorOpen = true;
            }
        }
        catch (TaskCanceledException) { }
    }

    private void OnPreviewPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_longPressCts != null && !_longPressCts.IsCancellationRequested)
        {
            var pos = e.GetPosition(this);
            if (Math.Abs(pos.X - _pressPos.X) > 10 || Math.Abs(pos.Y - _pressPos.Y) > 10)
            {
                _longPressCts.Cancel();
            }
        }
    }

    private void OnPreviewPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _longPressCts?.Cancel();
    }
}
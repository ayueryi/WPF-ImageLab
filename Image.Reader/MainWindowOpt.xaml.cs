using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

namespace Image.Reader;


public partial class MainWindowOpt
{
    public MainWindowViewModelOpt ViewModel { get; } = new();

    public MainWindowOpt()
    {
        DataContext = this;
        InitializeComponent();
    }
}

public partial class MainWindowViewModelOpt : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ImgItemOpt> _imgItemsCollection = new()
    {
        new ImgItemOpt()
        {
            ImgName = "示例"
        }
    };

    [RelayCommand]
    public async Task OpenFolder()
    {
        var dialog = new OpenFolderDialog();

        if (dialog.ShowDialog() is true)
        {
            var sw = new Stopwatch();
            sw.Start();

            // 获取当前进程
            Process currentProcess = Process.GetCurrentProcess();

            string selectedPath = dialog.FolderName;

            // 获取文件夹中所有的 PNG 文件
            string[] files = Directory.GetFiles(selectedPath, "*.png", SearchOption.AllDirectories);

            // 清空现有的集合
            ImgItemsCollection.Clear();

            // 创建并显示进度窗口

            var progressWindow = new ProgressWindow();
            Task.Run(delegate
            {
                App.Current.Dispatcher.Invoke(() => progressWindow.Show());
            });

            await Task.Run(() =>
            {
                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    try
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            // 创建一个新的 ImgItemOpt，并将其添加到集合中
                            ImgItemsCollection.Add(new ImgItemOpt
                            {
                                ImgName = Path.GetFileName(file),
                                ImgPath = file,
                            });

                            // 更新进度
                            double progress = (double)(i + 1) / files.Length * 100;
                            progressWindow.UpdateProgress(progress, i, files.Length);
                            progressWindow.UpdateTips($"耗时:{sw.ElapsedMilliseconds}ms,进度:{i}/{files.Length}");
                        });
                    }
                    catch (Exception ex)
                    {
                        // 处理加载图像时可能出现的异常
                        MessageBox.Show($"无法加载图像: {file}\n{ex.Message}", "错误", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                }
            });
        }
    }
}

public class ImgItemOpt
{
    public string ImgName { get; set; }

    public string ImgPath { get; set; }

    public BitmapSource? ImgSource
    {
        get
        {
            if (!File.Exists(ImgPath)) return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(ImgPath);
                bitmap.EndInit();
                bitmap.Freeze(); // 冻结图像以提高性能和线程安全性
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
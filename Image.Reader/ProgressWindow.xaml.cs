namespace Image.Reader;

public partial class ProgressWindow
{
    public ProgressWindow()
    {
        InitializeComponent();
    }

    public void UpdateProgress(double progress, int now, int max)
    {
        ProgressBar.Value = progress;
        if (now == max)
        {
            Dispatcher.Invoke(() => this.Close());
        }
    }

    public void UpdateTips(string msg)
    {
        TipsTextBlock.Text = msg;
    }
}
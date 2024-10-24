using System;
using System.Windows.Forms;
using Ghostscript.NET.DisplayTest.Callbacks;
using Ghostscript.NET.Viewer;

namespace Ghostscript.NET.DisplayTest;

public partial class FMain : Form
{
    private readonly FPreview _preview = new();
    private readonly StdIoHandler _stdioHandler;
    private GhostscriptViewer _viewer;

    public FMain()
    {
        InitializeComponent();

        _stdioHandler = new StdIoHandler(txtOutput);
    }

    private void FMain_Load(object sender, EventArgs e)
    {
        txtOutput.AppendText("Is64BitOperatingSystem: " + Environment.Is64BitOperatingSystem + "\r\n");
        txtOutput.AppendText("Is64BitProcess: " + Environment.Is64BitProcess + "\r\n");

        _preview.Show();
        Show();

        GhostscriptVersionInfo gvi = GhostscriptVersionInfo.GetLastInstalledVersion();

        _viewer = new GhostscriptViewer();

        _viewer.AttachStdIo(_stdioHandler);

        _viewer.DisplaySize += _viewer_DisplaySize;
        _viewer.DisplayUpdate += _viewer_DisplayUpdate;
        _viewer.DisplayPage += _viewer_DisplayPage;

        _viewer.Open(gvi, true);
    }

    private void _viewer_DisplayPage(object sender, GhostscriptViewerViewEventArgs e)
    {
        _preview.pbDisplay.Invalidate();
        _preview.pbDisplay.Update();
    }

    private void _viewer_DisplayUpdate(object sender, GhostscriptViewerViewEventArgs e)
    {
        _preview.pbDisplay.Invalidate();
        _preview.pbDisplay.Update();
    }

    private void _viewer_DisplaySize(object sender, GhostscriptViewerViewEventArgs e)
    {
        _preview.pbDisplay.Image = e.Image;
    }

    private void btnRun_Click(object sender, EventArgs e)
    {
        _viewer.Interpreter.Run(txtPostscript.Text);
        _preview.Activate();
    }

    private void FMain_FormClosed(object sender, FormClosedEventArgs e)
    {
        _viewer.Dispose();
        _viewer = null;
    }
}
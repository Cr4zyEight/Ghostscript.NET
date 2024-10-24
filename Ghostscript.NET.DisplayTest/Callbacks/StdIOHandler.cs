using System.Windows.Forms;

namespace Ghostscript.NET.DisplayTest;

public class StdIoHandler : GhostscriptStdIo
{
    private readonly TextBox _outputTextBox;

    public StdIoHandler(TextBox outputTextBox) : base(true, true, true)
    {
        _outputTextBox = outputTextBox;
    }

    public override void StdIn(out string input, int count)
    {
        input = null;
    }

    public override void StdOut(string output)
    {
        _outputTextBox.AppendText(output + "\r\n");
        _outputTextBox.SelectionStart = _outputTextBox.TextLength;
        _outputTextBox.ScrollToCaret();
    }

    public override void StdError(string error)
    {
        _outputTextBox.AppendText(error + "\r\n");
        _outputTextBox.SelectionStart = _outputTextBox.TextLength;
        _outputTextBox.ScrollToCaret();
    }
}
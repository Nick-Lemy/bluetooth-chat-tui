using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Chat.Ui;

public static class InputDialog
{
    public static string? Prompt(IApplication app, string title, string label)
    {
        using var dialog = new Dialog
        {
            Title = title,
            Width = 50,
            Height = 8,
        };
        dialog.SetScheme(Theme.Accent(Theme.Peach));

        var field = new TextField { X = label.Length + 2, Y = 1, Width = Dim.Fill(2) };
        field.SetScheme(Theme.Input);

        dialog.Add(new Label { Text = label, X = 1, Y = 1 }, field);

        var ok = new Button { Text = "OK", IsDefault = true };
        var cancel = new Button { Text = "Cancel" };

        var confirmed = false;

        void Submit(object? sender, CommandEventArgs e)
        {
            e.Handled = true;
            confirmed = true;
            app.RequestStop(dialog);
        }

        ok.Accepting += Submit;
        field.Accepting += Submit;
        cancel.Accepting += (_, e) => { e.Handled = true; app.RequestStop(dialog); };

        dialog.AddButton(ok);
        dialog.AddButton(cancel);

        field.SetFocus();
        app.Run(dialog);

        return confirmed ? field.Text.Trim() : null;
    }
}

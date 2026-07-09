using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Chat.Ui;

public static class LoginDialog
{
    public static (string Name, string Address)? Show(IApplication app, string name = "", string address = "localhost")
    {
        using var dialog = new Dialog
        {
            Title = "Connect to Hollow Chat",
            Width = 60,
            Height = 11,
        };
        dialog.SetScheme(Theme.Accent(Theme.Mauve));

        var nameField = new TextField { X = 12, Y = 1, Width = Dim.Fill(2), Text = name };
        var addressField = new TextField { X = 12, Y = 3, Width = Dim.Fill(2), Text = address };
        nameField.SetScheme(Theme.Input);
        addressField.SetScheme(Theme.Input);

        dialog.Add(
            new Label { Text = "Name:", X = 1, Y = 1 },
            nameField,
            new Label { Text = "Server:", X = 1, Y = 3 },
            addressField,
            new Label { Text = "host or host:port", X = 12, Y = 4 });

        var connect = new Button { Text = "Connect", IsDefault = true };
        var quit = new Button { Text = "Quit" };

        var confirmed = false;

        void Submit(object? sender, CommandEventArgs e)
        {
            e.Handled = true;
            confirmed = true;
            app.RequestStop(dialog);
        }

        connect.Accepting += Submit;
        nameField.Accepting += Submit;
        addressField.Accepting += Submit;
        quit.Accepting += (_, e) => { e.Handled = true; app.RequestStop(dialog); };

        dialog.AddButton(connect);
        dialog.AddButton(quit);

        nameField.SetFocus();
        app.Run(dialog);

        if (!confirmed) return null;

        var enteredName = nameField.Text.Trim();
        var enteredAddress = addressField.Text.Trim();

        return (
            enteredName.Length == 0 ? "Anonymous" : enteredName,
            enteredAddress.Length == 0 ? "localhost" : enteredAddress);
    }
}

using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Chat.Ui;

public static class CreateRoomDialog
{
    public static (string Name, string Description, int? Code)? Show(IApplication app)
    {
        using var dialog = new Dialog
        {
            Title = "Create a room",
            Width = 62,
            Height = 13,
        };
        dialog.SetScheme(Theme.Accent(Theme.Green));

        var nameField = new TextField { X = 15, Y = 1, Width = Dim.Fill(2) };
        var descriptionField = new TextField { X = 15, Y = 3, Width = Dim.Fill(2) };
        var codeField = new TextField { X = 15, Y = 5, Width = 10 };
        nameField.SetScheme(Theme.Input);
        descriptionField.SetScheme(Theme.Input);
        codeField.SetScheme(Theme.Input);

        dialog.Add(
            new Label { Text = "Name:", X = 1, Y = 1 },
            nameField,
            new Label { Text = "Description:", X = 1, Y = 3 },
            descriptionField,
            new Label { Text = "Code:", X = 1, Y = 5 },
            codeField,
            new Label { Text = "Leave the code blank for a public room.", X = 1, Y = 7 });

        var create = new Button { Text = "Create", IsDefault = true };
        var cancel = new Button { Text = "Cancel" };

        var confirmed = false;

        void Submit(object? sender, CommandEventArgs e)
        {
            e.Handled = true;
            confirmed = true;
            app.RequestStop(dialog);
        }

        create.Accepting += Submit;
        nameField.Accepting += Submit;
        descriptionField.Accepting += Submit;
        codeField.Accepting += Submit;
        cancel.Accepting += (_, e) => { e.Handled = true; app.RequestStop(dialog); };

        dialog.AddButton(create);
        dialog.AddButton(cancel);

        nameField.SetFocus();
        app.Run(dialog);

        if (!confirmed) return null;

        var name = nameField.Text.Trim();
        if (name.Length == 0)
        {
            MessageBox.ErrorQuery(app, "Invalid room", "Room name cannot be empty.", "OK");
            return null;
        }

        int? code = null;
        var codeText = codeField.Text.Trim();
        if (codeText.Length > 0)
        {
            if (!int.TryParse(codeText, out var parsed) || parsed < 1000 || parsed > 9999)
            {
                MessageBox.ErrorQuery(app, "Invalid code", "The code must be a number between 1000 and 9999.", "OK");
                return null;
            }
            code = parsed;
        }

        return (name, descriptionField.Text.Trim(), code);
    }
}

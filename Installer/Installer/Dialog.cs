using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpKit.Installer
{
    class Dialog
    {
        public Dialog()
        {
            Buttons = new List<DialogButton>();

        }

        public static string ShowSimple(string msg, string[] buttons)
        {
            var dialog = new Dialog
            {
                Message = msg,
                Buttons = buttons.Select(t => new DialogButton { Text = t }).ToList(),
            };
            var btn = dialog.ShowDialog();
            return btn.Text;
        }
        public DialogForm DialogForm { get; set; }
        public DialogButton LastClickedButton { get; set; }
        public DialogButton ShowDialog()
        {
            return ShowDialog(null);
        }
        public DialogButton ShowDialog(IWin32Window owner)
        {
            if (DialogForm == null)
                DialogForm = new DialogForm();
            DialogForm.Text = Title;
            DialogForm.TextBox1.Text = Message;
            DialogForm.FlowLayoutPanel1.Controls.Clear();
            foreach (var btn in Buttons)
            {
                var btn2 = new Button { Text = btn.Text, Tag = btn };
                if (btn.IsCancelButton)
                    DialogForm.CancelButton = btn2;
                if (btn.IsAcceptButton)
                    DialogForm.AcceptButton = btn2;
                btn2.Click += (s, e) =>
                {
                    LastClickedButton = (DialogButton)((Button)s).Tag;
                    DialogForm.Close();
                };
                DialogForm.FlowLayoutPanel1.Controls.Add(btn2);
            }
            DialogForm.ShowDialog(owner);
            return LastClickedButton;
        }

        public string Message { get; set; }

        public List<DialogButton> Buttons { get; set; }

        public string Header { get; set; }

        public string Title { get; set; }
    }

    public class DialogButton
    {
        public string Text { get; set; }
        public bool IsCancelButton { get; set; }
        public bool IsAcceptButton { get; set; }

    }

}

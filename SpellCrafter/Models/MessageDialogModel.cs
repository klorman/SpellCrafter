using SpellCrafter.Enums;

namespace SpellCrafter.Models
{
    public class MessageDialogModel
    {
        public string Message { get; set; } = "";
        public MessageDialogType Type { get; set; }
        public DialogResult DialogResult { get; set; } = DialogResult.None;

        public MessageDialogModel(string message, MessageDialogType type)
        {
            Message = message;
            Type = type;
        }
    }
}

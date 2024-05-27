using SpellCrafter.Models;

namespace SpellCrafter.Messages
{
    public class ViewAddonMessage
    {
        public Addon? Addon { get; private set; }

        public ViewAddonMessage(Addon? addon)
        {
            Addon = addon;
        }
    }
}

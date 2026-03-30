using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Custom.Systems.Hotbar
{
    // Hotbar MINIMAL: só guarda "strings" por slot.
    // - Para feats: guarda o comando (ex: "[disarm")
    // - Para abilities: guarda token "ABILITY:<id>"
    // Obs: sem persistência por enquanto (reiniciar server limpa). Depois a gente serializa no PlayerMobile.
    public static class OSUHotBar
    {
        public const int Slots = 10;

        private static readonly Dictionary<Serial, string[]> _bars = new Dictionary<Serial, string[]>();

        private static string[] GetBar(PlayerMobile pm)
        {
            if (pm == null)
                return null;

            string[] bar;
            if (!_bars.TryGetValue(pm.Serial, out bar) || bar == null || bar.Length != Slots)
            {
                bar = new string[Slots];
                _bars[pm.Serial] = bar;
            }

            return bar;
        }

        public static bool TryAddNext(PlayerMobile pm, string value, out string message)
        {
            message = null;

            if (pm == null)
            {
                message = "Erro interno.";
                return false;
            }

            if (string.IsNullOrEmpty(value))
            {
                message = "Nada para adicionar ao hotbar.";
                return false;
            }

            string[] bar = GetBar(pm);
            if (bar == null)
            {
                message = "Erro interno.";
                return false;
            }

            for (int i = 0; i < bar.Length; i++)
            {
                if (string.IsNullOrEmpty(bar[i]))
                {
                    bar[i] = value;
                    message = "Adicionado ao Hotbar no slot " + (i + 1) + ".";
                    return true;
                }
            }

            message = "Hotbar cheio. Remova algo antes.";
            return false;
        }

        // (extra útil pra debug)
        public static string GetSlot(PlayerMobile pm, int slotIndex)
        {
            string[] bar = GetBar(pm);
            if (bar == null) return null;
            if (slotIndex < 0 || slotIndex >= bar.Length) return null;
            return bar[slotIndex];
        }
    }
}

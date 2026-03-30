using Server.Custom.Systems.HtmlBooks;
using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Custom.Systems.HtmlBooks.Engine
{
    /// <summary>
    /// Garante que um selo (1..100) só possa ser escolhido por UMA BookSealerTool por vez.
    /// O selo 0 é genérico/invisível e não é reservado.
    /// </summary>
    public static class BookSealRegistry
    {
        private static readonly Dictionary<int, Serial> _sealToTool = new Dictionary<int, Serial>();
        private static readonly Dictionary<Serial, int> _toolToSeal = new Dictionary<Serial, int>();

        public static bool IsReserved(int sealId)
        {
            if (sealId <= 0) return false;
            return _sealToTool.ContainsKey(sealId);
        }

        public static bool TryReserve(BookSealerTool tool, int sealId)
        {
            if (tool == null || tool.Deleted)
                return false;

            if (sealId <= 0)
            {
                // 0 = não reserva
                Release(tool);
                return true;
            }

            // já reservado por outro?
            Serial owner;
            if (_sealToTool.TryGetValue(sealId, out owner) && owner != tool.Serial)
                return false;

            // se o tool já tinha outro selo, libera
            Release(tool);

            _sealToTool[sealId] = tool.Serial;
            _toolToSeal[tool.Serial] = sealId;
            return true;
        }

        public static void Release(BookSealerTool tool)
        {
            if (tool == null)
                return;

            int oldId;
            if (_toolToSeal.TryGetValue(tool.Serial, out oldId))
            {
                _toolToSeal.Remove(tool.Serial);

                Serial cur;
                if (_sealToTool.TryGetValue(oldId, out cur) && cur == tool.Serial)
                    _sealToTool.Remove(oldId);
            }
        }

        public static List<int> GetAvailableSealIds(BookSealerTool tool)
        {
            // lista “compacta”, sem buracos
            var list = new List<int>();

            for (int i = 1; i <= 100; i++)
            {
                Serial owner;
                if (_sealToTool.TryGetValue(i, out owner))
                {
                    if (tool != null && owner == tool.Serial)
                        list.Add(i);
                }
                else
                {
                    list.Add(i);
                }
            }
            return list;
        }

        public static void OnToolLoaded(BookSealerTool tool)
        {
            if (tool == null || tool.Deleted)
                return;

            // re-reserva depois de load
            if (tool.SealId > 0)
            {
                if (!TryReserve(tool, tool.SealId))
                {
                    // conflito ao carregar: cai pra genérico
                    tool.SealId = 0;
                }
            }
        }
    }
}

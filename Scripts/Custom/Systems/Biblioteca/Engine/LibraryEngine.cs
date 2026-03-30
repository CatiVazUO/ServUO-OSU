using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Custom.Systems.HtmlBooks.Engine;

namespace Server.Custom.Systems.Biblioteca.Engine
{
    public static class LibraryEngine
    {
        private static LibraryStorage _storage;

        private static void EnsureStorage()
        {
            if (_storage != null && !_storage.Deleted)
                return;

            // tenta achar storage existente
            foreach (Item it in World.Items.Values)
            {
                LibraryStorage s = it as LibraryStorage;
                if (s != null && !s.Deleted)
                {
                    _storage = s;
                    return;
                }
            }

            // cria se não achou
            _storage = new LibraryStorage();
            _storage.MoveToWorld(new Point3D(0, 0, 0), Map.Internal);
        }

        public static void Initialize()
        {
            EventSink.WorldLoad += OnWorldLoad;
        }

        private static void OnWorldLoad()
        {
            EnsureStorage();
        }

        public static bool IsReady
        {
            get
            {
                EnsureStorage();
                return _storage != null && !_storage.Deleted;
            }
        }

        public static IReadOnlyList<LibraryEntry> GetEntries()
        {
            if (!IsReady)
                return new List<LibraryEntry>();

            return _storage.Entries;
        }

        public static bool TitleLanguageExists(string title, OSULanguage lang)
        {
            if (!IsReady)
                return false;

            string tn = LibraryUtil.Normalize(title);
            for (int i = 0; i < _storage.Entries.Count; i++)
            {
                LibraryEntry e = _storage.Entries[i];
                if (e != null && e.Language == lang && e.TitleNorm == tn)
                    return true;
            }

            return false;
        }

        public static bool TryAddPublication(PlayerMobile pm, Item item, out string failReason)
        {
            failReason = null;

            if (!IsReady)
            {
                failReason = "A biblioteca ainda não está pronta.";
                return false;
            }

            if (item == null || item.Deleted)
            {
                failReason = "Item inválido.";
                return false;
            }

            if (LibraryUtil.IsLoosePage(item))
            {
                failReason = "Páginas soltas não podem ser entregues à biblioteca.";
                return false;
            }

            string title;
            OSULanguage lang;
            string author;
            bool anon;
            bool isComp;

            if (!LibraryUtil.TryGetPublicationInfo(item, out title, out lang, out author, out anon, out isComp))
            {
                failReason = "A publicação precisa ser um livro/pergaminho HTML selado (ou compilation selado).";
                return false;
            }

            if (TitleLanguageExists(title, lang))
            {
                failReason = "Já existe uma publicação com o mesmo título e idioma na biblioteca.";
                return false;
            }

            if (pm.Backpack == null || !item.IsChildOf(pm.Backpack))
            {
                failReason = "A publicação precisa estar na sua mochila.";
                return false;
            }

            // Move pro storage
            item.Movable = false;
            _storage.DropItem(item);

            _storage.AddEntry(item, title, lang, author, anon, isComp);
            _storage.InvalidateProperties();

            return true;
        }

        public static bool CanReadHere(PlayerMobile pm, Mobile npc, int range)
        {
            if (pm == null || npc == null)
                return false;

            if (pm.Map != npc.Map)
                return false;

            return pm.InRange(npc.Location, range);
        }

        public static bool PlayerUnderstands(PlayerMobile pm, OSULanguage lang)
        {
            return LanguageKnowledge.Understands(pm, lang);
        }

        // ===== Storage item + serialization =====
        public class LibraryStorage : Bag
        {
            private List<LibraryEntry> _entries;

            public List<LibraryEntry> Entries
            {
                get { return _entries; }
            }

            public override string DefaultName
            {
                get { return "LibraryStorage"; }
            }

            public LibraryStorage() : base()
            {
                Movable = false;
                Visible = false;
                Hue = 1;
                _entries = new List<LibraryEntry>();
            }

            public LibraryStorage(Serial serial) : base(serial)
            {
            }

            public void AddEntry(Item item, string title, OSULanguage lang, string author, bool anon, bool isComp)
            {
                var e = new LibraryEntry();
                e.ItemSerial = item.Serial;
                e.Title = title;
                e.TitleNorm = LibraryUtil.Normalize(title);
                e.Language = lang;
                e.Author = author;
                e.IsAnonymous = anon;
                e.IsCompilation = isComp;
                _entries.Add(e);
            }

            public Item FindItem(Serial serial)
            {
                // Primeiro tenta via World (rápido)
                Item it = World.FindItem(serial);

                // Tem que estar dentro do storage
                if (it != null && !it.Deleted && it.IsChildOf(this))
                    return it;

                // Fallback: varre os itens do container
                for (int i = 0; i < Items.Count; i++)
                {
                    Item child = Items[i] as Item;
                    if (child != null && child.Serial == serial)
                        return child;
                }

                return null;
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.Write(1); // version

                writer.Write(_entries.Count);
                for (int i = 0; i < _entries.Count; i++)
                {
                    LibraryEntry e = _entries[i];
                    writer.Write(e.ItemSerial);
                    writer.Write(e.Title);
                    writer.Write(e.TitleNorm);
                    writer.Write((int)e.Language);
                    writer.Write(e.Author);
                    writer.Write(e.IsAnonymous);
                    writer.Write(e.IsCompilation);
                }
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                int version = reader.ReadInt();

                _entries = new List<LibraryEntry>();

                int count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    LibraryEntry e = new LibraryEntry();
                    e.ItemSerial = reader.ReadInt();
                    e.Title = reader.ReadString();
                    e.TitleNorm = reader.ReadString();
                    e.Language = (OSULanguage)reader.ReadInt();
                    e.Author = reader.ReadString();
                    e.IsAnonymous = reader.ReadBool();
                    e.IsCompilation = reader.ReadBool();
                    _entries.Add(e);
                }

                Movable = false;
                Visible = false;
            }
        }

        public class LibraryEntry
        {
            public int ItemSerial;
            public string Title;
            public string TitleNorm;
            public OSULanguage Language;
            public string Author;
            public bool IsAnonymous;
            public bool IsCompilation;
        }
    }
}

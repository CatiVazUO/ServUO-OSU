using System;
using System.Collections.Generic;
using Server;

namespace Server.Custom.Systems.Creation.Engine
{
    public enum OSUCreationPath
    {
        None = 0,
        Warrior = 1,
        Artisan = 2
    }

    public enum OSUCreationGameMode
    {
        None = 0,
        Pvp = 1,
        NoPvp = 2
    }
    public enum OSUCreationAttribute
    {
        Str,
        Dex,
        Int,
        HP,
        Vit,
        Mana
    }

    /// <summary>
    /// Guarda as escolhas "virtuais" da criação. Só vira "real" quando você decidir no portal/entrada no mundo.
    /// </summary>
    public class OSUCreationContext
    {
        // ===== Página 2 =====
        public OSUCreationPath Path { get; set; }
        public OSUCreationGameMode GameMode { get; set; }

        public int CombatCap { get; set; }
        public int ProfCap { get; set; }

        // ===== Página 3 (Cultura / Povo) =====
        // (por enquanto só ID / flag)
        public string CultureId { get; set; }

        // ===== Página 4 (Atributos + Defeitos/Qualidades + Religião) =====
        // 150 pontos para dividir (6 atributos)
        public int Attr_Str { get; set; }
        public int Attr_Dex { get; set; }
        public int Attr_Int { get; set; }
        public int Attr_HP { get; set; }
        public int Attr_Vit { get; set; }
        public int Attr_Man { get; set; }

        // (Página 4) HP escolhido no sistema de atributos (quando implementarmos essa parte)
        public int ChosenHpCap { get; set; }  // 0 = ainda não definido

        public List<string> SelectedDefQualIds { get; set; } = new List<string>();

        public string ReligionId { get; set; } // pode ser "none" depois, mas por enquanto string

        // ===== Página 5 (Skills iniciais) =====
        public List<string> StartingCombatSkills { get; set; } = new List<string>();
        public List<string> StartingProfessionSkills { get; set; } = new List<string>();

        // ===== Página 6 (Aparência + Nome + Gênero etc) =====
        public bool GenderFemale { get; set; } // false = male, true = female (placeholder)
        public string ChosenName { get; set; }

        public int SkinHue { get; set; }

        public int HairItemId { get; set; }
        public int HairHue { get; set; }

        public int HairGumpId { get; set; }

        public int BeardItemId { get; set; }
        public int BeardHue { get; set; }

        // ===== Página 7 (Ficha RP + Avatar) =====
        public int RpHeight { get; set; }
        public int RpWeight { get; set; }
        public string RpEyeColor { get; set; }
        public string RpPersonality { get; set; }
        public string RpIndole { get; set; }
        public string AvatarId { get; set; }

        // ===== UI Page 5 (não precisa salvar em disco) =====
        public bool Page5ShowCombat { get; set; } = true;
        public int Page5ListPage { get; set; } = 0;
        public string Page5InfoSkill { get; set; } = null;
        public string Page5InfoReligion { get; set; } = null;

        // Página 6 - Aparência
        // Página 6
        public int BodyVariant { get; set; } // 0 = Corpo 1, 1 = Corpo 2
        public int FaceIndex { get; set; }   // 0..8
        public int HairIndex { get; set; }   // índice na lista da cultura
        public int BeardIndex { get; set; }  // índice na lista da cultura
        public bool ShowBeardTab { get; set; } // false = Cabelo, true = Barba

        // ===== Página 7 - Ficha RP =====
        public int RpWeightKg { get; set; }          // 1..140
        public int RpHeightCm { get; set; }          // 1..200
        public int RpAge { get; set; }               // 1..70

        public string RpHistoryStaff { get; set; }   // só staff
        public string RpTraitsPublic { get; set; }   // visível

        public int RpAvatarId { get; set; }          // gump id do avatar escolhido
        public int RpAvatarPage { get; set; }        // paginação (0,1,2...)

        public bool RpWeightSet { get; set; }
        public bool RpHeightSet { get; set; }
        public bool RpAgeSet { get; set; }
        public bool RpHistorySet { get; set; }
        public bool RpTraitsSet { get; set; }


        // ===== Helpers =====
        public bool HasChosenPage2
        {
            get { return Path != OSUCreationPath.None && GameMode != OSUCreationGameMode.None; }
        }

        public void ApplyPathCaps()
        {
            if (Path == OSUCreationPath.Warrior)
            {
                CombatCap = 50000;
                ProfCap = 30000;
            }
            else // Artisan
            {
                CombatCap = 30000;
                ProfCap = 50000;
            }
        }

        public void ResetAll()
        {
            // Página 2
            Path = OSUCreationPath.None;
            GameMode = OSUCreationGameMode.None;
            CombatCap = 0;
            ProfCap = 0;

            // Página 3
            CultureId = null;

            // Página 4
            Attr_Str = Attr_Dex = Attr_Int = Attr_HP = Attr_Vit = Attr_Man = 0;
            SelectedDefQualIds.Clear();
            ReligionId = null;

            // Página 5
            StartingCombatSkills.Clear();
            StartingProfessionSkills.Clear();

            // Página 6
            GenderFemale = false;
            ChosenName = null;
            SkinHue = 0;
            HairItemId = 0;
            HairHue = 0;
            BeardItemId = 0;
            BeardHue = 0;

            // Página 7
            RpHeight = 0;
            RpWeight = 0;
            RpEyeColor = null;
            RpPersonality = null;
            RpIndole = null;
            AvatarId = null;
        }

        // ===== Persistência =====

        private static void WriteStringList(GenericWriter writer, List<string> list)
        {
            if (list == null)
            {
                writer.WriteEncodedInt(0);
                return;
            }

            writer.WriteEncodedInt(list.Count);
            for (int i = 0; i < list.Count; i++)
                writer.Write(list[i]);
        }

        private static List<string> ReadStringList(GenericReader reader)
        {
            int count = reader.ReadEncodedInt();
            List<string> list = new List<string>(count);

            for (int i = 0; i < count; i++)
                list.Add(reader.ReadString());

            return list;
        }

        public void Serialize(GenericWriter writer)
        {
            // version do contexto
            writer.Write(1);

            writer.Write(ChosenHpCap);

            // v0 fields
            writer.Write((int)Path);
            writer.Write((int)GameMode);
            writer.Write(CombatCap);
            writer.Write(ProfCap);

            // v1 fields
            writer.Write(CultureId);

            writer.Write(Attr_Str);
            writer.Write(Attr_Dex);
            writer.Write(Attr_Int);
            writer.Write(Attr_HP);
            writer.Write(Attr_Vit);
            writer.Write(Attr_Man);

            WriteStringList(writer, SelectedDefQualIds);

            writer.Write(ReligionId);

            WriteStringList(writer, StartingCombatSkills);
            WriteStringList(writer, StartingProfessionSkills);

            writer.Write(GenderFemale);
            writer.Write(ChosenName);

            writer.Write(SkinHue);
            writer.Write(HairItemId);
            writer.Write(HairHue);
            writer.Write(BeardItemId);
            writer.Write(BeardHue);

            writer.Write(RpHeight);
            writer.Write(RpWeight);
            writer.Write(RpEyeColor);
            writer.Write(RpPersonality);
            writer.Write(RpIndole);
            writer.Write(AvatarId);
        }

        public static OSUCreationContext Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            OSUCreationContext ctx = new OSUCreationContext();

            // v0 always present
            ctx.Path = (OSUCreationPath)reader.ReadInt();
            ctx.GameMode = (OSUCreationGameMode)reader.ReadInt();
            ctx.CombatCap = reader.ReadInt();
            ctx.ProfCap = reader.ReadInt();

            if (ctx.CombatCap <= 0 && ctx.ProfCap <= 0)
                ctx.ApplyPathCaps();

            if (version >= 1)
            {
                ctx.ChosenHpCap = reader.ReadInt();
                ctx.CultureId = reader.ReadString();

                ctx.Attr_Str = reader.ReadInt();
                ctx.Attr_Dex = reader.ReadInt();
                ctx.Attr_Int = reader.ReadInt();
                ctx.Attr_HP = reader.ReadInt();
                ctx.Attr_Vit = reader.ReadInt();
                ctx.Attr_Man = reader.ReadInt();

                ctx.SelectedDefQualIds = ReadStringList(reader);

                ctx.ReligionId = reader.ReadString();

                ctx.StartingCombatSkills = ReadStringList(reader);
                ctx.StartingProfessionSkills = ReadStringList(reader);

                ctx.GenderFemale = reader.ReadBool();
                ctx.ChosenName = reader.ReadString();

                ctx.SkinHue = reader.ReadInt();
                ctx.HairItemId = reader.ReadInt();
                ctx.HairHue = reader.ReadInt();
                ctx.BeardItemId = reader.ReadInt();
                ctx.BeardHue = reader.ReadInt();

                ctx.RpHeight = reader.ReadInt();
                ctx.RpWeight = reader.ReadInt();
                ctx.RpEyeColor = reader.ReadString();
                ctx.RpPersonality = reader.ReadString();
                ctx.RpIndole = reader.ReadString();
                ctx.AvatarId = reader.ReadString();
            }
            else
            {
                // garante listas existindo
                ctx.SelectedDefQualIds = new List<string>();
                ctx.StartingCombatSkills = new List<string>();
                ctx.StartingProfessionSkills = new List<string>();
            }

            return ctx;
        }
    }
}

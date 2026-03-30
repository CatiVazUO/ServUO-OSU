using Server.Custom.Systems.Creation.Engine;
using Server.Mobiles;
using System;

namespace Server.Custom.Systems.Culture
{
    public enum OSUCultureInfoTopic
    {
        Lore = 1,
        Fisico = 2,
        Papeis = 3,
        Tradicoes = 4,
        Proverbios = 5
    }

    public abstract class OSUCultureDefinition
    {
        // ID interno (vai para o context)
        public abstract string Id { get; }

        // Nome que aparece no gump
        public abstract string DisplayName { get; }

        // Ordem no gump (1..6)
        public abstract int DisplayOrder { get; }

        // Gump image id (114,115,116,117...)
        public abstract int PortraitGumpId { get; }

        // Dados “extras” (não precisa usar agora, mas já fica pronto)
        // public virtual string CapitalCity => null;
        // public virtual string Economy => null;
        // public virtual string GovernedBy => null;

        // Textos por tópico
        public abstract string LoreHtml { get; }
        public abstract string FisicoHtml { get; }
        public abstract string PapeisHtml { get; }
        public abstract string TradicoesHtml { get; }
        public abstract string ProverbiosHtml { get; }

        public virtual int[] MaleHairGumpIds => new int[0];
        public virtual int[] FemaleHairGumpIds => new int[0];

        public virtual int[] MaleBeardGumpIds => DefaultBeardGumpIds;   // se você ainda não tiver barbas, deixa vazio

        public virtual string CapitalCityId => "";
        public virtual string CapitalCityName => "";

        // Skin / Hair / Beard colors (Página 6)
        // Cada cultura pode sobrescrever para limitar as opções.
        // Observação: usamos hues diretamente (os mesmos que você já estava usando no gump).

        //hues
        public virtual int[] SkinHues => new int[0];
        public virtual int[] BeardHues => HairColorHues;

        public virtual int[] HairColorHues => new int[0];

        // IDs de GUMP / ITEM.
        // Preview usa os GUMPs do arquivo da cultura.
        // O personagem equipado usa ITEMs calculados automaticamente.
        public virtual int HairGumpBaseMale => 54000;
        public virtual int HairGumpBaseFemale => 64000;
        public virtual int HairItemBase => 13050;
        public virtual int[] HairGumpIdsFemale => FemaleHairGumpIds;
        public virtual int[] HairGumpIdsMale => MaleHairGumpIds;

        public virtual int[] BeardColorHues => BeardHues;
        public virtual int BeardGumpBase => 53500;
        public virtual int BeardItemBase => 15160;

        public int GetHairGumpId(bool female, int index)
        {
            var list = female ? HairGumpIdsFemale : HairGumpIdsMale;
            if (list == null || list.Length == 0)
                return 0;

            if (index < 0) index = 0;
            if (index >= list.Length) index = list.Length - 1;
            return list[index];
        }

        public int GetHairItemId(bool female, int index)
        {
            int gumpId = GetHairGumpId(female, index);
            return MapHairGumpToItemId(gumpId, female);
        }

        public int GetBeardGumpId(int index)
        {
            var list = MaleBeardGumpIds;
            if (list == null || list.Length == 0)
                return 0;

            if (index < 0) index = 0;
            if (index >= list.Length) index = list.Length - 1;
            return list[index];
        }

        public int GetBeardItemId(int index)
        {
            int gumpId = GetBeardGumpId(index);
            return MapBeardGumpToItemId(gumpId);
        }

        public virtual int MapHairGumpToItemId(int gumpId, bool female)
        {
            if (gumpId <= 0)
                return 0;

            int gumpBase = female ? HairGumpBaseFemale : HairGumpBaseMale;
            return HairItemBase + (gumpId - gumpBase);
        }

        public virtual int MapBeardGumpToItemId(int gumpId)
        {
            if (gumpId <= 0)
                return 0;

            // Ex.: 59830 -> BeardItemBase (padrão 13178)
            return BeardItemBase + (gumpId - BeardGumpBase);
        }

        // Default de barbas: 16 itens, 54500..54515
        public virtual int[] DefaultBeardGumpIds => new int[]
        {
            53500, 53501, 53502, 53503, 53504, 53505, 53506, 53507,
            53508, 53509, 53510, 53511, 53512, 53513, 53514, 53515
        };


        public virtual void GiveStartingOutfit(PlayerMobile pm) { }
        public virtual void GiveStartingItems(PlayerMobile pm) { }

        public virtual Point3D StartLocation => Point3D.Zero;
        public virtual Map StartMap => Map.Trammel;


        public string GetHtml(OSUCultureInfoTopic topic)
        {
            switch (topic)
            {
                default:
                case OSUCultureInfoTopic.Lore: return LoreHtml;
                case OSUCultureInfoTopic.Fisico: return FisicoHtml;
                case OSUCultureInfoTopic.Papeis: return PapeisHtml;
                case OSUCultureInfoTopic.Tradicoes: return TradicoesHtml;
                case OSUCultureInfoTopic.Proverbios: return ProverbiosHtml;
            }
        }

        // Placeholder pros “efeitos reais” no futuro
        public virtual void ApplyEffects(object player, OSUCreationContext ctx)
        {
            // Depois a gente troca object por PlayerMobile quando for usar de verdade
        }
    }
}

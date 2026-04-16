using Server.Custom.Systems.Creation.Engine;
using System;

namespace Server.Custom.Systems.Religion
{
    public abstract class OSUReligionDefinition
    {
        public abstract string Id { get; }
        public abstract string Name { get; }

        /// <summary>Ordem na lista do gump (1..N)</summary>
        public abstract int DisplayOrder { get; }

        public virtual int IconGumpId => 159;

        public abstract string DescriptionHtml { get; }

        public virtual int[] TempleRiteItemIds => new int[0];
        public virtual int[] TempleWeddingItemIds => new int[0];
        public virtual int TempleFuneralCoffinItemId => 0x1C41;
        public virtual int TempleStatueItemId => 0; // deixe 0 enquanto não existir a estátua do deus

        public virtual void ApplyEffects(object player, OSUCreationContext ctx)
        {
            // placeholder
        }
    }
}

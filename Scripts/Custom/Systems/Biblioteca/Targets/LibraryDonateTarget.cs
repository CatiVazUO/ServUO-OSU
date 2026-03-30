using System;
using Server.Mobiles;
using Server.Targeting;
using Server.Items;
using Server.Custom.Systems.Biblioteca.Engine;

namespace Server.Custom.Systems.Biblioteca.Targets
{
    public class LibraryDonateTarget : Target
    {
        private readonly PlayerMobile _pm;
        private readonly Mobile _npc;

        public LibraryDonateTarget(PlayerMobile pm, Mobile npc) : base(12, false, TargetFlags.None)
        {
            _pm = pm;
            _npc = npc;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (_pm == null || _npc == null || _pm.Deleted || _npc.Deleted)
                return;

            Item item = targeted as Item;
            if (item == null)
            {
                _pm.SendMessage(0x22, "Selecione um item válido.");
                return;
            }

            string fail;
            if (!LibraryEngine.TryAddPublication(_pm, item, out fail))
            {
                _pm.SendMessage(0x22, fail);
                return;
            }

            _pm.SendMessage(0x55, "Publicação adicionada à biblioteca com sucesso!");
        }
    }
}

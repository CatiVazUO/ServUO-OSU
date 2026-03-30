using Server.Commands;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Mobiles;

namespace Server.Custom.Systems.SkillXP
{
    public interface IOSUFeat
    {
        OSUFeatDefinition Definition { get; }

        string CommandText { get; }

        string RequirementText { get; }
        bool CanPurchase(PlayerMobile pm, out string reason);

        void OnPurchased(PlayerMobile pm);

        void OnCommand(PlayerMobile pm, CommandEventArgs e);
    }
}

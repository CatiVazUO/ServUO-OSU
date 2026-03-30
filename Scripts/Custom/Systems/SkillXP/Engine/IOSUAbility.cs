using Server.Commands;
using Server.Custom.Systems.SkillXP.Engine;
using Server.Mobiles;

namespace Server.Custom.Systems.SkillXP
{
    public interface IOSUAbilityModule
    {
        void InitializeModule();
    }
    public interface IOSUAbility
    {
        OSUAbilityDefinition Definition { get; }
        string CommandText { get; }

        bool CanPurchase(PlayerMobile pm, out string reason);

        void OnPurchased(PlayerMobile pm);

        void OnCommand(PlayerMobile pm, CommandEventArgs e);
    }
}

using Server;
using Server.Commands;
using Server.Mobiles;
using System;
using System.Text;

namespace Server.Custom.Systems.Postos
{
    public class PostoCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("PostoInfo", AccessLevel.GameMaster, OnInfo);
            CommandSystem.Register("PostoReset", AccessLevel.GameMaster, OnReset);
            CommandSystem.Register("PostoProgress", AccessLevel.GameMaster, OnProgress);
            CommandSystem.Register("PostoReinoInfo", AccessLevel.GameMaster, OnReinoInfo);
        }

        private static void OnInfo(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length == 0)
            {
                e.Mobile.SendMessage("Use [PostoInfo <idDoPosto>.");
                StringBuilder sb = new StringBuilder();

                foreach (PostoDefinition postoDef in PostoSystem.AllDefinitions)
                {
                    if (sb.Length > 0)
                        sb.Append(" | ");

                    sb.Append(postoDef.Id);
                }

                e.Mobile.SendMessage("IDs: {0}", sb.ToString());
                return;
            }

            string postoId = e.Arguments[0];
            PostoDefinition def = PostoSystem.GetDefinition(postoId);
            PostoState st = PostoSystem.GetState(postoId);

            if (def == null || st == null)
            {
                e.Mobile.SendMessage("Posto inválido.");
                return;
            }

            BasePostoNPC npc = BasePostoNPC.FindByPostoId(postoId);

            e.Mobile.SendMessage("Posto: {0} ({1})", def.Name, def.Id);
            e.Mobile.SendMessage("Reino dono: {0}", PostoSystem.GetOwnerLabel(st));
            e.Mobile.SendMessage("Acordo ativo: {0}", PostoSystem.GetProgressCityLabel(st));
            e.Mobile.SendMessage("Progresso: {0}", PostoSystem.GetObjectiveProgressText(def, st));
            e.Mobile.SendMessage("Baú: {0}", PostoSystem.GetStoredAmount(def.Id));
            e.Mobile.SendMessage("Protegido até UTC: {0}", st.ProtectedUntilUtc);

            if (PostoSystem.IsContestActive(st))
            {
                e.Mobile.SendMessage("Disputa até UTC: {0}", st.ContestEndsUtc);
                if (st.ContestScores != null)
                {
                    for (int i = 0; i < st.ContestScores.Count; i++)
                    {
                        PostoContestScore score = st.ContestScores[i];
                        if (score == null)
                            continue;

                        e.Mobile.SendMessage("  {0}: {1}", PostoSystem.NormalizeCityId(score.CityId), score.Score);
                    }
                }
            }

            if (npc != null && !npc.Deleted)
                e.Mobile.SendMessage("NPC encontrado para esse posto.");
            else
                e.Mobile.SendMessage("Nenhum NPC carregado encontrado para esse posto.");
        }

        private static void OnReset(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length == 0)
            {
                e.Mobile.SendMessage("Use [PostoReset <idDoPosto>.");
                return;
            }

            string msg;
            if (PostoSystem.ResetPosto(e.Arguments[0], out msg))
                e.Mobile.SendMessage(msg);
            else
                e.Mobile.SendMessage(msg);
        }

        private static void OnProgress(CommandEventArgs e)
        {
            if (e.Arguments == null || e.Arguments.Length < 2)
            {
                e.Mobile.SendMessage("Use [PostoProgress <idDoPosto> <valor>.");
                return;
            }

            int value;
            if (!Int32.TryParse(e.Arguments[1], out value))
            {
                e.Mobile.SendMessage("Valor inválido.");
                return;
            }

            string msg;
            if (PostoSystem.SetProgress(e.Arguments[0], value, out msg))
                e.Mobile.SendMessage(msg);
            else
                e.Mobile.SendMessage(msg);
        }

        private static void OnReinoInfo(CommandEventArgs e)
        {
            string[] cities = PostoSystem.GetKnownCities();

            for (int i = 0; i < cities.Length; i++)
            {
                PostoKingdomResourceLedger ledger = PostoSystem.GetLedger(cities[i]);
                e.Mobile.SendMessage(
                    "{0} -> Ferro: {1} | Madeira: {2} | Algodão: {3}",
                    ledger.CityId,
                    ledger.Iron,
                    ledger.Wood,
                    ledger.Cotton);
            }
        }
    }
}

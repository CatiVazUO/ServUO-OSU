using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System.IO;
using Server.Misc;
using System.Collections.Generic;
using System;


namespace Server.Custom.Systems.Reinos
{
    public class ReinoDebugCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("Darpedra", AccessLevel.GameMaster, new CommandEventHandler(DarPedra_OnCommand));
            CommandSystem.Register("ReinoDarAcesso", AccessLevel.GameMaster, new CommandEventHandler(ReinoDarAcesso_OnCommand));
            CommandSystem.Register("ReinoRemoverAcesso", AccessLevel.GameMaster, new CommandEventHandler(ReinoRemoverAcesso_OnCommand));
            CommandSystem.Register("ReinoFase", AccessLevel.GameMaster, OnSetManualPhase);
            CommandSystem.Register("ReinoAutoFase", AccessLevel.GameMaster, OnClearManualPhase);
            CommandSystem.Register("ReinoInfo", AccessLevel.GameMaster, OnReinoInfo);
            CommandSystem.Register("DarUrna", AccessLevel.GameMaster, OnGiveUrna);
            CommandSystem.Register("ReinoForcarCheck", AccessLevel.GameMaster, OnForceCheck);
            CommandSystem.Register("ReinoTeste", AccessLevel.GameMaster, OnTestResult);
            CommandSystem.Register("ReinoLimpa", AccessLevel.GameMaster, OnClearTestResult);
            CommandSystem.Register("ReinoPosse", AccessLevel.GameMaster, OnForceInstallGovernors);
            CommandSystem.Register("ReinoFim", AccessLevel.GameMaster, OnForceEndMandate);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------


        private static void OnForceEndMandate(CommandEventArgs e)
        {
            if (e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Uso: [reinofimmandato <cityId>");
                return;
            }

            int cityId;
            if (!Int32.TryParse(e.Arguments[0], out cityId) || cityId < 0 || cityId >= ReinoElectionsSystem.CityNames.Length)
            {
                e.Mobile.SendMessage("Cidade inválida. Use 0, 1, 2 ou 3.");
                return;
            }

            ReinoCityData city = ReinoElectionsSystem.GetCityData(cityId);

            if (city == null)
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            PlayerMobile oldGovernor = ReinoElectionsSystem.FindPlayer(city.GovernorSerial);

            if (oldGovernor != null)
                ReinoAccessHelper.RevokeGovernorAccess(oldGovernor, cityId);

            city.GovernorSerial = 0;
            city.GovernorName = String.Empty;
            city.GovernorSinceUtc = DateTime.MinValue;

            ReinoElectionsSystem.Save();

            e.Mobile.SendMessage("Mandato encerrado manualmente em {0}.", ReinoElectionsSystem.GetCityName(cityId));
        }

        private static void OnForceInstallGovernors(CommandEventArgs e)
        {
            DateTime now = ReinoElectionsSystem.GetShardNow();
            int cycleId = ReinoElectionsSystem.GetElectionCycleId(now);

            ReinoElectionsSystem.InstallPendingGovernors(cycleId);

            ReinoElectionsSystem.Save();
            e.Mobile.SendMessage("Posse dos governadores pendentes executada.");
        }

        [Usage("Darpedra <cityId>")]
        [Description("Cria uma pedra do reino na mochila do GM.")]
        private static void DarPedra_OnCommand(CommandEventArgs e)
        {
            if (e.Length != 1)
            {
                e.Mobile.SendMessage("Uso: [Darpedra <cityId>");
                return;
            }

            int cityId;

            if (!Int32.TryParse(e.GetString(0), out cityId))
            {
                e.Mobile.SendMessage("CityId inválido.");
                return;
            }

            if (e.Mobile.Backpack == null)
            {
                e.Mobile.SendMessage("Você está sem backpack.");
                return;
            }

            e.Mobile.Backpack.DropItem(new PedraDoReino(cityId));
            e.Mobile.SendMessage("Pedra criada.");
        }



        //---------------------------------------------------------------------------------------------------------------------------------------------------

        [Usage("ReinoLimpa")]
        [Description("Limpa eleições")]
        private static void OnClearTestResult(CommandEventArgs e)
        {
            if (e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Uso: [reinolimparresultado <cityId>");
                return;
            }

            int cityId;
            if (!Int32.TryParse(e.Arguments[0], out cityId) || cityId < 0 || cityId >= ReinoElectionsSystem.CityNames.Length)
            {
                e.Mobile.SendMessage("Cidade inválida. Use 0, 1, 2 ou 3.");
                return;
            }

            ReinoCityData city = ReinoElectionsSystem.GetCityData(cityId);

            if (city == null)
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            PlayerMobile oldGovernor = ReinoElectionsSystem.FindPlayer(city.GovernorSerial);

            if (oldGovernor != null)
                ReinoAccessHelper.RevokeGovernorAccess(oldGovernor, cityId);

            city.GovernorSerial = 0;
            city.GovernorName = String.Empty;
            city.GovernorSinceUtc = DateTime.MinValue;

            city.PendingGovernorSerial = 0;
            city.PendingGovernorName = String.Empty;
            city.PendingCycleId = 0;

            city.ResultStartDateUtc = DateTime.MinValue;
            city.ResultEndDateUtc = DateTime.MinValue;
            city.ResultVisibleUntilUtc = DateTime.MinValue;

            city.Candidates.Clear();

            List<int> votesToRemove = new List<int>();

            foreach (KeyValuePair<int, ReinoVoteRecord> kv in ReinoElectionsSystem._votesByVoter)
            {
                if (kv.Value != null && kv.Value.CityId == cityId)
                    votesToRemove.Add(kv.Key);
            }

            for (int i = 0; i < votesToRemove.Count; i++)
                ReinoElectionsSystem._votesByVoter.Remove(votesToRemove[i]);

            ReinoElectionsSystem.Save();

            e.Mobile.SendMessage("Cidade {0} foi limpa: governador, resultado, candidaturas e votos foram removidos.", ReinoElectionsSystem.GetCityName(cityId));
        }



        //---------------------------------------------------------------------------------------------------------------------------------------------------



        [Usage("ReinoTeste <cityId> <nomeDoVencedor>")]
        [Description("Deixa testar o resultado do sistema de eleições")]
        private static void OnTestResult(CommandEventArgs e)
        {
            if (e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Uso: [reinotestresultado <cityId> [nomeDoVencedor]");
                return;
            }

            int cityId;
            if (!Int32.TryParse(e.Arguments[0], out cityId) || cityId < 0 || cityId >= ReinoElectionsSystem.CityNames.Length)
            {
                e.Mobile.SendMessage("Cidade inválida. Use 0, 1, 2 ou 3.");
                return;
            }

            ReinoCityData city = ReinoElectionsSystem.GetCityData(cityId);

            if (city == null)
            {
                e.Mobile.SendMessage("Cidade inválida.");
                return;
            }

            string winnerName = String.Empty;
            int winnerSerial = 1;

            if (e.Arguments.Length >= 2)
            {
                winnerName = e.Arguments[1];

                if (String.IsNullOrWhiteSpace(winnerName))
                {
                    e.Mobile.SendMessage("Nome do vencedor inválido.");
                    return;
                }
            }
            else
            {
                ReinoCandidateEntry winner = ReinoElectionsSystem.GetWinningCandidate(city);

                if (winner == null)
                {
                    e.Mobile.SendMessage("Essa cidade não tem candidatos para testar. Use: [reinotestresultado <cityId> <nomeDoVencedor>");
                    return;
                }

                winnerName = winner.Nome;
                winnerSerial = winner.Serial;
            }

            DateTime now = ReinoElectionsSystem.GetShardNow();
            DateTime mandateStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0).AddMonths(1);
            DateTime mandateEnd = mandateStart.AddMonths(2);

            city.PendingGovernorSerial = winnerSerial;
            city.PendingGovernorName = winnerName;
            city.PendingCycleId = ReinoElectionsSystem.GetElectionCycleId(now);
            city.ResultStartDateUtc = mandateStart.ToUniversalTime();
            city.ResultEndDateUtc = mandateEnd.ToUniversalTime();
            city.ResultVisibleUntilUtc = DateTime.UtcNow.AddDays(3);

            ReinoElectionsSystem._lastAnnouncedCycleId = city.PendingCycleId;

            ReinoElectionsSystem.Save();

            e.Mobile.SendMessage("Resultado de teste criado para {0}. Vencedor: {1}", ReinoElectionsSystem.GetCityName(cityId), city.PendingGovernorName);
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------


        [Usage("ReinoDarAcesso <cityId>")]
        [Description("Dá acesso de governador + chave para um alvo.")]
        private static void ReinoDarAcesso_OnCommand(CommandEventArgs e)
        {
            if (e.Length != 1)
            {
                e.Mobile.SendMessage("Uso: [ReinoDarAcesso <cityId>");
                return;
            }

            int cityId;

            if (!Int32.TryParse(e.GetString(0), out cityId))
            {
                e.Mobile.SendMessage("CityId inválido.");
                return;
            }

            e.Mobile.SendMessage("Escolha o player.");
            e.Mobile.Target = new ReinoDarAcessoTarget(cityId);
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------


        [Usage("Reinoforcarcheck")]
        [Description("Checar eleições dos reinos")]
        private static void OnForceCheck(CommandEventArgs e)
        {
            ReinoElectionsSystem.CheckState();
            ReinoElectionsSystem.Save();
            e.Mobile.SendMessage("Checagem dos reinos executada.");
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------

        [Usage("darurna <0-3>")]
        [Description("Cria uma urna de uma das cidades")]
        private static void OnGiveUrna(CommandEventArgs e)
        {
            if (e.Length != 1)
            {
                e.Mobile.SendMessage("Uso: [darurna <0-3>");
                return;
            }

            int cityId;
            if (!Int32.TryParse(e.Arguments[0], out cityId) || cityId < 0 || cityId >= ReinoElectionsSystem.CityNames.Length)
            {
                e.Mobile.SendMessage("Cidade inválida. Use 0, 1, 2 ou 3.");
                return;
            }

            UrnaDoReino urna = new UrnaDoReino(cityId);
            e.Mobile.AddToBackpack(urna);
            e.Mobile.SendMessage("Urna criada para a cidade: {0}", ReinoElectionsSystem.GetCityName(cityId));
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------



        [Usage("Reinoinfo")]
        [Description("Mostra fase do reino")]
        private static void OnReinoInfo(CommandEventArgs e)
        {
            DateTime now = ReinoElectionsSystem.GetShardNow();
            e.Mobile.SendMessage("[Reinos] Agora: {0}", now.ToString("dd/MM/yyyy HH:mm"));
            e.Mobile.SendMessage("[Reinos] Fase: {0}", ReinoElectionsSystem.DescribePhase());
            e.Mobile.SendMessage("[Reinos] Ciclo ativo: {0} | Último anúncio: {1} | Última posse: {2}", ReinoElectionsSystem._activeElectionCycleId, ReinoElectionsSystem._lastAnnouncedCycleId, ReinoElectionsSystem._lastInstalledCycleId);

            foreach (KeyValuePair<int, ReinoCityData> kv in ReinoElectionsSystem._cities)
            {
                ReinoCityData city = kv.Value;
                ReinoCandidateEntry winner = ReinoElectionsSystem.GetWinningCandidate(city);
                string leader = winner == null ? "ninguém" : (winner.Nome + " (" + winner.Votes + " voto[s])");
                string gov = String.IsNullOrEmpty(city.GovernorName) ? "sem governador" : city.GovernorName;
                e.Mobile.SendMessage("- {0}: governador atual = {1} | líder da eleição = {2}", ReinoElectionsSystem.GetCityName(kv.Key), gov, leader);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------


        [Usage("reinofase <fechado|candidatura|votacao|resultado|governo>")]
        [Description("Mudar fase do reino manualmente")]
        private static void OnSetManualPhase(CommandEventArgs e)
        {
            string arg = e.Arguments[0].ToLower();

            if (e.Length != 1)
            {
                e.Mobile.SendMessage("Uso: [reinofase <fechado|candidatura|votacao|resultado|governo>");
                return;
            }

            switch (arg)
            {
                case "fechado":
                    ReinoElectionsSystem._manualPhase = ReinoFase.Fechado;
                    ReinoElectionsSystem._useManualPhase = true;
                    break;

                case "candidatura":
                    ReinoElectionsSystem._manualPhase = ReinoFase.Candidatura;
                    ReinoElectionsSystem._useManualPhase = true;
                    break;

                case "votacao":
                    ReinoElectionsSystem._manualPhase = ReinoFase.Votacao;
                    ReinoElectionsSystem._useManualPhase = true;
                    break;

                case "resultado":
                    ReinoElectionsSystem._manualPhase = ReinoFase.Resultado;
                    ReinoElectionsSystem._useManualPhase = true;
                    break;

                case "governo":
                    ReinoElectionsSystem._manualPhase = ReinoFase.Governo;
                    ReinoElectionsSystem._useManualPhase = true;
                    break;

                default:
                    e.Mobile.SendMessage("Fase inválida. Use: fechado, candidatura, votacao, resultado ou governo.");
                    return;
            }

            e.Mobile.SendMessage("Fase manual dos reinos ativada: {0}", ReinoElectionsSystem._manualPhase);
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------


        [Usage("Reinoautofase")]
        [Description("Reseta a fase manual e volta pra a fase normal de eleições")]
        private static void OnClearManualPhase(CommandEventArgs e)
        {
            ReinoElectionsSystem._useManualPhase = false;
            e.Mobile.SendMessage("Fase manual desligada. O sistema voltou a usar o calendário normal.");
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------------


        [Usage("ReinoRemoverAcesso <cityId>")]
        [Description("Remove acesso de governador + chave de um alvo.")]
        private static void ReinoRemoverAcesso_OnCommand(CommandEventArgs e)
        {
            if (e.Length != 1)
            {
                e.Mobile.SendMessage("Uso: [ReinoRemoverAcesso <cityId>");
                return;
            }

            int cityId;

            if (!Int32.TryParse(e.GetString(0), out cityId))
            {
                e.Mobile.SendMessage("CityId inválido.");
                return;
            }

            e.Mobile.SendMessage("Escolha o player.");
            e.Mobile.Target = new ReinoRemoverAcessoTarget(cityId);
        }

        private class ReinoDarAcessoTarget : Target
        {
            private readonly int _cityId;

            public ReinoDarAcessoTarget(int cityId) : base(12, false, TargetFlags.None)
            {
                _cityId = cityId;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;

                if (pm == null)
                {
                    from.SendMessage("O alvo precisa ser um player.");
                    return;
                }

                ReinoAccessHelper.GrantGovernorAccess(pm, _cityId, true);
                from.SendMessage("Acesso concedido.");
                pm.SendMessage("Você recebeu acesso ao controle do reino.");
            }
        }

        private class ReinoRemoverAcessoTarget : Target
        {
            private readonly int _cityId;

            public ReinoRemoverAcessoTarget(int cityId) : base(12, false, TargetFlags.None)
            {
                _cityId = cityId;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;

                if (pm == null)
                {
                    from.SendMessage("O alvo precisa ser um player.");
                    return;
                }

                ReinoAccessHelper.RevokeGovernorAccess(pm, _cityId);
                from.SendMessage("Acesso removido.");
                pm.SendMessage("Seu acesso ao controle do reino foi removido.");
            }
        }
    }
}

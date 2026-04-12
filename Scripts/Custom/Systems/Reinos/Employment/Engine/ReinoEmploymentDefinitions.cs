using System;
using System.Collections.Generic;

namespace Server.Custom.Reinos
{
    public class ReinoEmploymentRoleSeed
    {
        public ReinoCargoKind Kind;
        public string Title;
        public string Description;
        public int DefaultSalary;
        public int Hierarchy;
        public bool IsDefault;
        public bool IsRemovable;
        public bool IsEssential;
        public bool CanFinancial;
        public bool CanMilitary;
        public bool CanHire;
        public bool CanFire;
        public string LinkedConstructionKey;
        public int Count;
    }

    public class ReinoEmploymentOptionalTemplate
    {
        public int Index;
        public ReinoCargoKind Kind;
        public string DisplayTitle;
        public string CreatedTitle;
        public string Description;
        public int DefaultSalary;
        public int Hierarchy;
        public bool CanFinancial;
        public bool CanMilitary;
        public bool CanHire;
        public bool CanFire;
        public bool RepresentativeOnlyFinancial;
        public bool PostosOnlyMilitary;
    }

    public static class ReinoEmploymentDefinitions
    {
        public static string GetLeaderTitle(string cultureId)
        {
            switch ((cultureId ?? String.Empty).Trim().ToLowerInvariant())
            {
                case "kamay": return "Primeiro Ministro";
                case "matalun": return "Oráculo";
                case "sarangs": return "Líder Absoluto";
                case "zosteros": return "Presidente do Conselho";
                default: return "Líder";
            }
        }

        public static string GetLeaderDescription(string cultureId)
        {
            switch ((cultureId ?? String.Empty).Trim().ToLowerInvariant())
            {
                case "kamay": return "Chefe do governo kamay. Hierarquia 1. Pode governar o reino, mas depende dos ministros essenciais para liberar todas as decisões do governo.";
                case "matalun": return "Chefe do governo matalun. Hierarquia 1. Depende do sacerdote essencial para liberar todas as decisões do governo.";
                case "sarangs": return "Chefe do governo sarang. Hierarquia 1. Pode governar sozinho, sem depender de outros cargos essenciais.";
                case "zosteros": return "Chefe do governo zortero. Hierarquia 1. Depende dos conselheiros essenciais para liberar todas as decisões do governo.";
                default: return "Chefe do governo do reino.";
            }
        }

        public static List<ReinoEmploymentRoleSeed> GetDefaultRoleSeeds(string cultureId)
        {
            var list = new List<ReinoEmploymentRoleSeed>();

            list.Add(new ReinoEmploymentRoleSeed
            {
                Kind = ReinoCargoKind.Leader,
                Title = GetLeaderTitle(cultureId),
                Description = GetLeaderDescription(cultureId),
                DefaultSalary = 0,
                Hierarchy = 1,
                IsDefault = true,
                IsRemovable = false,
                IsEssential = false,
                CanFinancial = true,
                CanMilitary = true,
                CanHire = false,
                CanFire = false,
                LinkedConstructionKey = String.Empty,
                Count = 1
            });

            switch ((cultureId ?? String.Empty).Trim().ToLowerInvariant())
            {
                case "kamay":
                    list.Add(new ReinoEmploymentRoleSeed
                    {
                        Kind = ReinoCargoKind.MinisterEconomy,
                        Title = "Ministro da Economia",
                        Description = "Cargo essencial do governo kamay. Organiza as decisões econômicas do reino.",
                        DefaultSalary = 280,
                        Hierarchy = 2,
                        IsDefault = true,
                        IsRemovable = false,
                        IsEssential = true,
                        CanFinancial = true,
                        CanMilitary = false,
                        CanHire = false,
                        CanFire = false,
                        LinkedConstructionKey = String.Empty,
                        Count = 1
                    });
                    list.Add(new ReinoEmploymentRoleSeed
                    {
                        Kind = ReinoCargoKind.MinisterDefense,
                        Title = "Ministro da Defesa",
                        Description = "Cargo essencial do governo kamay. Organiza as decisões militares do reino.",
                        DefaultSalary = 280,
                        Hierarchy = 2,
                        IsDefault = true,
                        IsRemovable = false,
                        IsEssential = true,
                        CanFinancial = false,
                        CanMilitary = true,
                        CanHire = false,
                        CanFire = false,
                        LinkedConstructionKey = String.Empty,
                        Count = 1
                    });
                    break;
                case "matalun":
                    list.Add(new ReinoEmploymentRoleSeed
                    {
                        Kind = ReinoCargoKind.Priest,
                        Title = "Sacerdote",
                        Description = "Cargo essencial do governo matalun. O oráculo depende do sacerdote para governar.",
                        DefaultSalary = 0,
                        Hierarchy = 2,
                        IsDefault = true,
                        IsRemovable = false,
                        IsEssential = true,
                        CanFinancial = false,
                        CanMilitary = false,
                        CanHire = false,
                        CanFire = false,
                        LinkedConstructionKey = String.Empty,
                        Count = 1
                    });
                    break;
                case "zosteros":
                    list.Add(new ReinoEmploymentRoleSeed
                    {
                        Kind = ReinoCargoKind.CouncilMember,
                        Title = "Conselheiro",
                        Description = "Cargo essencial do governo zortero. O presidente do conselho depende dos conselheiros para governar.",
                        DefaultSalary = 0,
                        Hierarchy = 2,
                        IsDefault = true,
                        IsRemovable = false,
                        IsEssential = true,
                        CanFinancial = false,
                        CanMilitary = false,
                        CanHire = false,
                        CanFire = false,
                        LinkedConstructionKey = String.Empty,
                        Count = 2
                    });
                    break;
            }

            list.Add(new ReinoEmploymentRoleSeed
            {
                Kind = ReinoCargoKind.Ambassador,
                Title = "Embaixador",
                Description = "Pode agir em nome do reino servindo de representate em alguns assuntos militares e enconomicos.",
                DefaultSalary = 0,
                Hierarchy = 3,
                IsDefault = true,
                IsRemovable = false,
                IsEssential = false,
                CanFinancial = true,
                CanMilitary = true,
                CanHire = false,
                CanFire = false,
                LinkedConstructionKey = String.Empty,
                Count = 1
            });

            list.Add(new ReinoEmploymentRoleSeed
            {
                Kind = ReinoCargoKind.Dispatcher,
                Title = "Dispachante",
                Description = "Responsável por retirar e despachar recursos dos postos do reino.",
                DefaultSalary = 0,
                Hierarchy = 4,
                IsDefault = true,
                IsRemovable = false,
                IsEssential = false,
                CanFinancial = false,
                CanMilitary = false,
                CanHire = false,
                CanFire = false,
                LinkedConstructionKey = String.Empty,
                Count = 1
            });

            return list;
        }

        public static List<ReinoEmploymentOptionalTemplate> GetOptionalTemplates()
        {
            return new List<ReinoEmploymentOptionalTemplate>
            {
                new ReinoEmploymentOptionalTemplate
                {
                    Index = 0,
                    Kind = ReinoCargoKind.Ambassador,
                    DisplayTitle = "Embaixador",
                    CreatedTitle = "Embaixador",
                    Description = "Pode agir em nome do reino servindo de representate em alguns assuntos militares e enconomicos.",
                    DefaultSalary = 200,
                    Hierarchy = 5,
                    CanFinancial = true,
                    CanMilitary = true,
                    CanHire = false,
                    CanFire = false,
                    RepresentativeOnlyFinancial = true,
                    PostosOnlyMilitary = true
                },
                new ReinoEmploymentOptionalTemplate
                {
                    Index = 1,
                    Kind = ReinoCargoKind.Dispatcher,
                    DisplayTitle = "Dispachante",
                    CreatedTitle = "Dispachante Auxiliar",
                    Description = "Cargo auxiliar focado em retirada de recursos dos postos.",
                    DefaultSalary = 200,
                    Hierarchy = 5,
                    CanFinancial = false,
                    CanMilitary = false,
                    CanHire = false,
                    CanFire = false,
                    RepresentativeOnlyFinancial = false,
                    PostosOnlyMilitary = false
                }
            };
        }

        public static ReinoEmploymentOptionalTemplate GetOptionalTemplate(int index)
        {
            List<ReinoEmploymentOptionalTemplate> list = GetOptionalTemplates();
            for (int i = 0; i < list.Count; i++)
                if (list[i].Index == index)
                    return list[i];
            return null;
        }
    }
}

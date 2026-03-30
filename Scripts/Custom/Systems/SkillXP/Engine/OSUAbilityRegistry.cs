using Server.Custom.Systems.SkillXP.Engine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Server.Custom.Systems.SkillXP
{
    public static class OSUAbilityRegistry
    {
        public static void RegisterAll()
        {
            // Procura TODAS as classes que implementam IOSUAbility e instancia
            Assembly asm = Assembly.GetExecutingAssembly();
            Type iface = typeof(IOSUAbility);

            foreach (Type t in asm.GetTypes())
            {
                try
                {
                    if (t == null || t.IsAbstract || t.IsInterface)
                        continue;

                    if (!iface.IsAssignableFrom(t))
                        continue;

                    // precisa ter construtor vazio
                    IOSUAbility ab = Activator.CreateInstance(t) as IOSUAbility;
                    if (ab == null || ab.Definition == null)
                        continue;

                    OSUAbilitySystem.AddAbility(ab);
                }
                catch
                {
                    // ignora tipos que não conseguem instanciar
                }
            }
        }
    }
}

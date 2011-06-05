using System;
using System.Collections.Generic;
using System.Reflection;
using Amude.Domain;
using Amude.Domain.Attribute;
using Amude.Global;
using Amude.Graphics;


namespace Amude.Core
{
    internal static class Bundle
    {
        private static Dictionary<String, String> texts;
        private static ProtectedDictionary<String, Character> characters;
        private static ProtectedDictionary<String, Entity> environment;
        private static ProtectedDictionary<String, Entity> projectiles;
        private static ProtectedDictionary<SpecialAbility, Animation> specialAbilities;
        private static ProtectedDictionary<String, Terrain> terrains;

        public static Dictionary<String, String> Texts
        {
            get
            {
                return texts;
            }
        }

        public static ProtectedDictionary<String, Character> Characters
        {
            get
            {
                return characters;
            }
        }

        public static ProtectedDictionary<String, Entity> Environment
        {
            get
            {
                return environment;
            }
        }

        public static ProtectedDictionary<String, Entity> Projectiles
        {
            get
            {
                return projectiles;
            }
        }

        public static ProtectedDictionary<SpecialAbility, Animation> SpecialAbilities
        {
            get
            {
                return specialAbilities;
            }
        }

        public static ProtectedDictionary<String, Terrain> Terrains
        {
            get
            {
                return terrains;
            }
        }

        public static void Load(ProgressBarCallBack callback)
        {
            callback(Constants.CONFIG_FILES_COUNT);
            characters = new ProtectedDictionary<String, Character>();
            environment = new ProtectedDictionary<String, Entity>();
            projectiles = new ProtectedDictionary<String, Entity>();
            specialAbilities = new ProtectedDictionary<SpecialAbility, Animation>();
            terrains = new ProtectedDictionary<String, Terrain>();

            texts = IO.LoadTexts(callback);

            Dictionary<String, Terrain> loadedTerrains = IO.LoadTerrains();
            foreach (KeyValuePair<String, Terrain> kv in loadedTerrains)
            {
                terrains.Add(kv.Key, kv.Value);
            }

            Dictionary<String, Entity> loadedEnvironment = IO.LoadEnvironment(callback);
            foreach (KeyValuePair<String, Entity> kv in loadedEnvironment)
            {
                environment.Add(kv.Key, kv.Value);
            }

            Dictionary<String, Entity> loadedProjectiles = IO.LoadProjectiles(callback);
            foreach (KeyValuePair<String, Entity> kv in loadedProjectiles)
            {
                projectiles.Add(kv.Key, kv.Value);
            }

            Dictionary<SpecialAbility, Animation> loadedSpecialAbilities;
            loadedSpecialAbilities = IO.LoadSpecialAbilities(callback);
            foreach (KeyValuePair<SpecialAbility, Animation> kv in loadedSpecialAbilities)
            {
                specialAbilities.Add(kv.Key, kv.Value);
            }

            Dictionary<String, Character> loadedCharacters = IO.LoadCharacters(callback);
            foreach (KeyValuePair<String, Character> kv in loadedCharacters)
            {
                characters.Add(kv.Key, kv.Value);
            }
        }

        public static SpecialAbility GetSpecialAbility(String rootName)
        {
            foreach (SpecialAbility sa in specialAbilities.Keys)
            {
                if (sa.RootName == rootName)
                {
                    return sa;
                }
            }

            throw new Exception("SpecialAbility não encontrada.");
        }

        public static Animation GetSpecialAbilityAnimation(String rootName)
        {
            return Bundle.specialAbilities[GetSpecialAbility(rootName)].Clone();
        }

        public static Animation GetAnimation(string entityClass, string entityName, AnimationType animationType)
        {
            PropertyInfo bufferInfo = (typeof(Bundle)).GetProperty(entityClass);
            Object entityBuffer = bufferInfo.GetValue(null, null);
            return ((ProtectedDictionary<String, Entity>)entityBuffer)[entityName].GetDefinedAnimation(animationType);
        }

        internal sealed class ProtectedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
            where TValue : ICloneable<TValue>
        {
            new public TValue this[TKey key]
            {
                get
                {
                    return base[key].Clone();
                }
                set
                {
                    base[key] = value;
                }
            }

            new public bool TryGetValue(TKey key, out TValue value)
            {
                bool ret = base.TryGetValue(key, out value);
                if (value != null)
                {
                    value = value.Clone();
                }
                return ret;
            }
        }
    }

}

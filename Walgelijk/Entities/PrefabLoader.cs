using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// Entity (de)serialisation utility struct
    /// </summary>
    public struct PrefabLoader
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
        };

        /// <summary>
        /// Deserialise a JSON representation of <see cref="Prefab"/> from a file
        /// </summary>
        public static Prefab Load(string path)
        {
            string json = File.ReadAllText(path);
            var prefab = JsonConvert.DeserializeObject<Prefab>(json, settings);
            return prefab;
        }

        /// <summary>
        /// Serialise a <see cref="Prefab"/> to a file
        /// </summary>
        public static void Save(Prefab prefab, string path)
        {
            string json = JsonConvert.SerializeObject(prefab, settings);
            File.WriteAllText(path, json);
        }
    }
}

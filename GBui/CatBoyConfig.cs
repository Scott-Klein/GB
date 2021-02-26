using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBui
{
    public static class CatBoyConfig
    {
        private const string FILE_NAME = "config.json";

        public static List<RomListItem> DeserialiseLibrary()
        {
            string configStr;
            if (File.Exists(FILE_NAME))
            {
                configStr = File.ReadAllText(FILE_NAME);
            }
            else
            {
                File.CreateText(FILE_NAME);
                configStr = "";
            }
            var deserialisationResult = JsonConvert.DeserializeObject<List<RomListItem>>(configStr);
            return deserialisationResult is not null ? deserialisationResult : new List<RomListItem>();
        }

        internal static void SaveLibrary(List<RomListItem> roms)
        {
            string configStr = JsonConvert.SerializeObject(roms);

            File.WriteAllTextAsync(FILE_NAME, configStr);
        }
    }
}

using BepInEx.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

//100% of this code was stolen from HIFU. <3
namespace BransItems.Modules.Utils
{
    public class ConfigManager
    {
        internal static bool ConfigChanged = false;
        internal static bool VersionChanged = false;

        public static T HandleConfig<T>(ConfigEntryBase entry, ConfigFile config, string name)
        {
            var method = typeof(ConfigFile).GetMethods().Where(x => x.Name == nameof(ConfigFile.Bind)).First();
            method = method.MakeGenericMethod(typeof(T));

            var newConfigEntry = new object[] { new ConfigDefinition(Regex.Replace(config.ConfigFilePath, "\\W", "") + " : " + entry.Definition.Section, name), entry.DefaultValue, new ConfigDescription(entry.Description.Description) };

            var backupVal = (ConfigEntryBase)method.Invoke(config, newConfigEntry);

            if (BransItems._preVersioning) entry.BoxedValue = entry.DefaultValue;

            if (!ConfigEqual(backupVal.DefaultValue, backupVal.BoxedValue))
            {
                if (VersionChanged)
                {
                    entry.BoxedValue = entry.DefaultValue;
                    backupVal.BoxedValue = backupVal.DefaultValue;
                }
            }
            return default;
        }

        private static bool ConfigEqual(object a, object b)
        {
            if (a.Equals(b)) return true;
            float fa, fb;
            if (float.TryParse(a.ToString(), out fa) && float.TryParse(b.ToString(), out fb) && Mathf.Abs(fa - fb) < 0.0001) return true;
            return false;
        }

        public static T ConfigOption<T>(string section, string key, T defaultvalue, string description)
        {
            var config = BransItems.AugConfig.Bind<T>(section, key, defaultvalue, description);
            ConfigManager.HandleConfig<T>(config, BransItems.AugBackupConfig, key);

            return config.Value;
        }
    }
}

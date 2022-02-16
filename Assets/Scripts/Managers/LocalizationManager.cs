using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace NoZ.Zisle
{
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        private static StringTable _stringTable = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void LoadTable()
        {
            if (null == _stringTable)
            {
                var loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync("UI");
                loadingOperation.WaitForCompletion();
                _stringTable = loadingOperation.Result;
            }
        }

        public static string GetString(string key)
        {
            if (key == null)
                return "<null>";

            if (null == _stringTable)
                return key;

            return _stringTable.GetEntry(key)?.Value ?? key;
        }
    }

    public static class LocalizationManagerExtensions
    {
        public static string Localized(this string key) => LocalizationManager.GetString(key);
    }

}

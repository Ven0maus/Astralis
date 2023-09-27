using Newtonsoft.Json;
using System.IO;

namespace Astralis.Configuration
{
    internal static class GameConfiguration
    {
        public static T Load<T>()
        {
            var type = typeof(T);
            var path = Path.Combine(Constants.Configuration.JsonFilesPath, type.Name + ".json");
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}

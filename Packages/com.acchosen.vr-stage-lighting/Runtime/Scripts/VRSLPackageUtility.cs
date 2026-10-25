#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.IO;
using Newtonsoft.Json;

namespace VRSL.EditorScripts
{
    public struct Package
    {
        [JsonProperty("version")]
        public string version;
    }
    
    public class VRSLPackageUtility
    {
        private static VRSLPackageUtility _instance;

        public static VRSLPackageUtility GetInstance()
        {
            if (_instance == null)
            {
                _instance = new VRSLPackageUtility();
            }

            return _instance;
        }

        public static Package ReadPackage(string pathToFile)
        {
            if (!File.Exists(pathToFile))
            {
                throw new FileNotFoundException(pathToFile);
            }

            var value = File.ReadAllText(pathToFile);
            
            return JsonConvert.DeserializeObject<Package>(value);
        }
    }
}
#endif
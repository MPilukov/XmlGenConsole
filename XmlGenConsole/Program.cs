using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace XmlGenConsole
{
    internal class Program
    {
        private static readonly Dictionary<string, string> WebResourceTypes = new Dictionary<string, string>
        {
            {"html", "1"}, 
            {"css", "2"},
            {"js", "3"},
            {"png", "5"},
        };

        public static async Task Main(string[] args)
        {
            if (args.Length < 4)
            {
                return;
            }
            
            var fileExt = args[0];
            var fileName = args[1];
            var fullDirName = args[2];
            var fullSolutionDir = args[3];

            if (!fullSolutionDir.EndsWith("ModulBank-CRM\\Web Resources\\",
                StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (!WebResourceTypes.TryGetValue(fileExt.ToLower(), out var webResourceType))
            {
                return;
            }
            
            var dirFile = fullDirName.Replace(fullSolutionDir, "")
                .Replace("Web Resources\\Root\\", "");
            var filePath = $"{dirFile}\\{fileName}";
            
            var webResInCrmSolFolder = fullSolutionDir.Replace("Web Resources", "CrmSolutions\\WebResources");
            var filePathInCrmSolFolder = $"{webResInCrmSolFolder}\\WebResources\\{filePath}.data.xml";

            var trace = GetTracer(filePath);
            
            await CreateFileIfNotFound(dirFile, filePath, webResInCrmSolFolder, filePathInCrmSolFolder, trace);
        }

        private static Action<string> GetTracer(string fileName)
        {
            var logsFileName = $"{fileName}"
                .Replace("\\", "")
                .Replace(":", "")
                .Replace(".", "");
            
            var trace = new Action<string>(s =>
            {
                Console.WriteLine(s);
                if (!Directory.Exists("logs"))
                {
                    Directory.CreateDirectory("logs");
                }
                File.AppendAllText($"logs/{logsFileName}.txt", s + Environment.NewLine);
            });

            return trace;
        }
        
        private static async Task CreateFileIfNotFound(string dirFile, string filePath, 
            string webResInCrmSolFolder, string filePathInCrmSolFolder, Action<string> trace)
        {
            var isExistInRepo = IsExist(filePathInCrmSolFolder, trace);
            if (isExistInRepo == null)
            {
                trace($"Не удалось проверить наличие файла {filePathInCrmSolFolder}.");
                return;
            }

            if (isExistInRepo.Value)
            {
                trace($"Файл уже существует {filePathInCrmSolFolder}.");
                return;
            }

            var isExistInCrm = await IsExistInCrm(filePath, trace);
            if (isExistInCrm == null)
            {
                trace("Не удалось подключиться к CRM.");
                return;
            }

            WebResource resource;
            
            if (isExistInCrm.Value)
            {
                resource = await GetWebResource(filePath, trace);
                if (resource == null)
                {
                    trace($"Не удалось получить файл из CRM {filePath}.");
                    return;
                }
            }
            else
            {
                resource = new WebResource
                {
                    
                };
            }

            var id = resource.WebResourceId;
            resource.FileName = $"/WebResources/{resource.Name.Replace("/", "").Replace(".", "")}{id}";
            resource.WebResourceId = $"{{{id}}}";
            
            CreateXml(filePathInCrmSolFolder, resource, trace);
        }
        
        private static void CreateXml(string fileName, WebResource webResource, Action<string> trace)
        {
            try
            {
                var dir = Path.GetDirectoryName(fileName);
                if (string.IsNullOrWhiteSpace(dir))
                {
                    trace($"Не указана папка для файла : {fileName}");
                    return;
                }
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                var xns = new XmlSerializerNamespaces();
                xns.Add(string.Empty, string.Empty);
                
                var formatter = new XmlSerializer(typeof(WebResource));
 
                using (var fs = new StreamWriter(fileName, false, new UTF8Encoding()))
                {
                    formatter.Serialize(fs, webResource, xns);
                }
            }
            catch (Exception e)
            {
                trace($"CreateXml error : {e}");
            }
        }
        private static bool? IsExist(string filePath, Action<string> trace)
        {
            try
            {
                return File.Exists(filePath);
            }
            catch (Exception e)
            {
                trace($"IsExist error : {e}");
                return null;
            }
        }
        private static async Task<bool?> IsExistInCrm(string filePath, Action<string> trace)
        {
            try
            {
                Console.WriteLine($"Check exist file: {filePath}");
                
                var url = ConfigurationManager.AppSettings.Get("crm.baseUrl");
                var userName = ConfigurationManager.AppSettings.Get("crm.userName");
                var password = ConfigurationManager.AppSettings.Get("crm.password");

                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    return null;
                }

                var credentials = new NetworkCredential(userName, password);
                var handler = new HttpClientHandler { Credentials = credentials };
                
                using (var client = new HttpClient(handler))
                {
                    var baseUri = new Uri(url);
                    var uri = new Uri(baseUri, url + "WebResources/" + filePath);
                    
                    var response = await client.GetAsync(uri);
                    Console.WriteLine($"Response StatusCode: {(int)response.StatusCode}");

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return null;
                        case HttpStatusCode.OK:
                            return true;
                        case HttpStatusCode.NotFound:
                            return false;
                        default:
                            return false;
                    }
                }

            }
            catch (Exception e)
            {
                trace($"IsExistInCrm error : {e}");
                return null;
            }
        }
        private static async Task<WebResource> GetWebResource(string filePath, Action<string> trace)
        {
            try
            {
                var url = ConfigurationManager.AppSettings.Get("crm.baseUrl");
                var userName = ConfigurationManager.AppSettings.Get("crm.userName");
                var password = ConfigurationManager.AppSettings.Get("crm.password");

                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    return null;
                }

                var credentials = new NetworkCredential(userName, password);
                var handler = new HttpClientHandler { Credentials = credentials };
                
                using (var client = new HttpClient(handler))
                {
                    var baseUri = new Uri(url);
                    var uri = new Uri(baseUri, 
                        url + "/api/data/v8.2/webresourceset?" +
                        "$select=canbedeleted,componentstate,createdon,dependencyxml,description,displayname," +
                        "introducedversion,isavailableformobileoffline,iscustomizable,isenabledformobileclient," +
                        "ishidden,ismanaged,name,webresourceid,webresourcetype" +
                        "&$filter=name eq '" + filePath.Replace("\\", "/") + "'");
                    
                    var response = await client.GetAsync(uri);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            var content = await response.Content.ReadAsStringAsync();

                            var obj = JsonConvert.DeserializeObject<InnerODataResponse>(content);

                            if (obj?.Value?.Any() ?? false)
                            {
                                var first = obj.Value[0];
                                return new WebResource
                                {
                                    WebResourceId = first.WebResourceId,
                                    WebResourceType = first.WebResourceType,
                                    Name = first.Name,
                                    DisplayName = first.DisplayName,
                                    DependencyXml = first.DependencyXml,
                                    IntroducedVersion = first.IntroducedVersion,
                                    IsCustomizable = first.IsCustomizable.Value ? (byte)1 : (byte)0,
                                    IsHidden = first.IsHidden.Value ? (byte)1 : (byte)0,
                                    CanBeDeleted = first.CanBeDeleted.Value ? (byte)1 : (byte)0,
                                    IsAvailableForMobileOffline = first.IsAvailableForMobileOffline ? (byte)1 : (byte)0,
                                    IsEnabledForMobileClient = first.IsEnabledForMobileClient ? (byte)1 : (byte)0,
                                };
                            }
                            
                            return null;
                        default:
                            return null;
                    }
                }
            }
            catch (Exception e)
            {
                trace($"GetWebResource error : {e}");
                return null;
            }
        }
    }
}
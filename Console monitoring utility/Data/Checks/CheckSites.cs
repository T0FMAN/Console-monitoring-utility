using Console_monitoring_utility.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_monitoring_utility.Data.Checks
{
    public class CheckSites : ICheckConnections
    {
        public List<string> Logger { get; set; } = new();

        public async Task CheckAvailability(List<string> values)
        {
            var header = $"Тест подключения до сайтов ({values.Count}х)..\n";

            Console.WriteLine(header);

            using (var client = new HttpClient())
            {
                foreach (var url in values)
                {
                    try
                    {
                        using (var response = await client.GetAsync(url))
                        {
                            var code = (int)response.StatusCode;

                            var textResponse = string.Empty;

                            textResponse = code switch
                            {
                                200 => $"Сайт {url} доступен (код 200)",
                                _ => $"Ошибка доступа к {url} : Код {code}",
                            };
                            Log(textResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"{url} : {ex.Message}");
                    }
                }
            }
        }

        public void Log(string value)
        {
            Logger.Add(value);
            Console.WriteLine(value);
        }
    }
}

using Console_monitoring_utility;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text;

var text = string.Empty;

var pathParams = "Files/parameters.json";
var pathResult = "Files/result.json";
var pathLogger = "Files/log.txt";

if (args.Length > 0)
{
    try
    {
        using (StreamReader sr = new(pathResult, Encoding.UTF8))
            text = await sr.ReadToEndAsync();

        var data = JsonConvert.DeserializeObject<DataJson>(text);

        Console.WriteLine(TemplateLog(data));
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
else
{
    var time = $"{DateTime.Now:dd MMMM yyyy HH:mm:ss}";

    using (StreamReader sr = new(pathParams))
        text = await sr.ReadToEndAsync();

    var data = JsonConvert.DeserializeObject<DataJson>(text);

    var statusList = await CheckAvailabilitySites(data!.Sites);
    statusList.ForEach(n => Console.WriteLine(n));

    var statusDb = await CheckConnectionToDB(data!.StringConnection);
    Console.WriteLine(statusDb);

    await Log();

    async Task Log()
    {
        var dataJson = new DataJson
        {
            DateTime = time,
            Sites = statusList,
            StringConnection = statusDb,
        };

        var json = JsonConvert.SerializeObject(dataJson);

        using (StreamWriter sw = new(pathResult, false))
            await sw.WriteLineAsync(json);

        using (StreamWriter sw = new(pathLogger, true))
        {
            var text = TemplateLog(dataJson);

            await sw.WriteLineAsync(text);
        }
    }

    static async Task<List<string>> CheckAvailabilitySites(List<string> sites)
    {
        var checkedList = new List<string>();

        using (HttpClient client = new())
        {
            foreach (var url in sites)
            {
                try
                {
                    using (var response = await client.GetAsync("https://" + url))
                    {
                        var code = (int)response.StatusCode;

                        var textResponse = string.Empty;

                        switch (code)
                        {
                            case 200:
                                textResponse = $"Сайт {url} доступен (код 200)\n";
                                break;

                            default:
                                textResponse = $"Ошибка доступа к {url} : Код {code}\n";
                                break;
                        }

                        checkedList.Add(textResponse);
                    }
                }
                catch (Exception ex)
                {
                    checkedList.Add(ex.Message);
                }
            }
        }

        return checkedList;
    }

    static async Task<string> CheckConnectionToDB(string stringConn)
    {
        Console.WriteLine($"Соединение с базой данных по адресу '{stringConn}'...\n");

        using (var connection = new SqlConnection(stringConn))
        {
            try
            {
                connection.OpenAsync();

                await Task.Delay(2500);

                if (connection.State == ConnectionState.Open)
                    return "Соединение прошло успешно";
                else
                    return "Сервер базы данных не отвечает, либо задано неправильное подключение..";

            }
            catch (SqlException ex)
            {
                return $"Ошибка SQL: {ex.Message}";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}

static string TemplateLog(DataJson data)
{
    if (data is null)
        return "Проверок подключения еще не проводилось..";

    var dateTemp = $"Время проверки: {data.DateTime}\n";
    var siteTemp = $"Результат проверки соединений до сайтов:\n";
    var dbTemp = $"Результат проверки соединения к базе данных: {data.StringConnection}\n";

    data.Sites.ForEach(n => siteTemp += $"{n}\n");

    return $"{dateTemp}\n{siteTemp}\n{dbTemp}";
}
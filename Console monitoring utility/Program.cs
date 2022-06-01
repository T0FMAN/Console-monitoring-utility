using Console_monitoring_utility;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Text.Json;

var text = string.Empty;
var path = args[0];

using (StreamReader sr = new(path))
    text = await sr.ReadToEndAsync();

var data = JsonSerializer.Deserialize<DataJson>(text);

var statusList = await CheckAvailabilitySites(data!.Sites);
var testConn = await CheckConnectionToDB(data!.StringConnection);

statusList.ForEach(n => Console.WriteLine($"{n}\n"));
Console.WriteLine(testConn);

static async Task<List<string>> CheckAvailabilitySites(List<string> sites)
{
    var checkedList = new List<string>();

    using (HttpClient client = new())
    {
        foreach (var url in sites)
        {
            using (var response = await client.GetAsync("https://" + url))
            {
                var code = (int)response.StatusCode;

                var textResponse = string.Empty;

                switch (code)
                {
                    case 200:
                        textResponse = $"Сайт {url} доступен (код 200)";
                        break;

                    default:
                        textResponse = $"Ошибка доступа к {url} : Код {code}";
                        break;
                }

                checkedList.Add(textResponse);
            }
        }
    }
    return checkedList;
}

static async Task<bool> CheckConnectionToDB(string stringConn)
{
    using (var connection = new SqlConnection(stringConn))
    {
        try
        {
            await connection.OpenAsync();

            //await Task.Delay(1500);

            //if (conn.State == ConnectionState.Open)
            //    return true;
            //else throw new Exception();

            return true;
        }
        catch
        {
            return false;
        }
    }
}

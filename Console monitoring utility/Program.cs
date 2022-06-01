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
statusList.ForEach(n => Console.WriteLine($"{n}\n"));

CheckConnectionToDB(data!.StringConnection);

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
                            textResponse = $"Сайт {url} доступен (код 200)";
                            break;

                        default:
                            textResponse = $"Ошибка доступа к {url} : Код {code}";
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

static async void CheckConnectionToDB(string stringConn)
{
    Console.WriteLine($"Соединение с базой данных по адресу '{stringConn}'...\n");

    using (var connection = new SqlConnection(stringConn))
    {
        try
        {
            connection.OpenAsync();

            await Task.Delay(2500);

            if (connection.State == ConnectionState.Open)
                Console.WriteLine("Соединение прошло успешно");
            else
                Console.WriteLine("Сервер базы данных не отвечает, либо задано неправильное подключение..");

        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Ошибка SQL: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
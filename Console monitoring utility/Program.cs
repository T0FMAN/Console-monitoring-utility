using Console_monitoring_utility;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text;

var text = string.Empty;

var pathParams = "Files/parameters.json";
var pathResult = "Files/result.json";
var pathLogger = "Files/log.txt";

if (args.Length > 0) // запуск с параметром
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
else // запуск без параметров
{
    var time = $"{DateTime.Now:dd MMMM yyyy HH:mm:ss}";

    using (StreamReader sr = new(pathParams))
        text = await sr.ReadToEndAsync();

    var data = JsonConvert.DeserializeObject<DataJson>(text);

    var statusList = await CheckAvailabilitySites(data!.Sites);
    statusList.ForEach(n => Console.WriteLine(n));

    var statusDb = await CheckConnectionToDB(data!.StringConnection);
    Console.WriteLine(statusDb);

    SendMail();
    await Log();

    void SendMail()
    {
        var mail = ""; // почтовый адрес от учетной записи, с которой будет отправлять письмо
        var password = ""; // пароль от учетной записи почтового сервиса

        try
        {
            var fromAdress = new MailAddress(mail, "Утилита тестирования соединения");
            var toAdress = new MailAddress(""); // кому будет отправлено письмо

            MailMessage message = new(fromAdress, toAdress);

            message.Subject = "Новая проверка соединения";
            message.Attachments.Add(new Attachment("Files/result.json"));

            var smtpClient = new SmtpClient // клиент для отправки почты
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mail, password),
            };

            smtpClient.Send(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    // логирование результата
    async Task Log()
    {
        var dataJson = new DataJson
        {
            DateTime = time,
            Sites = statusList,
            StringConnection = statusDb,
        };

        var json = JsonConvert.SerializeObject(dataJson);

        // логирование последней проверки в файл result.json
        using (StreamWriter sw = new(pathResult, false))
            await sw.WriteLineAsync(json);

        // логирование в общий файл log.txt
        using (StreamWriter sw = new(pathLogger, true))
        {
            var text = TemplateLog(dataJson);

            await sw.WriteLineAsync(text);
        }
    }
    // проверка доступа к сайтами
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
    // проверка соединения с БД
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
// шаблон для логирования
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
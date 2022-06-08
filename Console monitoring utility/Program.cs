using Console_monitoring_utility.Data;
using Console_monitoring_utility.Data.Checks;
using Console_monitoring_utility.Extensions;
using System.Net;
using System.Net.Mail;

var pathParams = "Files/parameters.json";
var pathResult = "Files/result.json";
var pathLogger = "Files/log.txt";
var pathMailer = "Files/mailSettings.json";

if (args.Length > 0) // запуск с параметром
{
    try
    {
        var value = await pathResult.GetDataFile();

        if (!value.Item1)
        {
            Console.WriteLine(value.Item2);
            return;
        }

        var result = value.Item2.Deserialize(ClassType.ParamsCheck) as ParamsCheck;

        var text = result!.TemplateLog(true);

        Console.WriteLine(text);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
else // запуск без параметров
{
    var time = $"{DateTime.Now:dd MMMM yyyy HH:mm:ss}";
    // получение параметров проверки соединений
    var dataParams = await pathParams.GetDataFile();

    if (!dataParams.Item1)
    {
        Console.WriteLine(dataParams.Item2);
        return;
    }
    // десереализация параметров в класс
    var parameters = dataParams.Item2.Deserialize(ClassType.ParamsCheck) as ParamsCheck;

    var sites = parameters!.Sites;
    var stringsConn = parameters!.StringsConnection;

    var checkDb = new CheckDbs();
    await checkDb.CheckAvailability(stringsConn); // проверка соединений до БД

    var checkSites = new CheckSites();
    await checkSites.CheckAvailability(sites); // проверка соединений до сайтов

    var data = new ParamsCheck
    {
        DateTime = time,
        Sites = checkSites.Logger,
        StringsConnection = checkDb.Logger,
    };

    var text = data.TemplateLog(); // шаблон для логирования
    var json = data.Serialize(); // текст в формат json для записи в файл

    var log = await pathLogger.WriteDataFile(text, true); // запись файла в общие результаты
    Console.WriteLine("\n" + log);

    var res = await pathResult.WriteDataFile(json, false); // запись в файл с последней проверкой
    Console.WriteLine("\n" + res);

    var mailData = await pathMailer.GetDataFile(); // получение текста файла настроек почты

    if (!mailData.Item1)
    {
        Console.WriteLine(mailData.Item2);
        return;
    }

    try
    {
        // получение данных почтового клиента, адресатов из файла
        var mail = mailData.Item2.Deserialize(ClassType.Mailer) as Mailer;
        // добавление файла во вложения письма
        if (mail.Message != null)
            mail.Message.Attachments.Add(new Attachment(pathResult));
        else
        {
            Console.WriteLine("Проблемы с созданием сообщения..");
            return;
        }
        mail.SmtpClient.Send(mail.Message);

        Console.WriteLine($"Письмо успешно разослано адресатам ({mail.Message.To.Count}x)");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
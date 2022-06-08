using Console_monitoring_utility.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_monitoring_utility.Extensions
{
    public enum ClassType
    {
        Mailer,
        ParamsCheck
    }

    public static class Extensions
    {
        public static string Serialize(this ParamsCheck paramsCheck)
        {
            try
            {
                return JsonConvert.SerializeObject(paramsCheck)!;
            }
            catch (Exception ex)
            {
                return ex.TempErrorLog();
            }
        }

        public static object Deserialize(this string data, ClassType classType)
        {
            try
            {
                switch (classType)
                {
                    case ClassType.Mailer:
                        return JsonConvert.DeserializeObject<Mailer>(data)!;

                    case ClassType.ParamsCheck:
                        return JsonConvert.DeserializeObject<ParamsCheck>(data)!;

                        default: throw new Exception("Неизвестный тип");
                }
            }
            catch (Exception ex) 
            {
                return ex.TempErrorLog();
            }
        }

        public static async Task<Tuple<bool, string>> GetDataFile(this string path)
        {
            var data = string.Empty;

            try
            {
                using (var sr = new StreamReader(path))
                    data = await sr.ReadToEndAsync();

                if (data is null)
                    return new Tuple<bool, string>(false, "Файл пустой");
                else
                    return new Tuple<bool, string>(true, data);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.TempErrorLog());
            }
        }

        public static async Task<string> WriteDataFile(this string path, string data, bool append)
        {
            try
            {
                using (var sw = new StreamWriter(path, append))
                    await sw.WriteLineAsync(data);

                return $"Файл по пути {path} успешно записан";
            }
            catch (Exception ex)
            {
                var error = ex.TempErrorLog();

                return "Перезапись файла неуспешна.." + error;
            }
        }

        public static string TemplateLog(this ParamsCheck data, bool isArgs = false)
        {
            var dateTemp = $"Время проверки: {data.DateTime}\n";
            var siteTemp = $"Результат проверки соединений до сайтов({data.Sites.Count}х):\n";
            var dbTemp = $"Результат проверки соединения к базам данных({data.StringsConnection.Count}х):\n";
            var split = "********************************";

            data.Sites.ForEach(n => siteTemp += $"{n}\n");
            data.StringsConnection.ForEach(n => dbTemp += $"{n}\n");

            if (isArgs)
                split = string.Empty;

            return $"{dateTemp}\n{siteTemp}\n{dbTemp}" + split;
        }

        public static string TempErrorLog(this Exception ex)
        {
            return "Ошибка: " + ex.Message;
        }
    }
}

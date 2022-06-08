using Console_monitoring_utility.Extensions;
using Console_monitoring_utility.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_monitoring_utility.Data.Checks
{
    public class CheckDbs : ICheckConnections
    {
        public List<string> Logger { get; set; } = new();

        public async Task CheckAvailability(List<string> values)
        {
            var header = $"Тест подключений к базам данных ({values.Count} БД, тайм-аут 2,5с.)..\n";

            Console.WriteLine(header);

            foreach (var str in values)
            {
                var tempStr = $"Подключение к базе данных по адресу: {str}..";

                Console.WriteLine(tempStr);

                using (var connection = new SqlConnection(str))
                {
                    try
                    {
                        connection.OpenAsync();

                        await Task.Delay(2500);

                        var stateSucces = "Соединение прошло успешно\n";
                        var stateError = "Сервер базы данных не отвечает, либо задано неправильное подключение\n";

                        if (connection.State == ConnectionState.Open)
                        {
                            Console.WriteLine(stateSucces);
                            Logger.Add($"{str} - {stateSucces}");
                        }
                        else
                        {
                            Console.WriteLine(stateError);
                            Logger.Add($"{str} - {stateError}");
                        }
                    }
                    catch (SqlException ex)
                    {
                        Log($"Ошибка SQL: {ex.Message}\n");
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message + '\n');
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

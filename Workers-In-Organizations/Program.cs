using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

public class Organization
{
    public List<Worker> Workers { get; set; }
    public List<Position> Positions { get; set; }
    public double AverageSalary { get; set; }
}

public class Position
{
    public string PositionName { get; private set; }
    public double Bonus { get; private set; }
    public double FixedSalary { get; private set; }
    public double HourlyRate { get; private set; }
    public Position(string _positionName, double _bonus, double _fixedSalary, double _hourlyRate)
    {
        PositionName = _positionName;
        Bonus = _bonus;
        FixedSalary = _fixedSalary;
        HourlyRate = _hourlyRate;
    }
}

[XmlInclude(typeof(Fixed))]
[XmlInclude(typeof(Hourly))]
public abstract class Worker : IComparable<Worker>
{
    public int Id { get; set; }
    public string Position { get; set; }
    public string Surname { get; set; }
    public string Name { get; set; }
    public string Patronymic { get; set; }
    public string Birthday { get; set; }
    public double Salary { get; set; }
    public abstract double CalculateSalary(List<Position> positions);

    public Worker() { }
    public Worker(string[] data, List<Position> positions)
    {
        Id = Convert.ToInt32(data[0]);
        Position = data[1];
        Surname = data[2];
        Name = data[3];
        Patronymic = data[4];
        Birthday = data[5];
        Salary = CalculateSalary(positions);
    }

    public int CompareTo(Worker cWorker)
    {
        return !Salary.Equals(cWorker.Salary) ?
        Salary.CompareTo(cWorker.Salary) < 0 ? 1 : -1 :
        Surname.Equals(cWorker.Surname) ?
        Name.Equals(cWorker.Name) ?
        Patronymic.CompareTo(cWorker.Patronymic) :
        Name.CompareTo(cWorker.Name) :
        Surname.CompareTo(cWorker.Surname);
    }
}

public class Fixed : Worker
{
    public Fixed() { }
    public Fixed(string[] data, List<Position> positions) : base(data, positions) { }
    public override double CalculateSalary(List<Position> positions)
    {
        var position = positions.Find(x => x.PositionName == Position);
        return position.FixedSalary + position.Bonus;
    }
}

public class Hourly : Worker
{
    public Hourly() { }
    public Hourly(string[] data, List<Position> positions) : base(data, positions) { }

    public override double CalculateSalary(List<Position> positions)
    {
        var position = positions.Find(x => x.PositionName == Position);
        return (20.8 * 8 * position.HourlyRate) + position.Bonus;
    }
}

internal class Program
{
    private static Organization company { get; set; }

    static int ChoiceActionMenu()
    {
        string choice = "";
        while (!Regex.IsMatch(choice, @"^[1-9]$"))
        {
            Console.WriteLine("\nВведите значение от 1 до 9:");
            choice = Console.ReadLine();
        }
        return Convert.ToByte(choice);
    }

    static int ChoiceTypeWorkersOrFormat()
    {
        string choice = "";
        while (!Regex.IsMatch(choice, @"^[1-2]$"))
        {
            Console.WriteLine("\nВведите 1 или 2:");
            choice = Console.ReadLine();
        }
        return Convert.ToByte(choice);
    }

    static void AddWorker(byte x, string[] data)
    {
        switch (x)
        {
            case 1:
                company.Workers.Add(new Fixed(data, company.Positions));
                break;
            case 2:
                company.Workers.Add(new Hourly(data, company.Positions));
                break;
        }
        Console.WriteLine("Сотрудник добавлен");
    }

    static void CheckWorker(byte x)
    {
        Console.WriteLine("\nВведите через пробел следующие данные о сотруднике: ID, должность, ФИО (тоже через пробел) и дату рождения\n" +
        "В организации имеются следующие должности: " + string.Join(", ", company.Positions.Select(x => x.PositionName)));
        try
        {
            string[] input = Console.ReadLine().Split(' ');
            if (company.Positions.Exists(x => x.PositionName == input[1]))
            {
                if (company.Workers.Count != 0)
                {
                    byte y = 0;
                    foreach (Worker c in company.Workers)
                    {
                        if (c.Id == Convert.ToInt32(input[0]))
                            y = 1;
                    }
                    if (y == 1)
                        Console.WriteLine("Введенный Id не уникален. Введите другое значение Id");
                    else
                        AddWorker(x, input);
                }
                else
                    AddWorker(x, input);
            }
            else
                Console.WriteLine("Указана некорректная должность сотрудника. Попробуйте еще раз, указав корректное значение");
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("Ошибка ввода данных. Проверьте правильность ввода и повторите попытку");
        }
        catch (System.FormatException)
        {
            Console.WriteLine("Id должен являться целым числом");
        }
    }

    static void EndCase()
    {
        Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
        Console.ReadKey();
        Menu();
    }

    static void Menu()
    {
        Console.WriteLine("\n" +
        "Меню программы:\n" +
        "1. Загрузить список сотрудников из файла\n" +
        "2. Добавить нового сотрудника в список\n" +
        "3. Упорядочить список\n" +
        "4. Показать имена первых 5-ти сотрудников в списке\n" +
        "5. Показать идентификаторы последних 3-х сотрудников в списке\n" +
        "6. Вывести весь список сотрудников\n" +
        "7. Показать текущую среднюю з/п по организации\n" +
        "8. Сохранить список сотрудников в файл\n" +
        "9. Выйти из программы");

        switch (ChoiceActionMenu())
        {
            case 1:
                Console.WriteLine("\nУкажите тип загружаемого файла:\n" +
                "1. XML\n" +
                "2. JSON");
                switch (ChoiceTypeWorkersOrFormat())
                {
                    case 1:
                        var xmlFormatter1 = new XmlSerializer(typeof(List<Worker>));
                        string FileName;
                        Console.WriteLine("\nВведите имя файла:");
                        FileName = Console.ReadLine();
                        string FullFileName = FileName + ".xml";
                        try
                        {
                            using (var file = new FileStream(FullFileName, FileMode.Open))
                            {
                                company.Workers = xmlFormatter1.Deserialize(file) as List<Worker>;
                            }
                            Console.WriteLine("Данные загружены");
                        }
                        catch (InvalidOperationException)
                        {
                            Console.WriteLine("Указанный файл не может быть считан");
                        }
                        catch (FileNotFoundException)
                        {
                            Console.WriteLine("Указанный файл не найден");
                        }
                        EndCase();
                        break;
                    case 2:
                        string FileName1;
                        Console.WriteLine("\nВведите имя файла:");
                        FileName1 = Console.ReadLine();
                        string FullFileName1 = FileName1 + ".json";
                        try
                        {
                            using (FileStream fstream = File.OpenRead(FullFileName1))
                            {
                                byte[] array = new byte[fstream.Length];
                                fstream.Read(array, 0, array.Length);
                                string file = System.Text.Encoding.UTF8.GetString(array);
                                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented };
                                company.Workers = JsonConvert.DeserializeObject<List<Worker>>(file, settings);
                            }
                            Console.WriteLine("Данные загружены");
                        }
                        catch (JsonReaderException)
                        {
                            Console.WriteLine("Указанный файл не может быть считан");
                        }
                        catch (FileNotFoundException)
                        {
                            Console.WriteLine("Указанный файл не найден");
                        }
                        EndCase();
                        break;
                }
                break;
            case 2:
                Console.WriteLine("\nУкажите тип сотрудника:\n" +
                "1. С фиксированной оплатой\n" +
                "2. Почасовик");
                switch (ChoiceTypeWorkersOrFormat())
                {
                    case 1:
                        CheckWorker(1);
                        EndCase();
                        break;
                    case 2:
                        CheckWorker(2);
                        EndCase();
                        break;
                }
                break;
            case 3:
                if (company.Workers.Count != 0)
                {
                    company.Workers.Sort();
                    Console.WriteLine("Список упорядочен");
                }
                else
                    Console.WriteLine("Список пуст");
                EndCase();
                break;
            case 4:
                if (company.Workers.Count < 5)
                {
                    Console.WriteLine("Недостаточно сотрудников");
                    EndCase();
                }
                else
                {
                    Console.WriteLine("\nФИО первых 5 сотрудников в списке:\n");
                    int i = 0;
                    foreach (Worker c in company.Workers)
                    {
                        if (i < 5)
                        {
                            Console.WriteLine($"{i + 1}: {c.Surname} {c.Name} {c.Patronymic}");
                            i++;
                        }
                        else break;
                    }
                    EndCase();
                }
                break;
            case 5:
                if (company.Workers.Count < 3)
                {
                    Console.WriteLine("Недостаточно сотрудников");
                    EndCase();
                }
                else
                {
                    Console.WriteLine("Id последних 3 сотрудников в списке:\n");
                    int i = 0;
                    foreach (Worker c in company.Workers)
                    {
                        if (i > company.Workers.Count - 4)
                            Console.WriteLine($"{i + 1}: {c.Id}");
                        i++;
                    }
                    EndCase();
                }
                break;
            case 6:
                if (company.Workers.Count != 0)
                {
                    foreach (Worker c in company.Workers)
                    {
                        Console.WriteLine($"Id: {c.Id}\n" + $"ФИО: {c.Surname} {c.Name} {c.Patronymic}\n" + $"Дата рождения: {c.Birthday}\n" + $"Зарплата: {c.Salary}\n");
                    }
                }
                else
                    Console.WriteLine("Список пуст");
                EndCase();
                break;
            case 7:
                if (company.Workers.Count != 0)
                {
                    double sum = 0;
                    int n = 0;
                    foreach (Worker c in company.Workers)
                    {
                        sum += c.Salary;
                        n++;
                    }
                    company.AverageSalary = sum / n;
                    Console.WriteLine($"Средняя заработная плата по организации: {company.AverageSalary}\n");
                }
                else
                    Console.WriteLine("Список пуст");
                EndCase();
                break;
            case 8:
                Console.WriteLine("\nУкажите тип сохраняемого файла:\n" +
                "1. XML\n" +
                "2. JSON");
                switch (ChoiceTypeWorkersOrFormat())
                {
                    case 1:
                        string FileName;
                        Console.WriteLine("\nВведите имя сохраняемого файла:");
                        FileName = Console.ReadLine();
                        string FullFileName = FileName + ".xml";
                        try
                        {
                            var xmlFormatter = new XmlSerializer(typeof(List<Worker>));
                            using (var file = new FileStream(FullFileName, FileMode.OpenOrCreate))
                            {
                                xmlFormatter.Serialize(file, company.Workers);
                            }
                            Console.WriteLine("Данные сохранены");
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine("Введенное имя файла содержит недопустимые символы");
                        }
                        EndCase();
                        break;
                    case 2:
                        string FileName1;
                        Console.WriteLine("\nВведите имя сохраняемого файла:");
                        FileName1 = Console.ReadLine();
                        string FullFileName1 = FileName1 + ".json";
                        try
                        {
                            using (StreamWriter file1 = File.CreateText(FullFileName1))
                            {
                                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented };
                                JsonSerializer serializer = JsonSerializer.Create(settings);
                                serializer.Serialize(file1, company.Workers);
                            }
                            Console.WriteLine("Данные сохранены");
                        }
                        catch (ArgumentException)
                        {
                            Console.WriteLine("Введенное имя файла содержит недопустимые символы");
                        }
                        EndCase();
                        break;
                }
                break;
            case 9:
                System.Environment.Exit(0);
                break;
        }
    }

    private static void Main()
    {
        company = new Organization();
        company.Workers = new List<Worker>();
        company.Positions = new List<Position>
            {
                new Position("Программист", 6000, 70000, 420),
                new Position("Менеджер", 4000, 38000, 230),
                new Position("Дизайнер", 4500, 55000, 330),
                new Position("Тестировщик", 5000, 60000, 360),
                new Position("Геймдизайнер", 5500, 65000, 390)
            };
        Menu();
        Console.ReadKey();
    }
}
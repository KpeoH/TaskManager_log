using System.Diagnostics;
using Newtonsoft.Json;
using Serilog;

namespace TaskManager_log
{
    internal class Program
    {
        static readonly string taskManagerFile = "Tasks.json";

        static void Main(string[] args)
        {
            // Настройка Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()  // Логирование в консоль
                .WriteTo.File("logs\\task_manager_log.json", rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10_000_000, 
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)  
                .CreateLogger();

            Log.Information("Программа запущена");

            while (true)
            {
                Console.WriteLine("\tВыберите задачу:" +
                    "\n1. Добавить задачу" +
                    "\n2. Просмотреть все задачи" +
                    "\n3. Удалить задачу" +
                    "\n4. Изменить статус задачи" +
                    "\n5. Выйти");
                string? userChoise = Console.ReadLine();
                switch (userChoise)
                {
                    case "1":
                        Log.Information("Пользователь выбрал режим добавления задачи");
                        AddTask();
                        break;
                    case "2":
                        Log.Information("Пользователь выбрал режим просмотра всех задач");
                        ViewTasks();
                        break;
                    case "3":
                        Log.Information("Пользователь выбрал режим удаления задачи");
                        DeleteTask();
                        break;
                    case "4":
                        Log.Information("Пользователь выбрал режим изменения статуса задачи");
                        MarkTaskAsCompleted();
                        break;
                    case "5":
                        Log.Information("Пользователь решил завершить выполнение программы");


                        var memoryUsage = Process.GetCurrentProcess().PrivateMemorySize64;
                        Log.Information($"Текущее потребление памяти: {memoryUsage / (1024 * 1024)} MB");
                        Console.WriteLine($"Текущее потребление памяти: {memoryUsage / (1024 * 1024)} MB");
                        Console.ReadKey();
                        return;  

                    default:
                        Log.Warning("Введена некорректная команда.");
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }

        static List<TaskItem> LoadTasks()
        {
            if (!File.Exists(taskManagerFile))
            {
                Log.Information("Файл Tasks.json не существует. Создаём новый.");
                return new List<TaskItem>();
            }
            try
            {
                string json = File.ReadAllText(taskManagerFile);
                var tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json);
                Log.Information("Задачи успешно загружены из файла.");
                return tasks ?? new List<TaskItem>();
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка при загрузке задач: {Message}", ex.Message);
                throw;
            }
        }

        static void SaveTasks(List<TaskItem> tasks)
        {
            try
            {
                string json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
                File.WriteAllText(taskManagerFile, json);
                Log.Information("Задачи успешно сохранены в файл.");
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка при сохранении файла: {Message}", ex.Message);
                throw;
            }
        }

        static void AddTask()
        {
            Console.WriteLine("Введите название задачи");
            string? name = Console.ReadLine();
            Log.Information("Пользователь ввёл название задачи: {TaskName}", name);

            var tasks = LoadTasks();
            int newId = tasks.Count > 0 ? tasks.Max(t => t.Id) + 1 : 1;

            tasks.Add(new TaskItem { Id = newId, Name = name!, IsCompleted = false });
            SaveTasks(tasks);

            Log.Information("Задача '{TaskName}' добавлена с ID {TaskId}.", name, newId);
            Console.WriteLine("Задача успешно добавлена.");
        }

        static void ViewTasks()
        {
            var tasks = LoadTasks();
            if (tasks.Count == 0)
            {
                Log.Information("Список задач пуст.");
                Console.WriteLine("Список задач пуст");
            }

            Console.WriteLine("\n=== СПИСОК ЗАДАЧ ===");
            foreach (var task in tasks)
            {
                Console.WriteLine($"Id: {task.Id}, Задача: {task.Name}, Выполнено ли: {task.IsCompleted}");
            }
            Log.Information("Список задач выведен на экран.");
        }

        static void DeleteTask()
        {
            Console.WriteLine("Введите ID задачи для удаления");
            if (!int.TryParse(Console.ReadLine(), out int taskId))
            {
                Log.Warning("Введен некорректный ID задачи.");
                Console.WriteLine("Неверный ID. Попробуйте снова.");
                return;
            }
            var tasks = LoadTasks();
            var taskToRemove = tasks.FirstOrDefault(t => t.Id == taskId);

            if (taskToRemove == null)
            {
                Log.Information("Задача с ID {TaskId} не найдена.", taskId);
                Console.WriteLine("Задача не найдена.");
                return;
            }

            tasks.Remove(taskToRemove);
            SaveTasks(tasks);

            Log.Information("Задача с ID {TaskId} успешно удалена.", taskId);
            Console.WriteLine("Задача успешно удалена.");
        }

        static void MarkTaskAsCompleted()
        {
            Console.WriteLine("Введите ID задачи для изменения её статуса выполнения.");
            if (!int.TryParse(Console.ReadLine(), out int taskId))
            {
                Log.Warning("Введен некорректный ID задачи.");
                Console.WriteLine("Неверный ID. Попробуйте снова.");
                return;
            }
            var tasks = LoadTasks();
            var taskToComplete = tasks.FirstOrDefault(t => t.Id == taskId);

            taskToComplete!.IsCompleted = true;
            SaveTasks(tasks);

            Log.Information("Статус задачи с ID {TaskId} успешно изменён.", taskId);
            Console.WriteLine("Задача помечена как выполненная.");
        }

        public class TaskItem
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public bool IsCompleted { get; set; }
        }
    }
}

using System.Diagnostics;
using Newtonsoft.Json;

namespace TaskManager_log
{
    internal class Program
    {
        static readonly string taskManagerFile = "Tasks.json";
        static void Main(string[] args)
        {
            string fileLogPath = "program_log.txt";
            Trace.Listeners.Add(new TextWriterTraceListener(fileLogPath));
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;
            Trace.WriteLine($"\n[{DateTime.Now}] Программа запущена");

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
                        Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Пользователь выбрал режим добавления задачи");
                        AddTask();
                        break;
                    case "2":
                        Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Пользователь выбрал режим просмотра всех задач");
                        ViewTasks();
                        break;
                    case "3":
                        Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Пользователь выбрал режим удаления задачи");
                        DeleteTask();
                        break;
                    case "4":
                        Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Пользователь выбрал режим изменения статуса задачи");
                        MarkTaskAsCompleted();
                        break;
                    case "5":
                        Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Пользователь решил завершить выполнение программы");
                        return;
                    default:
                        Trace.TraceWarning($"[{DateTime.Now:HH:MM:ss}] Введена некорректная команда.");
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }


        static List<TaskItem> LoadTasks()
        {
            if (!File.Exists(taskManagerFile))
            {
                Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] файл Tasks.json не существует. Создаём новый.");
                return new List<TaskItem>();
            }
            try
            {
                string json = File.ReadAllText(taskManagerFile);
                var tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json);
                Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Задачи успешно загружены из файла.");
                return tasks ?? new List<TaskItem>();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"[{DateTime.Now:HH:MM:ss}] Ошибка при загрузки задач: {ex.Message}");
                throw;
            }
        }

        static void SaveTasks(List<TaskItem> tasks)
        {
            try
            {
                string json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
                File.WriteAllText(taskManagerFile, json);
                Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Задачи успешно сохранены в файл.");
            }
            catch (Exception ex)
            {
                {
                    Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Ошибка при сохранении файла: {ex.Message}");
                    throw;
                }
            }
        }

        static void AddTask()
        {
            Console.WriteLine("Введите название задачи");
            string? name = Console.ReadLine();
            Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Пользователь ввёл название задачи: {name}");

            var tasks = LoadTasks();
            int newId = tasks.Count > 0 ? tasks.Max(t => t.Id) + 1 : 1;

            tasks.Add(new TaskItem { Id = newId, Name = name!, IsCompleted = false });
            SaveTasks(tasks);

            Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Задача '{name}' добавлена с ID {newId}.");
            Console.WriteLine("Задача успешно добавлена.");
        }

        static void ViewTasks()
        {
            var tasks = LoadTasks();
            if (tasks.Count == 0 )
            {
                Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Список задач пуст.");
                Console.WriteLine("Список задач пуст");
            }

            Console.WriteLine("\n=== СПИСОК ЗАДАЧ ===");
            foreach ( var task in tasks )
            {
                Console.WriteLine($"Id: {task.Id}, Задача: {task.Name}, Выполнено ли: {task.IsCompleted}");
            }
            Trace.TraceInformation($"\n[{DateTime.Now:HH:MM:ss}] Список задач выведен на экран.");
        }

        static void DeleteTask()
        {
            Console.WriteLine("Введите ID задачи для удаления");
            if (!int.TryParse(Console.ReadLine(), out int taskId))
            {
                Trace.TraceWarning($"[{DateTime.Now:HH:MM:ss}] Введен некорректный ID задачи.");
                Console.WriteLine("Неверный ID. Попробуйте снова.");
                return;
            }
            var tasks = LoadTasks();
            var taskToRemove = tasks.FirstOrDefault(t => t.Id == taskId);

            if (taskToRemove == null)
            {
                Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Задача с {taskId} не найдена.");
                Console.WriteLine("Задача не найдена.");
                return;
            }

            tasks.Remove(taskToRemove);
            SaveTasks(tasks);

            Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Задача с ID {taskId} успешно удалена.");
            Console.WriteLine("Задача успешно удалена.");
        }

        static void MarkTaskAsCompleted()
        {
            Console.WriteLine("Введите ID задачи для изменения её статуса выполнения.");
            if (!int.TryParse(Console.ReadLine(), out int taskId))
            {
                Trace.TraceWarning($"[{DateTime.Now:HH:MM:ss}] Введен некорректный ID задачи.");
                Console.WriteLine("Неверный ID. Попробуйте снова.");
                return;
            }
            var tasks = LoadTasks();
            var taskToComplete = tasks.FirstOrDefault(t => t.Id == taskId);

            taskToComplete!.IsCompleted = true;
            SaveTasks(tasks);

            Trace.TraceInformation($"[{DateTime.Now:HH:MM:ss}] Статус задачи с ID {taskId} успешно изменён.");
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


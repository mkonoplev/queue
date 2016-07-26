using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Queue
{
    /*
     * 1.Надо сделать очередь с операциями push(t) и T pop(). Операции должны поддерживать обращение с разных потоков.
     * Операция push всегда вставляет и выходит. Операция pop ждет пока не появится новый элемент.
     * В качестве контейнера внутри можно использовать только стандартную очередь (Queue).
     */
    class Program
    {
        // Для генерации случайных занчений, помещаемых в очередь.
        static readonly Random Random = new Random((int)DateTime.Now.Ticks);

        static void Main(string[] args)
        {
            using (var cts = new CancellationTokenSource())
            {
                var queue = new Queue<int>();

                // Запись в очередь.
                Action<object> pusher = delegate(object obj)
                {
                    var managedThreadId = Thread.CurrentThread.ManagedThreadId;

                    Utilities.WriteLine("{0}: стартовал", managedThreadId);
                    if (obj is CancellationToken)
                    {
                        var ct = (CancellationToken)obj;

                        while (true)
                        {
                            if (ct.IsCancellationRequested)
                            {
                                Utilities.WriteLine("{0}: завершается...", managedThreadId);
                                return;
                            }
                            var tmp = Random.Next(100);
                            queue.Push(tmp);
                        }
                    }
                };
                // Чтение из очереди.
                Action<object> popper = delegate(object obj)
                {
                    var managedThreadId = Thread.CurrentThread.ManagedThreadId;

                    Utilities.WriteLine("{0}: стартовал", managedThreadId);
                    if (obj is CancellationToken)
                    {
                        var ct = (CancellationToken)obj;

                        while (true)
                        {
                            if (ct.IsCancellationRequested)
                            {
                                Utilities.WriteLine("{0}: завершается...", managedThreadId);
                                return;
                            }
                            queue.Pop();
                        }
                    }
                };
                var tasks = new List<Task>();
                var taskFactory = new TaskFactory();

                // Количесто потоков выбранно случайным образом.
                for (var i = 0; i < 10; i++)
                {
                    var action = 0 == i % 3 ? pusher : popper;
                    tasks.Add(taskFactory.StartNew(action, cts.Token));
                }
                Utilities.WaitKey();
                cts.Cancel();
                Task.WaitAll(tasks.ToArray());
                Utilities.WaitKey(ConsoleKey.Enter);
            }
        }
    }
}

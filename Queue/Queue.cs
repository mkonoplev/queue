using System;
using System.Threading;

namespace Queue
{
    class Queue<T>
    {
        #region Static and constants.
#if DEBUG
        private const int DelayMin = 200;
        private const int DeleayMax = 1100;
#endif
        #endregion

        #region Member variables.

        private readonly System.Collections.Queue _queue = new System.Collections.Queue();
        private int _numberOfPushers;
        private int _numberOfPoppers;
        private readonly AutoResetEvent _are1 = new AutoResetEvent(false);
        private readonly AutoResetEvent _are2 = new AutoResetEvent(false);

#if DEBUG
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);
#endif
        #endregion

        #region Properties.

        #endregion

        #region Methods and implementations.

        public void Push(T t)
        {
            try
            {
                if (1 == Interlocked.Increment(ref _numberOfPushers))
                {
                    // Блокировки нет. Помещаем элемент в очередь, возвращаем управление.
                    Enqueue(t);
                }
                else
                {
                    // Блокируем поток...
                    _are1.WaitOne();
                    // ...помещаем элемент в очередь, возвращаем управление.
                    Enqueue(t);
                }
            }
            finally
            {
                if (0 != Interlocked.Decrement(ref _numberOfPushers))
                {
                    // Другие потоки (push) заблокирвованны; пробуждаем один из них.
                    _are1.Set();
                }
            }
        }

        public T Pop()
        {
            var sl = new SpinWait();
            T t;
            try
            {
                if (1 == Interlocked.Increment(ref _numberOfPoppers))
                {
                    // Блокировки нет. Ждем появление в очереди элемента...
                    while (0 == _queue.Count)
                    {
                        sl.SpinOnce();
                    }
                    // ...читаем, возвращаем управление.
                    t = Dequeue();
                }
                else
                {
                    // Блокируем поток...
                    _are2.WaitOne();
                    // ...ждем появление в очереди элемента...
                    while (0 == _queue.Count)
                    {
                        sl.SpinOnce();
                    }
                    // ...читаем, возвращаем управление.
                    t = Dequeue();
                }
            }
            finally
            {
                if (0 != Interlocked.Decrement(ref _numberOfPoppers))
                {
                    // Другие потоки (pop) заблокирвованны; пробуждаем один из них.
                    _are2.Set();
                }
            }
            return t;
        }

        private void Enqueue(T t)
        {
            try
            {
#if DEBUG
                Utilities.Write("пишем: '{0}'...", t);
#endif
                _queue.Enqueue(t);
#if DEBUG
                Utilities.WriteLine("ок");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Utilities.WriteLine("ошибка ({0})", ex.Message);
#endif
                throw;
            }
#if DEBUG
            Thread.Sleep(_random.Next(DelayMin, DeleayMax));
#endif
        }

        private T Dequeue()
        {
            T t;
            try
            {
#if DEBUG
                Utilities.Write("читаем...");
#endif
                t = (T)_queue.Dequeue();
#if DEBUG
                Utilities.WriteLine("'{0}'", t);
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Utilities.WriteLine("ошибка: {0}", ex.Message);
#endif
                throw;
            }
#if DEBUG
            Thread.Sleep(_random.Next(DelayMin, DeleayMax));
#endif
            return t;
        }
        #endregion
    }
}

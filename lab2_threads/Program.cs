using System;
using System.Threading;

namespace lab2_threads
{
    class Program
    {
        private static readonly int numOfElements = 10;
        private static readonly int numOfThreads = 2;

        private readonly Thread[] thread = new Thread[numOfThreads];
        private readonly int[] array = new int[numOfElements];

        private int threadCount = 0;
        private Result result = new Result(0, -1);

        private readonly object lockerForCollect = new object();
        private readonly object lockerForCount = new object();
        static void Main(string[] args)
        {
            Program program = new Program();
            program.InitArr();
            Console.WriteLine(program.FindMinNum(0, numOfElements));

            Console.WriteLine(program.ParallelSearch());
        }
        private void InitArr()
        {
            Random rnd = new Random();
            for (int i = 0; i < numOfElements; i++)
            {
                array[i] = 1;
            }
            array[rnd.Next(0, numOfElements)] = -1;
        }

        public Result FindMinNum(int startIndex, int finishIndex)
        {
            int minNum = array[startIndex];
            int minNumIndex = startIndex;

            for (int i = startIndex; i < finishIndex; i++)
            {
                if (array[i] < minNum)
                {
                    minNum = array[i];
                    minNumIndex = i;
                }
            }

            return new Result(minNum, minNumIndex);
        }

        private Result ParallelSearch()
        {
            int step = numOfElements / numOfThreads;
            for (int i = 0; i < numOfThreads; i++)
            {
                thread[i] = new Thread(StarterThread);
                thread[i].Start(new Bound(i * step, i * step + step));
            }

            lock (lockerForCount)
            {
                while (threadCount < numOfThreads)
                {
                    Monitor.Wait(lockerForCount);
                }
            }
            return result;
        }
        private void StarterThread(object param)
        {
            if (param is Bound)
            {
                Result localResult = FindMinNum((param as Bound).StartIndex, (param as Bound).FinishIndex);

                lock (lockerForCollect)
                {
                    CollectData(localResult);
                }
                IncThreadCount();
            }
        }
        private void IncThreadCount()
        {
            lock (lockerForCount)
            {
                threadCount++;
                Monitor.Pulse(lockerForCount);
            }
        }

        public void CollectData(Result newResult)
        {
            if (result.MinNum > newResult.MinNum)
            {
                result = newResult;
            }
        }

        class Bound
        {
            public Bound(int startIndex, int finishIndex)
            {
                StartIndex = startIndex;
                FinishIndex = finishIndex;
            }

            public int StartIndex { get; set; }
            public int FinishIndex { get; set; }
        }

        public class Result
        {
            public Result(int minNum, int minNumIndex)
            {
                MinNum = minNum;
                MinNumIndex = minNumIndex;
            }
            public int MinNum { get; set; }
            public int MinNumIndex { get; set; }

            public override string ToString()
            {
                return $"Мiнiмальним елементом масиву є {MinNum} з iндексом {MinNumIndex} ";
            }
        }

    }
}

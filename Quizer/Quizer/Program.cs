using System;
using System.IO;

namespace Quizer
{
    class MainClass
    {
        public static void ConsoleClear()
        {
            for (int i = 0; i < Console.WindowHeight; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }
        }

        public static void Main()
        {
            var quizer = new Quizer();

            quizer.LoadQuestions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"));
            quizer.GenerateQueue();

            ConsoleClear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Progress: "
                + (100 - ((double)quizer.QuizQueue.Count / quizer.QuizList.Count - 1))
                + "%");

            foreach (var quizIndex in quizer.QuizQueue)
            {
                //TODO: what is this why is this
                quizer.PrintQuestion(quizIndex);
                var prevProgress = quizer.QuizList[quizIndex].Progress;
                var answer =  quizer.GetInput(quizIndex);
                var correct = quizer.CheckUpdate(quizIndex, answer);
                var currentProgress = quizer.QuizList[quizIndex].Progress;
                var dif = currentProgress - prevProgress;

                Console.SetCursorPosition(0, 0);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, 0);
                if (correct)
                    Console.Write("Correct!");
                else
                    Console.Write(quizer.GetAnswer(quizIndex));

                Console.Write(" " + currentProgress + " (" + (dif > 0 ? "+" : "") + dif + ")");
                Console.ReadLine();
                ConsoleClear();
            }
        }
    }
}

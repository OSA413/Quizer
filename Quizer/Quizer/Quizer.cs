using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Quizer
{
    public class Item
    {
        public Quiz Quiz;
        public string Path;
        public int Progress;

        public Item(Quiz Quiz, string Path, int Progress)
        {
            this.Quiz = Quiz;
            this.Path = Path;
            this.Progress = Progress;
        }
    }

    public class Quizer
    {
        private Random rng = new Random();
        public List<int> QuizQueue = new List<int>();
        public List<Item> QuizList;

        public void LoadQuestions(string path,
            SearchOption searchOption = SearchOption.AllDirectories)
        {
            QuizList = new List<Item>();

            var fileList = Directory.GetFiles(path, "*.quiz", searchOption);

            foreach (var f in fileList)
            {
                var fullPath = Path.Combine(path, f);
                var newQuiz = new Quiz();
                int progress = 0;

                foreach (var line in File.ReadLines(fullPath))
                {
                    var indexOf = line.IndexOf("=");
                    if (indexOf == -1)
                        continue;

                    var key = line.Substring(0, indexOf);
                    var value = line.Substring(indexOf + 1);

                    if (key == "Type")
                        newQuiz.Type = (QuizType)Enum.Parse(typeof(QuizType), value);
                    else if (key == "Question")
                        newQuiz.Question = value;
                    else if (key == "Answer")
                        newQuiz.Answer = value;
                    else if (key.StartsWith("Option"))
                    {
                        if (newQuiz.Options == null)
                            newQuiz.Options = new Dictionary<int, string>();
                        var index = Convert.ToInt32(key.Substring("Option".Length));
                        newQuiz.Options[index] = value;
                    }
                }

                var progressFile = Path.Combine( 
                Path.GetDirectoryName(fullPath),
                "progress",
                Path.GetFileNameWithoutExtension(fullPath) + ".progress");

                if (File.Exists(progressFile))
                    progress = Convert.ToInt32(File.ReadAllText(progressFile));

                QuizList.Add(new Item(newQuiz, fullPath, progress));
            }
        }

        public void GenerateQueue()
        {
            QuizQueue.Clear();
            for (int i = 0; i < QuizList.Count; i++)
                for (int j = 0; j < 101 - QuizList[i].Progress; j++)
                    QuizQueue.Add(i);

            //TODO think up a better way of getting random queue of integers with rarity
            for (int i = 0; i < QuizQueue.Count; i++)
            {
                var ti = rng.Next(QuizQueue.Count);
                var t = QuizQueue[ti];
                QuizQueue[ti] = QuizQueue[i];
                QuizQueue[i] = t;
            }
        }

        public string GetAnswer(int quizIndex) =>
            QuizList[quizIndex].Quiz.Answer;

        public int GetNewProgress(int currentProgress, bool answerIsCorrect)
        {
            return Math.Max(0,
                Math.Min(100, 
                currentProgress + (3 * (answerIsCorrect ? 1 : 0) - 2)
                * (currentProgress / 10 + 1)));
        }

        public void UpdateProgress(int quizIndex, int newProgress)
        {
            var q = QuizList[quizIndex];
            QuizList[quizIndex].Progress = newProgress;
            var fullPath = q.Path;

            var progressFile = Path.Combine(
                Path.GetDirectoryName(fullPath),
                "progress",
                Path.GetFileNameWithoutExtension(fullPath) + ".progress");

            if (!Directory.Exists(Path.GetDirectoryName(progressFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(progressFile));

            File.WriteAllText(progressFile, q.Progress.ToString());
        }

        public bool CheckUpdate(int quizIndex, string answer)
        {
            var q = QuizList[quizIndex];
            var correct = q.Quiz.Answer.ToLower() == answer.ToLower();
            var newProgress = GetNewProgress(q.Progress, correct);
            UpdateProgress(quizIndex, newProgress);
            return correct;
        }

        public void PrintQuestion(int quizIndex)
        {
            var q = QuizList[quizIndex].Quiz;

            Console.SetCursorPosition(0, 1);
            Console.WriteLine(q.Question);
            if (q.Options != null)
                foreach (var k in q.Options.Keys.OrderBy(x => x))
                {
                    Console.Write("[" + k + "] ");
                    Console.WriteLine(q.Options[k]);
                }
            else if (q.Type == QuizType.TrueFalse)
            {
                Console.WriteLine("[1] True");
                Console.WriteLine("[2] False");
            }
        }

        public string GetInput(int quizIndex)
        {
            var q = QuizList[quizIndex].Quiz;
            Console.CursorTop = Console.WindowHeight - 3;

            string text;
            switch (q.Type)
            {
                case QuizType.TrueFalse: text = "Выберите ответ"; break;
                case QuizType.CheckBox: text = "Выберите один или несколько ответов"; break;
                case QuizType.RadioButton: text = "Выберите ответ"; break;
                default: text = "Введите ответ"; break;
            }

            Console.WriteLine(text);
            Console.Write(">>> ");
            var input = Console.ReadLine();
            if (q.Type == QuizType.TrueFalse)
            {
                if (input == "1")
                    input = "True";
                else if (input == "2")
                    input = "False";
            }
            return input;
        }
    }
}

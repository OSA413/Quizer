using System.Collections.Generic;

namespace Quizer
{
    public class Quiz
    {
        public string Question;
        public string Answer;
        public QuizType Type;
        public Dictionary<int, string> Options;
    }
}

public enum QuizType
{
    Question,
    CheckBox,
    RadioButton,
    TrueFalse,
}
using System;
using System.Collections.Generic;

namespace MissingDlls
{
    public class TextLine
    {
        public TextLine(int tabs)
        {
            this.tabs = tabs;
        }
        
        private readonly int tabs;
        private readonly List<Text> texts = new List<Text>();
        public bool IsError { get; set; }
        public bool IsGac { get; set; }
        
        public void Print()
        {
            var oldColor = Console.ForegroundColor;
            for (int i = 0; i < tabs; i++)
            {
                Console.Write("  ");
            }
            foreach (var text in texts)
            {
                Console.ForegroundColor = text.Color;
                Console.Write(text.Data);
            }
            Console.WriteLine();
            Console.ForegroundColor = oldColor;
        }

        public void AddText(string text, ConsoleColor color)
        {
            texts.Add(new Text { Color = color, Data = text});
        }
    }
}
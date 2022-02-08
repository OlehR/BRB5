using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5
{
    public class Question
    {
        public int Id1 { get { return Id * 10 + 1; } }
        public int Id2 { get { return Id * 10 + 2; } }
        public int Id3 { get { return Id * 10 + 3; } }
        public int Id { get; set; }
        public bool IsHead { get; set; }
        public string Text { get; set; }
        public int Choice { get; set; }
        public string Note { get; set; }
    }
}

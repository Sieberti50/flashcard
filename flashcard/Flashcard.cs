using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Flashcard
{
    public string Polish { get; set; }
    public string English { get; set; }

    public Flashcard(string polish, string english)
    {
        Polish = polish;
        English = english;
    }
}

using Flashcards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AppData
{
    public List<FlashcardSet> Sets { get; set; } = new()
        {
            new FlashcardSet { Name = "Podstawowe" },
            new FlashcardSet { Name = "Podróże" },
            new FlashcardSet { Name = "Biznes" }
        };
    public string CurrentSetName { get; set; } = "Podstawowe";
    public UserStats Stats { get; set; } = new();
}
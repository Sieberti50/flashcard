namespace Flashcards
{
    public class Flashcard
    {
        public string Polish { get; set; } = "";
        public string English { get; set; } = "";
        public string Category { get; set; } = "Podstawowe";
        public string Difficulty { get; set; } = "Średnie";
        public int TimesCorrect { get; set; } = 0;
        public int TimesWrong { get; set; } = 0;

        public Flashcard() { }
        public Flashcard(string pl, string en, string cat = "Podstawowe")
        {
            Polish = pl; English = en; Category = cat;
        }
    }
}
public class UserStats
{
    public int TotalTests { get; set; } = 0;
    public int TotalCorrect { get; set; } = 0;
    public int TotalQuestions { get; set; } = 0;
    public DateTime LastTestDate { get; set; } = DateTime.MinValue;
    public List<string> MostCommonMistakes { get; set; } = new();
}
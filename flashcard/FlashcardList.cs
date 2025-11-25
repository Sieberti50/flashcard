// FlashcardList.cs
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

public class FlashcardList
{
    public List<Flashcard> Cards { get; private set; } = new List<Flashcard>();

    public void LoadFromFile()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Pliki tekstowe (*.txt)|*.txt|Wszystkie pliki (*.*)|*.*",
            Title = "Wybierz plik ze słówkami"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            Cards.Clear();
            string[] lines = File.ReadAllLines(openFileDialog.FileName);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains(";")) continue;

                var parts = line.Split(new[] { ';' }, 2);
                if (parts.Length == 2)
                {
                    Cards.Add(new Flashcard(parts[0].Trim(), parts[1].Trim()));
                }
            }
        }
    }
}
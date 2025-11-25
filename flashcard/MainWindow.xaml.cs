using Microsoft.Win32;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Flashcards
{
    public partial class MainWindow : Window
    {
        private AppData appData;
        private FlashcardSet CurrentSet => appData.Sets.Find(s => s.Name == appData.CurrentSetName)!;
        private List<Flashcard> Cards => CurrentSet.Cards;

        private int currentIndex = 0;
        private readonly Random rnd = new();
        private bool polishToEnglish = true;
        private bool testMode = false;
        private List<Flashcard> testQueue = new();

        public MainWindow()
        {
            InitializeComponent();
            appData = DataService.Load();
            RefreshCategories();
            UpdateStats();
            ShowCurrentCard();
        }

        private void RefreshCategories()
        {
            CategoryComboBox.ItemsSource = null;
            CategoryComboBox.ItemsSource = appData.Sets.Select(s => s.Name);
            CategoryComboBox.SelectedItem = appData.CurrentSetName;
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is string name)
            {
                appData.CurrentSetName = name;
                DataService.Save(appData);
                currentIndex = 0;
                ShowCurrentCard();
                UpdateInfo();
                UpdateStats();
            }
        }

        private void NewSet_Click(object sender, RoutedEventArgs e)
        {
            string name = Interaction.InputBox("Podaj nazwę nowego zestawu:", "Nowy zestaw");
            if (string.IsNullOrWhiteSpace(name)) return;
            if (appData.Sets.Any(s => s.Name == name))
            {
                MessageBox.Show("Zestaw o tej nazwie już istnieje!");
                return;
            }

            appData.Sets.Add(new FlashcardSet { Name = name });
            RefreshCategories();
            DataService.Save(appData);
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Pliki tekstowe|*.txt" };
            if (dlg.ShowDialog() == true)
            {
                var lines = File.ReadAllLines(dlg.FileName);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(';', 3);
                    var card = new Flashcard(parts[0].Trim(), parts[1].Trim(), appData.CurrentSetName);
                    if (parts.Length > 2) card.Difficulty = parts.Length > 2 ? parts[2].Trim() : "Średnie";
                    Cards.Add(card);
                }
                DataService.Save(appData);
                UpdateStats();
                ShowCurrentCard();
            }
        }

        private void EditCards_Click(object sender, RoutedEventArgs e)
        {
            var win = new EditCardsWindow(Cards, () =>
            {
                DataService.Save(appData);
                UpdateStats();
                ShowCurrentCard();
            });
            win.ShowDialog();
        }

        private void LearningMode_Click(object sender, RoutedEventArgs e)
        {
            if (Cards.Count == 0) { MessageBox.Show("Brak słówek!"); return; }
            testMode = false;
            TestButtonsPanel.Visibility = Visibility.Collapsed;
            ShowAnswerButton.Visibility = Visibility.Visible;
            currentIndex = 0;
            ShowCurrentCard();
            UpdateInfo();
        }

        private void TestMode_Click(object sender, RoutedEventArgs e)
        {
            if (Cards.Count == 0) { MessageBox.Show("Brak słówek!"); return; }

            testMode = true;
            appData.Stats.TotalTests++;
            appData.Stats.LastTestDate = DateTime.Now;
            BuildTestQueue();
            currentIndex = 0;
            polishToEnglish = rnd.Next(2) == 0;
            TestButtonsPanel.Visibility = Visibility.Visible;
            ShowAnswerButton.Visibility = Visibility.Collapsed;
            ShowCurrentCard();
            UpdateInfo();
            DataService.Save(appData);
        }

        private void BuildTestQueue()
        {
            testQueue.Clear();
            foreach (var card in Cards)
            {
                int weight = card.Difficulty switch
                {
                    "Trudne" => 8,
                    "Średnie" => 3,
                    "Łatwe" => 1,
                    _ => 3
                };
                for (int i = 0; i < weight; i++) testQueue.Add(card);
            }
            for (int i = testQueue.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (testQueue[i], testQueue[j]) = (testQueue[j], testQueue[i]);
            }
        }

        private void ShowCurrentCard()
        {
            if (Cards.Count == 0)
            {
                QuestionTextBlock.Text = "Brak słówek w zestawie";
                AnswerTextBlock.Visibility = Visibility.Collapsed;
                return;
            }

            var card = testMode ? testQueue[currentIndex] : Cards[currentIndex];
            AnswerTextBlock.Visibility = Visibility.Collapsed;
            ShowAnswerButton.Visibility = testMode ? Visibility.Collapsed : Visibility.Visible;

            polishToEnglish = rnd.Next(2) == 0;
            QuestionTextBlock.Text = polishToEnglish ? card.Polish : card.English;
        }

        private void ShowAnswer_Click(object sender, RoutedEventArgs e)
        {
            var card = testMode ? testQueue[currentIndex] : Cards[currentIndex];
            AnswerTextBlock.Text = polishToEnglish ? card.English : card.Polish;
            AnswerTextBlock.Visibility = Visibility.Visible;
            ShowAnswerButton.Visibility = Visibility.Collapsed;
        }

        private void NextCard_Click(object sender, RoutedEventArgs e) => NextCard();
        private void PrevCard_Click(object sender, RoutedEventArgs e)
        {
            currentIndex = (currentIndex - 1 + (testMode ? testQueue.Count : Cards.Count)) % (testMode ? testQueue.Count : Cards.Count);
            ShowCurrentCard();
            UpdateInfo();
        }

        private void NextCard()
        {
            if (testMode)
            {
                currentIndex++;
                if (currentIndex >= testQueue.Count)
                {
                    EndTest();
                    return;
                }
            }
            else
            {
                currentIndex = (currentIndex + 1) % Cards.Count;
            }
            ShowCurrentCard();
            UpdateInfo();
        }

        private void CorrectAnswer_Click(object sender, RoutedEventArgs e)
        {
            var card = testQueue[currentIndex];
            card.TimesCorrect++;
            appData.Stats.TotalCorrect++;
            appData.Stats.TotalQuestions++;
            UpdateDifficulty(card);
            NextAfterAnswer();
        }

        private void WrongAnswer_Click(object sender, RoutedEventArgs e)
        {
            var card = testQueue[currentIndex];
            card.TimesWrong++;
            appData.Stats.TotalQuestions++;

            string mistake = $"{card.Polish} → {card.English}";
            if (!appData.Stats.MostCommonMistakes.Contains(mistake))
                appData.Stats.MostCommonMistakes.Add(mistake);

            UpdateDifficulty(card);
            NextAfterAnswer();
        }

        private void UpdateDifficulty(Flashcard card)
        {
            if (card.TimesWrong >= 4) card.Difficulty = "Trudne";
            else if (card.TimesWrong >= 2) card.Difficulty = "Średnie";
            else if (card.TimesCorrect >= 5) card.Difficulty = "Łatwe";
            DataService.Save(appData);
        }

        private void NextAfterAnswer()
        {
            var card = testQueue[currentIndex];
            AnswerTextBlock.Visibility = Visibility.Visible;
            AnswerTextBlock.Text = polishToEnglish ? card.English : card.Polish;

            Task.Delay(1200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    currentIndex++;
                    if (currentIndex >= testQueue.Count) EndTest();
                    else
                    {
                        polishToEnglish = rnd.Next(2) == 0;
                        ShowCurrentCard();
                        UpdateInfo();
                    }
                });
            });
        }

        private void EndTest()
        {
            double percent = appData.Stats.TotalQuestions > 0
                ? Math.Round(100.0 * appData.Stats.TotalCorrect / appData.Stats.TotalQuestions, 1)
                : 0;

            MessageBox.Show(
                $"Test zakończony!\n\n" +
                $"Poprawne: {appData.Stats.TotalCorrect} / {appData.Stats.TotalQuestions} ({percent}%)\n" +
                $"Wszystkich testów: {appData.Stats.TotalTests}\n" +
                $"Najczęstsze błędy: {appData.Stats.MostCommonMistakes.Count}\n" +
                $"Ostatni test: {appData.Stats.LastTestDate:yyyy-MM-dd HH:mm}",
                "Wynik testu", MessageBoxButton.OK, MessageBoxImage.Information);

            testMode = false;
            TestButtonsPanel.Visibility = Visibility.Collapsed;
            UpdateStats();
            DataService.Save(appData);
        }

        private void UpdateInfo()
        {
            if (Cards.Count == 0)
            {
                InfoTextBlock.Text = "Brak słówek";
                return;
            }

            int total = testMode ? testQueue.Count : Cards.Count;
            InfoTextBlock.Text = $"{currentIndex + 1} / {total} • {CurrentSet.Name}";
        }

        private void UpdateStats()
        {
            double overall = appData.Stats.TotalQuestions > 0
                ? Math.Round(100.0 * appData.Stats.TotalCorrect / appData.Stats.TotalQuestions, 1)
                : 0;

            StatsTextBlock.Text = $"Testy: {appData.Stats.TotalTests} | " +
                                  $"Poprawność: {overall}% | " +
                                  $"Słówek: {Cards.Count} | " +
                                  $"Trudne: {Cards.Count(c => c.Difficulty == "Trudne")} | " +
                                  $"Błędy: {appData.Stats.MostCommonMistakes.Count}";
            UpdateInfo();
        }
    }
}
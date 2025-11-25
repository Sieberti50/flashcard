using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Flashcards
{
    public partial class MainWindow : Window
    {
        private FlashcardList flashcardList = new FlashcardList();
        private List<Flashcard> currentList = new List<Flashcard>();
        private int currentIndex = 0;
        private Random rnd = new Random();

        private bool polishToEnglish = true;

        private int correctAnswers = 0;
        private int totalQuestions = 0;
        private bool testMode = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            flashcardList.LoadFromFile();
            if (flashcardList.Cards.Count > 0)
            {
                currentList = new List<Flashcard>(flashcardList.Cards);
                currentIndex = 0;
                UpdateInfoText();
                ShowCurrentCard();
            }
            else
            {
                InfoTextBlock.Text = "Nie wczytano żadnych słówek.";
            }
        }

        private void LearningMode_Click(object sender, RoutedEventArgs e)
        {
            if (currentList.Count == 0) { MessageBox.Show("Najpierw wczytaj słówka!"); return; }

            testMode = false;
            TestButtonsPanel.Visibility = Visibility.Collapsed;
            ShowAnswerButton.Visibility = Visibility.Visible;
            NextButton.Content = "Następne →";
            polishToEnglish = true;
            ShuffleCards();
            currentIndex = 0;
            correctAnswers = totalQuestions = 0;
            ShowCurrentCard();
            UpdateInfoText();
        }

        private void TestMode_Click(object sender, RoutedEventArgs e)
        {
            if (currentList.Count == 0) { MessageBox.Show("Najpierw wczytaj słówka!"); return; }

            testMode = true;
            TestButtonsPanel.Visibility = Visibility.Visible;
            ShowAnswerButton.Visibility = Visibility.Collapsed;
            NextButton.Content = "Dalej";
            correctAnswers = 0;
            totalQuestions = 0;
            ShuffleCards();
            currentIndex = 0;
            ShowCurrentCard();
            UpdateInfoText();
        }

        private void ShowCurrentCard()
        {
            if (currentList.Count == 0) return;

            var card = currentList[currentIndex];
            AnswerTextBlock.Visibility = Visibility.Collapsed;
            ShowAnswerButton.Visibility = testMode ? Visibility.Collapsed : Visibility.Visible;

            if (polishToEnglish)
                QuestionTextBlock.Text = card.Polish;
            else
                QuestionTextBlock.Text = card.English;

            if (!testMode)
                polishToEnglish = rnd.Next(2) == 0;
        }

        private void ShowAnswer_Click(object sender, RoutedEventArgs e)
        {
            var card = currentList[currentIndex];
            AnswerTextBlock.Visibility = Visibility.Visible;
            ShowAnswerButton.Visibility = Visibility.Collapsed;

            AnswerTextBlock.Text = polishToEnglish ? card.English : card.Polish;
        }

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            if (currentList.Count == 0) return;

            currentIndex++;
            if (currentIndex >= currentList.Count)
            {
                if (testMode)
                {
                    MessageBox.Show($"Koniec testu!\nPoprawne odpowiedzi: {correctAnswers}/{totalQuestions} ({Percentage()}%)",
                        "Wynik testu", MessageBoxButton.OK, MessageBoxImage.Information);
                    testMode = false;
                    TestButtonsPanel.Visibility = Visibility.Collapsed;
                    UpdateInfoText();
                    return;
                }
                currentIndex = 0;
            }

            ShowCurrentCard();
            UpdateInfoText();
        }

        private void PrevCard_Click(object sender, RoutedEventArgs e)
        {
            if (currentList.Count == 0) return;

            currentIndex--;
            if (currentIndex < 0)
                currentIndex = currentList.Count - 1;

            ShowCurrentCard();
            UpdateInfoText();
        }

        private void CorrectAnswer_Click(object sender, RoutedEventArgs e)
        {
            correctAnswers++;
            totalQuestions++;
            GoToNextInTest();
        }

        private void WrongAnswer_Click(object sender, RoutedEventArgs e)
        {
            totalQuestions++;
            GoToNextInTest();
        }

        private void GoToNextInTest()
        {
            UpdateInfoText();
            ShowAnswerButton.Visibility = Visibility.Collapsed;

            var card = currentList[currentIndex];
            AnswerTextBlock.Visibility = Visibility.Visible;
            AnswerTextBlock.Text = polishToEnglish ? card.English : card.Polish;

            System.Threading.Tasks.Task.Delay(1200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    currentIndex++;
                    if (currentIndex >= currentList.Count)
                    {
                        MessageBox.Show($"Koniec testu!\nWynik: {correctAnswers}/{totalQuestions} ({Percentage()}%)",
                            "Koniec", MessageBoxButton.OK);
                        testMode = false;
                        TestButtonsPanel.Visibility = Visibility.Collapsed;
                        UpdateInfoText();
                    }
                    else
                    {
                        ShowCurrentCard();
                        UpdateInfoText();
                    }
                });
            });
        }

        private void ShuffleCards()
        {
            for (int i = currentList.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var temp = currentList[i];
                currentList[i] = currentList[j];
                currentList[j] = temp;
            }
        }

        private void UpdateInfoText()
        {
            if (currentList.Count == 0)
            {
                InfoTextBlock.Text = "Wczytaj plik ze słówkami.";
                return;
            }

            string mode = testMode ? "TRYB TESTU" : "TRYB NAUKI";
            string score = testMode ? $" | Poprawne: {correctAnswers}/{totalQuestions} ({Percentage()}%)" : "";
            string cardInfo = $"Słówko {currentIndex + 1}/{currentList.Count}";

            InfoTextBlock.Text = $"{mode} • {cardInfo}{score}";
        }

        private string Percentage()
        {
            if (totalQuestions == 0) return "0";
            return Math.Round((double)correctAnswers / totalQuestions * 100, 1).ToString();
        }
    }
}
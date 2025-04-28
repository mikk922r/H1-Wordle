using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Wordle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> Words = new List<string>();
        private string CurrentWord = string.Empty;
        private int CurrentRow = 1;
        private int CurrentCol = 1;

        public MainWindow()
        {
            InitializeComponent();

            KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            InitWords();

            CurrentWord = "CLIPE"; // Words[Random.Shared.Next(Words.Count)];
        }

        private void InitWords()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../", "words.txt");

            if (!File.Exists(path))
            {
                return;
            }

            string[] words = File.ReadAllLines(path);

            foreach (string word in words)
            {
                Words.Add(word.Trim().ToUpper());
            }
        }

        private string GetCurrentGuess()
        {
            string guess = string.Empty;

            for (int col = 1; col <= 5; col++)
            {
                TextBlock? textBlock = FindName($"R{CurrentRow}C{col}") as TextBlock;

                if (textBlock is null)
                {
                    continue;
                }

                guess += textBlock.Text;
            }

            return guess;
        }

        private void Reset()
        {
            CurrentWord = Words[Random.Shared.Next(Words.Count)];

            CurrentRow = 1;

            CurrentCol = 1;

            for (int row = 1; row <= 6; row++)
            {
                for (int col = 1; col <= 5; col++)
                {
                    TextBlock? textBlock = FindName($"R{row}C{col}") as TextBlock;

                    if (textBlock is null)
                    {
                        continue;
                    }

                    textBlock.Text = string.Empty;

                    Border? border = textBlock.Parent as Border;

                    if (border is null)
                    {
                        continue;
                    }

                    border.Background = Brushes.Transparent;
                }
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EnterButtonClicked(sender, e);
            }
            else if (e.Key == Key.Back)
            {
                DeleteButtonClicked(sender, e);
            }
            else
            {
                Button? button = FindName($"Btn{e.Key}") as Button;

                if (button is null)
                {
                    return;
                }

                LetterButtonClicked(button, e);
            }
        }

        private void LetterButtonClicked(object sender, RoutedEventArgs e)
        {
            if (CurrentWord is null)
            {
                return;
            }

            Button? button = sender as Button;

            if (button is null)
            {
                return;
            }

            if (button.Content is not string letter)
            {
                return;
            }

            TextBlock? textBlock = FindName($"R{CurrentRow}C{CurrentCol}") as TextBlock;

            if (textBlock is null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(textBlock.Text))
            {
                return;
            }

            textBlock.Text = letter.ToUpper();

            if (CurrentCol < 5)
            {
                CurrentCol++;
            }
        }

        private void EnterButtonClicked(object sender, RoutedEventArgs e)
        {
            string guess = GetCurrentGuess();

            if (string.IsNullOrEmpty(guess))
            {
                return;
            }

            if (guess.Length != 5)
            {
                MessageBox.Show("Guess must be 5 letters!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            if (!Words.Contains(guess.ToUpper()))
            {
                MessageBox.Show("Guess is not a valid word!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            Dictionary<char, int> remainingLetters = new Dictionary<char, int>();

            foreach (char c in CurrentWord)
            {
                if (remainingLetters.ContainsKey(c))
                {
                    remainingLetters[c]++;
                }
                else
                {
                    remainingLetters[c] = 1;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                TextBlock? textBlock = FindName($"R{CurrentRow}C{i + 1}") as TextBlock;

                if (textBlock is null)
                {
                    continue;
                }

                if (guess[i] == CurrentWord[i])
                {
                    Border? border = textBlock.Parent as Border;

                    if (border is null)
                    {
                        continue;
                    }

                    border.Background = Brushes.Green;

                    remainingLetters[guess[i]]--;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                TextBlock? textBlock = FindName($"R{CurrentRow}C{i + 1}") as TextBlock;

                if (textBlock is null)
                {
                    continue;
                }

                Border? border = textBlock.Parent as Border;

                if (border is null)
                {
                    continue;
                }

                if (border.Background == Brushes.Green)
                {
                    continue;
                }

                if (remainingLetters.ContainsKey(guess[i]) && remainingLetters[guess[i]] > 0)
                {
                    border.Background = Brushes.Yellow;

                    remainingLetters[guess[i]]--;
                }
                else
                {
                    border.Background = Brushes.Red;
                }
            }

            if (guess.Equals(CurrentWord, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("You guessed the word!", "Congratulations", MessageBoxButton.OK, MessageBoxImage.Information);

                Reset();

                return;
            }

            CurrentRow++;

            CurrentCol = 1;

            if (CurrentRow > 6)
            {
                MessageBox.Show($"You lost! The word was: {CurrentWord}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);

                Reset();
            }
        }

        private void DeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            if (CurrentWord is null)
            {
                return;
            }

            TextBlock? textBlock = FindName($"R{CurrentRow}C{CurrentCol}") as TextBlock;

            if (textBlock is null)
            {
                return;
            }

            if (string.IsNullOrEmpty(textBlock.Text) && CurrentCol > 1)
            {
                CurrentCol--;

                DeleteButtonClicked(sender, e);
            }
            else
            {
                textBlock.Text = string.Empty;
            }
        }
    }
}
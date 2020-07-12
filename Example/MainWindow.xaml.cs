using System;
using System.Collections.Generic;
using System.Windows;
using FitnessCalculator;

namespace Example
{
    public partial class MainWindow : Window
    {
        private static readonly HashSet<char> EnglishAlphabet = new HashSet<char>()
            {
                'A',    'B',    'C',    'D',
                'E',    'F',    'G',    'H',
                'I',    'J',    'K',    'L',
                'M',    'N',    'O',    'P',
                'Q',    'R',    'S',    'T',
                'U',    'V',    'W',    'X',
                'Y',    'Z'
            };
        private static readonly HashSet<char> RussianAlphabet = new HashSet<char>()
            {
                'А',    'Б',    'В',    'Г',
                'Д',    'Е',    'Ё',    'Ж',
                'З',    'И',    'Й',    'К',
                'Л',    'М',    'Н',    'О',
                'П',    'Р',    'С',    'Т',
                'У',    'Ф',    'Х',    'Ц',
                'Ч',    'Ш',    'Щ',    'Ъ',
                'Ы',    'Ь',    'Э',    'Ю',
                'Я'
            };
        private static readonly HashSet<char> GermanAlphabet = new HashSet<char>()
            {
                'A',    'K',    'U',
                'B',    'L',    'V',
                'C',    'M',    'W',
                'D',    'N',    'X',
                'E',    'O',    'Y',
                'F',    'P',    'Z',
                'G',    'Q',    'Ä',
                'H',    'R',    'Ö',
                'I',    'S',    'Ü',
                'J',    'T',    'ß'
            };
        private static readonly Dictionary<Lang, (HashSet<char>, string)> LanguageInfos = new Dictionary<Lang, (HashSet<char>, string)>
        {
            [Lang.ENG] = (EnglishAlphabet, "quadgrams_eng.txt"),
            [Lang.RU] = (RussianAlphabet, "quadgrams_ru.txt"),
            [Lang.DE] = (GermanAlphabet, "quadgrams_de.txt")
        };
        private static readonly Dictionary<Lang, TextFitnessCalculator> Calculators = new Dictionary<Lang, TextFitnessCalculator>
        {
            [Lang.ENG] = null,
            [Lang.RU] = null,
            [Lang.DE] = null
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            Lang lang = (Lang)LangSelector.SelectedIndex;

            if (Calculators[lang] == null)
            {
                (HashSet<char> Alphabet, string QuadgramsDataPath) langInfo = LanguageInfos[lang];

                QuadgramDataset dataset = new QuadgramDataset(langInfo.Alphabet);

                Utils.ParseAndFill(dataset, langInfo.QuadgramsDataPath);

                Calculators[lang] = new TextFitnessCalculator(dataset);
            }

            double fitness = Calculators[lang].GetFitness(TextBoxInput.Text.ToUpper());

            FitnessResult.Content = Math.Round(fitness, 5);
        }

        private enum Lang
        {
            ENG = 0,
            RU = 1,
            DE = 2
        }
    }
}

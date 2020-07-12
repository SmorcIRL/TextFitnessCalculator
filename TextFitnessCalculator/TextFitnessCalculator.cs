using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dawn;

namespace FitnessCalculator
{
    public class TextFitnessCalculator
    {
        private readonly double[,,,] _quadgramFrequencies;
        private readonly Dictionary<char, byte> _posByChar;
        private readonly Dictionary<byte, char> _charByPos;
        private readonly int _alphabetLength;
        private readonly double _fitnessFloor;
        private readonly double _fitnessNormal;

        public TextFitnessCalculator(QuadgramDataset dataset)
        {
            Guard.Argument(dataset, nameof(dataset)).NotNull();
            Guard.Argument(dataset, nameof(dataset)).Require(dataset.Alphabet.Count <= Byte.MaxValue, x => "Alphabet must be no longer than 256 characters");

            _posByChar = new Dictionary<char, byte>();
            _charByPos = new Dictionary<byte, char>();

            char[] alph_arr = dataset.Alphabet.Select(x => Char.ToUpper(x)).ToArray();
            _alphabetLength = alph_arr.Length;

            for (byte i = 0; i < _alphabetLength; i++)
            {
                _posByChar[_charByPos[i] = alph_arr[i]] = i;
            }

            long total_set_size = dataset.SetSize;

            _quadgramFrequencies = new double[_alphabetLength, _alphabetLength, _alphabetLength, _alphabetLength];

            _fitnessFloor = Math.Log10(1.0 / total_set_size);

            Parallel.For(0, _alphabetLength, i_1 =>
            {
                for (int i_2 = 0; i_2 < _alphabetLength; i_2++)
                {
                    for (int i_3 = 0; i_3 < _alphabetLength; i_3++)
                    {
                        for (int i_4 = 0; i_4 < _alphabetLength; i_4++)
                        {
                            _quadgramFrequencies[i_1, i_2, i_3, i_4] = _fitnessFloor;
                        }
                    }
                }
            });

            double f_normal = 0;

            foreach (KeyValuePair<string, long> pair in dataset)
            {
                string quad = pair.Key;
                long quad_count = pair.Value;

                f_normal += (_quadgramFrequencies[_posByChar[quad[0]], _posByChar[quad[1]], _posByChar[quad[2]], _posByChar[quad[3]]] = Math.Log10((double)quad_count / total_set_size));
            }

            _fitnessNormal = Math.Abs(f_normal / dataset.QuadgramsCount);
        }

        public double GetFitness(string text)
        {
            return GetFitness(text.Where(x => _posByChar.ContainsKey(x)).Select(x => _posByChar[x]).ToArray());
        }
        public double GetFitness(byte[] arr, int length = Int32.MaxValue)
        {
            double f_custom = 0;
            int K = Math.Min(arr.Length, length) - 3;

            for (int i = 0; i < K; i++)
            {
                f_custom += _quadgramFrequencies[arr[i], arr[i + 1], arr[i + 2], arr[i + 3]];
            }

            f_custom /= K;

            return Math.Pow((f_custom - _fitnessNormal) / _fitnessNormal, 8);
        }
    }

    public class QuadgramDataset
    {
        private readonly Dictionary<string, long> _quadgrams = new Dictionary<string, long>();

        public HashSet<char> Alphabet { get; }
        public long SetSize => _quadgrams.Values.Sum();
        public int QuadgramsCount => _quadgrams.Count;
        public Dictionary<string, long>.Enumerator GetEnumerator()
        {
            return _quadgrams.GetEnumerator();
        }

        public QuadgramDataset(HashSet<char> alphabet)
        {
            _quadgrams = new Dictionary<string, long>();
            Alphabet = new HashSet<char>(alphabet.Select(x => Char.ToUpper(x)));
        }

        public void Add(string quadgram, long count)
        {
            Guard.Argument(quadgram, nameof(quadgram)).Require(!String.IsNullOrEmpty(quadgram) && quadgram.Length == 4, x => "Invalid quadgram string");
            Guard.Argument(count, nameof(count)).NotNegative();

            for (int i = 0; i < 4; i++)
            {
                Guard.Operation(Alphabet.Contains(quadgram[i]), $"Quadgram contains non-alphabet symbol \"{quadgram[i]}\"");
            }

            if (!_quadgrams.TryAdd(quadgram, count))
            {
                throw new ArgumentException($"Quadgram \"{quadgram}\" is already added");
            }
        }
    }

    public static class Utils
    {
        public static void ParseAndFill(QuadgramDataset dataset, string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    string[] arr = reader.ReadLine().Split(' ');

                    dataset.Add(arr[0], Int64.Parse(arr[1]));
                }
            }
        }
    }
}
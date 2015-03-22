using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoWindow
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string path = "C:\\Users\\tstapleton\\Desktop\\censusData\\wordIncomes.txt";

        public static ConcurrentDictionary<string,int> wordToIncome = new ConcurrentDictionary<string, int>(); 

        public static List<Tuple<int,string>> sortedIncomes = new List<Tuple<int, string>>();

        public static int count = 0;


        public MainWindow()
        {
            InitializeComponent();
            
            List<string> data = File.ReadAllLines(path).ToList();
            
            List<Tuple<int,string>> temp = new List<Tuple<int, string>>();
            foreach (var line in data)
            {
                var splits = line.Split(' ');

                if (splits.Length != 2)
                {
                    continue;
                }

                int value;
                if (!Int32.TryParse(splits[1], out value))
                {
                    count++;
                    continue;
                }

                Tuple<int,string> pair = new Tuple<int, string>(value, splits[0]);

                temp.Add(pair);
            }

            long total = 0;

            foreach (var pair in temp)
            {
                total += pair.Item1;

            }

            int median = (int)(total / (long)temp.Count);

            int adjustment = median - 51000;

            foreach (Tuple<int,string> pair in temp)
            {
                var current = new Tuple<int, string>(pair.Item1 - adjustment, pair.Item2);
                sortedIncomes.Add(current);

                wordToIncome.AddOrUpdate(current.Item2, current.Item1, (x, y) => current.Item1);
            }
            

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Button_Click(null, null);
            }
        }
        


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchBox.Text.ToLower();

            SearchResults.Items.Clear();

            int value;
            if (!wordToIncome.TryGetValue(searchTerm, out value) || sortedIncomes.IndexOf(new Tuple<int, string>(value, searchTerm)) == -1)
            {
                SearchResults.Items.Add(searchTerm + " was not found");
                PreviousSearches.Items.Add(searchTerm + " was not found");
                return;
            }

            var nearest = getNearest(searchTerm, value);

            foreach (var before in nearest.Item1)
            {
                SearchResults.Items.Add(before.Item1 + " ------- " + before.Item2);
            }

            SearchResults.Items.Add(" ");

            SearchResults.Items.Add(searchTerm + " ------- " + value);

            SearchResults.Items.Add(" ");

            foreach (var after in nearest.Item2)
            {
                SearchResults.Items.Add(after.Item1 + " ------- " + after.Item2);
            }

            var item1 = SearchResults.Items[SearchResults.Items.Count - 3];
            var item2 = SearchResults.Items[SearchResults.Items.Count - 2];
            var item3 = SearchResults.Items[SearchResults.Items.Count - 1];

            SearchResults.Items.Insert(5, item1);
            SearchResults.Items.Insert(5, item2);
            SearchResults.Items.Insert(5, item3);

            SearchResults.Items.RemoveAt(SearchResults.Items.Count - 1);
            SearchResults.Items.RemoveAt(SearchResults.Items.Count - 1);
            SearchResults.Items.RemoveAt(SearchResults.Items.Count - 1);

            SearchResults.SelectedIndex = 6;

            PreviousSearches.Items.Insert(0, searchTerm + " ------- " + value);

        }

        public static Tuple<List<Tuple<string, int>>, List<Tuple<string,int>>> getNearest(string word, int value)
        {
            Tuple<int, string> pair = new Tuple<int, string>(value, word);

            int index = sortedIncomes.IndexOf(pair);

            List<Tuple<string,int>> before = new List<Tuple<string,int>>();
            List<Tuple<string,int>> after = new List<Tuple<string,int>>();

            for (int i = index - 5; i < index; i++)
            {
                before.Add(new Tuple<string,int>(sortedIncomes[i].Item2, sortedIncomes[i].Item1));
            }

            for (int i = index + 1; i < index + 6; i++)
            {
                before.Add(new Tuple<string, int>(sortedIncomes[i].Item2, sortedIncomes[i].Item1));
            }

            return new Tuple<List<Tuple<string, int>>, List<Tuple<string, int>>>(before,after);
        }
    }
}

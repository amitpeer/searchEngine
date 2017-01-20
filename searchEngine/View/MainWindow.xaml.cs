using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.IO;
using System.Windows.Forms;
using searchEngine.SearchExecution;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace searchEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string m_pathToCorpus;
        private string m_pathToPosting;
        private bool m_shouldStem;
        private SortedSet<string> m_languages;
        private Controller controller;
        private string errorMessage = "Problem occured. Please follow the instructions.";

        public MainWindow()
        {
           this.controller = new Controller(this);
           m_pathToCorpus="";
           m_pathToPosting="";
           InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var fd = new System.Windows.Forms.FolderBrowserDialog();
            DialogResult result = fd.ShowDialog();
            this.pathToLoadCorpus.Text = fd.SelectedPath;
        }

        private void pathToLoadCorpus_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_pathToCorpus = this.pathToLoadCorpus.Text;
        }

        private void folderPathToPosting_Click(object sender, RoutedEventArgs e)
        {
            var fd = new System.Windows.Forms.FolderBrowserDialog();
            DialogResult result = fd.ShowDialog();
            this.pathToLoadPosting.Text = fd.SelectedPath;
        }
        private void pathToLoadPosting_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_pathToPosting = this.pathToLoadPosting.Text;
        }

        private void startIndexing_Click(object sender, RoutedEventArgs e)

        {
            if (m_pathToCorpus == "")
            {
                System.Windows.Forms.MessageBox.Show("You must type a path to the corpus and stop words","Error",  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (m_pathToPosting == "")
                {
                    System.Windows.Forms.MessageBox.Show("You must type a path to the posting file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    m_shouldStem = checkBox.IsChecked.Value;
                    controller.startIndexing(m_shouldStem, m_pathToCorpus, m_pathToPosting);
                    m_languages = controller.getLanguagesInCorpus();
                    foreach(string lang in m_languages)
                    {
                        comboBox1.Items.Add(lang);
                    }
                    finishedIndexing();
                }

            }
        }

        private void reserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Array.ForEach(Directory.GetFiles(m_pathToPosting), File.Delete);
                comboBox1.Items.Clear();
                controller.reset();
                this.pathToLoadPosting.Text = "";
                this.pathToLoadCorpus.Text = "";
                m_pathToPosting = "";
                m_pathToCorpus = "";
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Nothing to Delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void showDic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, int[]> dicToDisplay = controller.getMainDic();
                Dictionary<string, int> dicTermDis = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int[]> termInfo in dicToDisplay)
                {
                    dicTermDis.Add(termInfo.Key, termInfo.Value[0]);
                }
                dataDisplay windowNew = new dataDisplay(dicTermDis);
                windowNew.ShowDialog();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void finishedIndexing()
        {
            System.Windows.Forms.MessageBox.Show("Number of documents indexed:" + controller.getNumberOfParsedDocs() + "\n" +"Number of unique terms: "+controller.getNumberOfUniqueTerms()+"\n"+"Total time"+controller.getTime());
        }

        private void loadDic_Click(object sender, RoutedEventArgs e)
        {
            if (m_pathToPosting == "")
            {
                System.Windows.Forms.MessageBox.Show("You must type a path to the posting file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                m_shouldStem = checkBox.IsChecked.Value;
                if(!controller.load(m_pathToPosting, m_shouldStem))
                {
                    System.Windows.Forms.MessageBox.Show("You must type a path to a correct folder:with stem/without", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    comboBox1.Visibility = Visibility.Visible;
                    m_languages = controller.getLanguagesInCorpus();
                    comboBox1.Items.Add("All languags");
                    foreach (string lang in m_languages)
                    {
                        comboBox1.Items.Add(lang);
                    }
                    System.Windows.Forms.MessageBox.Show("Files loaded successfully");
                }
            }
       
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            string query = "Space Program";
            Searcher sr = new Searcher(controller);
            Dictionary<string, List<string>> ans = sr.searchFile("", null, false);
            List<string> langsSelected = new List<string>();
            foreach(string lang in comboBox1.SelectedItems)
            {
                langsSelected.Add(lang);
            }
            if(langsSelected.Contains("All languags"))
            {
                langsSelected = null;
            }
            List<string> docsRelevent=sr.search(query, langsSelected, m_shouldStem);
            foreach (string doc in docsRelevent)
            {
                Results.Items.Add(doc);
            }
        }

        private void tb_query_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = tb_query.Text.TrimStart();

            // check the "space" key has been pressed
            if (query.Length > 0 && query[query.Length - 1] == ' ')
            {
                if (query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length == 1)
                {
                    giveSuggestions(query.Trim());
                }
            }
            else // different key pressed - make all suggestion labels invisible
            {
                for (int i = 1; i <= 5; i++)
                {
                    System.Windows.Controls.Label lb = (System.Windows.Controls.Label)FindName("suggestion" + i);
                    lb.Content = "";
                }
            }
        }

        private void giveSuggestions(string query)
        {
            if (controller.getFreqDic() != null)
            {
                List<string> suggestions = controller.getFreqDic().ContainsKey(query.ToLower()) ? controller.getFreqDic()[query.ToLower()] : null;
                if (suggestions!=null && suggestions.Count>1)
                {
                    // found at least one suggestion
                    int i = 1;
                    foreach (string suggest in suggestions)
                    {
                        System.Windows.Controls.Label lb = (System.Windows.Controls.Label)FindName("suggestion" + i);
                        lb.Content = query + " " + suggest;                      
                        i++;
                    }
                }
                else
                {
                    // no suggestions found
                    suggestion1.Content = "no suggestion found";
                }
            }
        }

        private void deSelectbutton1_Click(object sender, RoutedEventArgs e)
        {
            comboBox1.UnselectAll();
        }

        private void suggestion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Label lb = sender as System.Windows.Controls.Label;
            if (!lb.Content.Equals("") && !lb.Content.Equals("no suggestion found"))
            {
                tb_query.Text = lb.Content + "";
                tb_query.SelectionStart = tb_query.Text.Length;
            }
        }

        private void suggestion_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Label lb = sender as System.Windows.Controls.Label;
            if (!lb.Content.Equals("") && !lb.Content.Equals("no suggestion found"))
            {
                Cursor = System.Windows.Input.Cursors.Hand;
            }
        }

        private void suggestion_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cursor = System.Windows.Input.Cursors.Arrow;
        }
    }
}


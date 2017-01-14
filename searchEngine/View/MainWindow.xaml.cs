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
                    m_languages = controller.getLanguagesInCorpus();
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
            string query = "international terrorists";
            Searcher sr = new Searcher(controller);
            sr.search(query, null);
        }
    }
}

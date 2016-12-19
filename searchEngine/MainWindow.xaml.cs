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

namespace searchEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string m_pathToCorpus = "C:\\Users\\amitp\\Documents\\לימודים\\סמסטר ה\\אחזור\\מנוע\\corpus\\small corpus";
        private static readonly string m_pathToSave = "C:\\Users\\amitp\\Documents\\לימודים\\סמסטר ה\\אחזור\\מנוע\\corpus\\results";
        bool m_shouldStem;
       
        public MainWindow()
        {
            InitializeComponent();
            ManageSearch manageSearch = new ManageSearch(m_shouldStem, m_pathToCorpus, m_pathToSave);
            //ManageSearch.main();
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
                    m_stem = checkBox.IsChecked.Value;

                }

            }



        }

        private void reserButton_Click(object sender, RoutedEventArgs e)
        {
            comboBox1.Items.Add("amit");
            comboBox1.Items.Add("adam");
           // Directory.Delete(m_pathToPosting + "\\SearchEngine", true);
            m_pathToPosting = "";
            m_pathToCorpus = "";
        }
    }
}

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
using System.Windows.Shapes;

namespace searchEngine
{
    /// <summary>
    /// Interaction logic for dataDisplay.xaml
    /// </summary>
    public partial class dataDisplay  : Window
    {
            public dataDisplay(Dictionary<string,int> dic)
            {
                InitializeComponent();
                dicTerms = dic;
                DataContext = this;
            }

            public Dictionary<string, int> dicTerms { get; set; }

            private int _selectedBookIndex;
            }
        }


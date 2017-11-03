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

namespace JSONDb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            JsonMaker.JsonMakerAPDB a = new JsonMaker.JsonMakerAPDB();

            a.setString("Pessoas.usuarios[0].nome", "Pedro");
            a.setString("Pessoas.usuarios[0].idade", "25");
            a.setString("Pessoas.usuarios[1].nome", "maria");
            a.setString("Pessoas.usuarios[1].idade", "28");
            a.setString("Pessoas.usuarios[0].emails[0]", "Pedro@provedor.ind.br");
            a.setString("Pessoas.usuarios[0].emails[1]", "Pedro@provedor.ind.br");
            //string ret = a.get("abc");

            //MessageBox.Show(ret);
            MessageBox.Show(a.ToJson());
        }
    }
}

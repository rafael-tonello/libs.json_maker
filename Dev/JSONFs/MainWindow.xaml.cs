using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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

            
        }

        double totalMs = 0;
        Semaphore sm = new Semaphore(0, int.MaxValue);
        int totalThreads = 0;

        public void threadTest(string prefix)
        {
            Thread th = new Thread(delegate ()
            {
                JsonMaker.JsonMakerFS a = new JsonMaker.JsonMakerFS(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                DateTime totalTime = DateTime.Now;
                
                a.setString(prefix + "Pessoas.usuarios[0].nome", "Pedro");
                a.setString(prefix + "Pessoas.usuarios[0].idade", "25");
                a.setString(prefix + "Pessoas.usuarios[1].nome", "maria");
                a.setString(prefix + "Pessoas.usuarios[1].idade", "28");
                a.setString(prefix + "Pessoas.usuarios[0].emails[0]", "Pedro@provedor.ind.br");
                a.setString(prefix + "Pessoas.usuarios[0].emails[1]", "Pedro@provedor.ind.br");
                
                double total = DateTime.Now.Subtract(totalTime).TotalMilliseconds;

                sm.WaitOne();
                totalMs = totalMs + total;
                totalThreads--;
                sm.Release();
                
            });
            th.Start();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread th = new Thread(delegate ()
            {
                totalMs = 0;
                totalThreads = 0;
                int maxThreads = 100;

                for (int cont = 0; cont < maxThreads; cont++)
                {
                    this.threadTest(cont.ToString());
                    totalThreads++;
                }
                sm.Release();

                while (totalThreads > 0)
                    Thread.Sleep(10);

                this.Dispatcher.Invoke(delegate ()
                {
                    MessageBox.Show((totalMs / maxThreads).ToString() + " ms");

                });


            });
            th.Start();


        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dev
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            JsonMaker.JsonMaker jm = new JsonMaker.JsonMaker();
            //jm.add("pai.filho1", "valor do filho1");

            jm.add("pai", "{f1:[{f2:\"f3\"}, \"f4\"]}");
            MessageBox.Show(jm.get("pai"));
            MessageBox.Show(jm.get("pai.f1"));
            MessageBox.Show(jm.get("pai.f1[0]"));
            MessageBox.Show(jm.get("pai.f1[1]"));
            MessageBox.Show(jm.get("pai.f1[0].f2"));
            MessageBox.Show(jm.ToJson());

            jm.add("pai.filho1[0]", "valor do filho1.1");
            jm.add("pai.filho1[1]", "valor do filho1.2");

            jm.add("pai.filho2", "valor do filho2");

            //jm.add("pai.filho3", "valor do filho3");
            jm.add("pai.filho3.filho3_1", "valor do filho3.1");
            jm.add("pai.filho3.filho3_2", "valor do filho3.2");

            MessageBox.Show(jm.get("pai"));
            MessageBox.Show(jm.get("pai.filho1"));
            MessageBox.Show(jm.get("pai.filho1[0]"));
            MessageBox.Show(jm.get("pai.filho1[1]"));
            MessageBox.Show(jm.get("pai.filho2"));
            MessageBox.Show(jm.get("pai.filho3"));
            MessageBox.Show(jm.get("pai.filho3.filho3_1"));
            MessageBox.Show(jm.get("pai.filho3.filho3_2"));
            MessageBox.Show(jm.get("pai.filho1.filho1[2]"));
            MessageBox.Show(jm.ToJson());



            jm.add("pai", "valor do pai");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            JsonMaker.JsonMaker jm = new JsonMaker.JsonMaker();
            jm.fromJson(textBox1.Text);
            textBox1.Text = jm.ToJson();
        }
    }
}

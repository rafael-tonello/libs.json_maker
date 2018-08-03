using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
            JsonMaker.JSON jm = new JsonMaker.JSON();
            //jm.add("pai.filho1", "valor do filho1");
            bool format = true;

            jm.set("pai", "{f1:[{f2:\"f3\"}, \"f4\"]}");
            MessageBox.Show(jm.get("pai", format));
            MessageBox.Show(jm.get("pai.f1", format));
            MessageBox.Show(jm.get("pai.f1[0]", format));
            MessageBox.Show(jm.get("pai.f1[1]", format));
            MessageBox.Show(jm.get("pai.f1[0].f2", format));
            MessageBox.Show(jm.ToJson());

            jm.set("pai.filho1[0]", "valor do filho1.1");
            jm.set("pai.filho1[1]", "valor do filho1.2");

            jm.set("pai.filho2", "valor do filho2");

            //jm.add("pai.filho3", "valor do filho3");
            jm.set("pai.filho3.filho3_1", "valor do filho3.1");
            jm.set("pai.filho3.filho3_2", "valor do filho3.2");

            MessageBox.Show(jm.get("pai", format));
            MessageBox.Show(jm.get("pai.filho1", format));
            MessageBox.Show(jm.get("pai.filho1[0]", format));
            MessageBox.Show(jm.get("pai.filho1[1]", format));
            MessageBox.Show(jm.get("pai.filho2", format));
            MessageBox.Show(jm.get("pai.filho3", format));
            MessageBox.Show(jm.get("pai.filho3.filho3_1", format));
            MessageBox.Show(jm.get("pai.filho3.filho3_2", format));
            MessageBox.Show(jm.get("pai.filho1.filho1[2]", format));
            MessageBox.Show(jm.ToJson(format));



            jm.setString("pai", "valor do pai");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            JsonMaker.JSON jm = new JsonMaker.JSON();
            jm.fromJson(textBox1.Text);
            textBox1.Text = jm.ToJson();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            JsonMaker.JSON jm = new JsonMaker.JSON(JsonMaker.JSON.JsonType.File, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\test");
            jm.fromJson(textBox1.Text);
            textBox1.Text = jm.ToJson();
        }
    }
}

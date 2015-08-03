using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using PublicSpeechHelper;
using PublicSpeechHelper.Helpers;
using PublicSpeechHelper.SpeechApi;
using SpeechLib;


namespace PublicSpeechHelperTest
{


    //TODO speech groups multiple input different methods

    //TODO dynamic add/remove and enable disable methods

    //TODO rename all the event parameter types...

    //TODO enable all disable command (on/off like xbox on/ xbox off)

    [SpeechEnabled]
    public partial class Form1 : Form
    {

        private readonly SpeechHelper helper;


        public Form1()
        {
            InitializeComponent();

            

            var clt = CultureInfo.CurrentCulture;
            
            var vlt = clt.DisplayName;

            helper = new SpeechHelper("de-de");

            //grab some speech methods
            //helper.GatherCommands(Assembly.GetExecutingAssembly());
            helper.GatherCommands(typeof(Form1), this);

            //helper.AddSimpleCommand("de-de", "abbrechen");

            //build the speech recognition (words)
            helper.RebuildAllCommands();


            helper.OnTextRecognized.Subscribe(p =>
            {
                listBox1.Items.Add(p.Text);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                //helper.Speak("ok");
            });

            helper.OnListeningParameters.Subscribe(tuple =>
            {
                helper.Speak("jetzt die parameter");

            });

            helper.OnParameterRecognized.Subscribe(p =>
            {
                helper.Speak("wert für " + p.SpeechParameterInfo.Parameter.ParameterInfo.Name + " wird erwartet");
            });

            helper.OnParameterFinished.Subscribe(p =>
            {
                //helper.
                helper.Speak("parameter " + p.SpeechParameterInfo.Parameter.ParameterInfo.Name + " fertig");
            });

            helper.OnBeforeMethodInvoked.Subscribe(p =>
            {
                //p.Method.
                //helper.Speak("methode wird ausgeführt");
            });

            helper.OnLastParameterFinished.Subscribe(p =>
            {
                helper.Speak("methode " + p.RecognizedText + " wird jetzt ausgeführt");
            });


        }

        [SpeechMethod("de-de", "test", Key = "1")]
        public void TestMethod0(
            [SpeechParameter("de-de","x","setzte x gleich")] 
            int x,
            [SpeechParameter("de-de", "y", "setzte x gleich")] 
            int y
            )
        {
            textBox1.Text += @"Das ist nur ein Test (" + x + @", " + y + @")!" + Environment.NewLine;
        }

        [SpeechMethod("de-de", "test2", Key = "2")]
        public void TestMethod9()
        {
            textBox1.Text += @"Das ist nur ein Test2!" + Environment.NewLine;
        }

        [SpeechMethod("de-de", "leeren")]
        public void TestMethod1()
        {
            textBox1.Clear();
        }

        [SpeechMethod("de-de", "stopp")]
        public void s1()
        {
            helper.Speak("stoppe");
            helper.StopListening();
            this.Text = "Stopped";
        }

        [SpeechMethod("de-de", "fenster schließen", "exit", "ende")]
        public void TestMethod2()
        {
            helper.Speak("bis zum nächsten mal ...");
            this.Close();
        }

        [SpeechMethod("de-de", "sage etwas")]
        public void TestMethod3()
        {
            helper.Speak("was soll ich sagen?");
        }

        [SpeechMethod("de-de", "was kann ich sagen", "hilfe")]
        public void TestMethod4()
        {
            textBox1.Clear();
            foreach (var publicSpeechMethod in helper.AllCommands.Commands[helper.CurrentInputCulture])
            {
                textBox1.Text += "Methode: " + Environment.NewLine;


                textBox1.Text += '\t' + publicSpeechMethod.Key + " (" + publicSpeechMethod.Value.Count + ")" + Environment.NewLine;
                

            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            helper.Speak("starte");
            helper.StartListening();
            this.Text = "Listening...";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            helper.Speak("stoppe");
            helper.StopListening();
            this.Text = "Stopped";
        }
    }
}

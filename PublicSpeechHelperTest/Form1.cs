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

using SpeechLib;


namespace PublicSpeechHelperTest
{

    //TODO use dict for better performance
    //TODO performance improvements
    //TODO all rename to parameters

    //TODO parameter testen mit vorgefertigter auswahl

    //TODO speech groups multiple input different methods

    //TODO dynamic add remove and enable disable methods

    [PublicSpeechEnabled]
    public partial class Form1 : Form
    {

        private PublicSpeechHelper.PublicSpeechHelper helper;

        private SpSharedRecoContext recognitionContext = null;
        private ISpRecoGrammar recognitionGrammar = null;

        public Form1()
        {
            InitializeComponent();


            var clt = CultureInfo.CurrentCulture;
            
            var vlt = clt.DisplayName;

            helper = new PublicSpeechHelper.PublicSpeechHelper("de-de");
            helper.SetInputCulture(CultureInfo.GetCultureInfo("de-DE"));

            helper.GatherCommands(Assembly.GetExecutingAssembly());
            helper.RebuildCommands();
            helper.OnTextRecognized.Subscribe(p =>
            {
                listBox1.Items.Add(p.Text);
                helper.Speak("ok");
            });

            helper.OnUserInvokeMethod.Subscribe(method =>
            {
                method.Invoke(this, null);
            });

        }

        [PublicSpeechMethod("de-de", "test")]
        public void TestMethod0()
        {
            textBox1.Text += @"Das ist nur ein Test!" + Environment.NewLine;
        }

        [PublicSpeechMethod("de-de", "leeren")]
        public void TestMethod1()
        {
            textBox1.Clear();
        }


        [PublicSpeechMethod("de-de", "fenster schließen", "exit", "ende")]
        public void TestMethod2()
        {
            helper.Speak("bis zum nächsten mal ...");
            this.Close();
        }

        [PublicSpeechMethod("de-de", "sage etwas")]
        public void TestMethod3()
        {
            helper.Speak("was soll ich sagen?");
        }

        [PublicSpeechMethod("de-de", "was kann ich sagen", "hilfe")]
        public void TestMethod4()
        {
            //textBox1.Clear();
            foreach (var publicSpeechMethod in helper.Commands[helper.CurrentCulture])
            {
                textBox1.Text += "Methode: " + Environment.NewLine;

                foreach (var speechName in publicSpeechMethod.SpeechNames)
                {
                    textBox1.Text +=  '\t' + speechName + Environment.NewLine;
                }
                
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            helper.StartListening();
            this.Text = "Listening...";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            helper.StopListening();
            this.Text = "Stopped";
        }
    }
}

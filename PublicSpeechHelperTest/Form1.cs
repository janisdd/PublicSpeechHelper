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

    //TODO rename all the event parameter types...

    //TODO check all possibilities when chainging and leaving parameters (names) out

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

            helper.OnParameterValueConvert = (p, value) =>
            {
                int i;
                if(int.TryParse(value, out i))
                {
                    return i;
                }
                return null;
            };
            //grab some speech methods
            //helper.GatherCommands(Assembly.GetExecutingAssembly());
            helper.GatherCommands(typeof(Form1), this);

            helper.AddSimpleCommand("de-de", "abbrechen", () =>
            {
                if (helper.State == ExecutingState.ListeningForParameterValue)
                {
                    helper.Speak("ab jetzt wird wieder ausschau nach parametern gehalten");
                    helper.AbortListeningForCurrentParameterValue();
                }
                else if (helper.State == ExecutingState.ListeningForParameters)
                {
                    helper.Speak("ab jetzt werden wieder methoden überwacht");
                    helper.AbortListeningForParameters();
                    
                }
            });

            /*
            helper.AddSimpleCommand("de-de", "los",() =>
            {
                textBox1.Text += "los" + Environment.NewLine;

            });

            helper.AddSimpleCommand("de-de", "ja", () =>
            {
                helper.ChangeSimpleCommand("de-de", "los", false);
            });

            helper.AddSimpleCommand("de-de", "nein", () =>
            {
                helper.ChangeSimpleCommand("de-de", "los", true);
            });
             * */

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
                textBox1.Text += tuple.RecognizedText + "(";
                helper.Speak("jetzt die parameter");
            });

            helper.OnParameterRecognized.Subscribe(p =>
            {
                textBox1.Text += p.RecognizedParameterNameText + ": ";
                helper.Speak("wert für " + p.SpeechParameterInfo.Parameter.ParameterInfo.Name + " wird erwartet");
            });

            helper.OnParameterFinished.Subscribe(p =>
            {
                textBox1.Text += p.SpeechParameterInfo.Value + ", ";
                helper.Speak("parameter " + p.SpeechParameterInfo.Parameter.ParameterInfo.Name + " fertig");
            });

            helper.OnBeforeMethodInvoked.Subscribe(p =>
            {
                //p.Method.
                //helper.Speak("methode wird ausgeführt");
            });

            helper.OnLastParameterFinished.Subscribe(p =>
            {
                textBox1.Text += ")" + Environment.NewLine;
                helper.Speak("methode " + p.RecognizedText + " wird jetzt ausgeführt");
            });


            //helper.ChangeCommand("", false);


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

        [SpeechMethod("de-de", "test", Key = "1")]
        public void TestMethod0(
            [SpeechParameter("de-de", "x", "x gleich")] 
            int x,
            [SpeechParameter("de-de", "y", "x gleich")] 
            int y
            )
        {
            textBox1.Text += @"Das ist nur ein Test (x: " + x + @", y: " + y + @")!" + Environment.NewLine;
        }

        //[SpeechMethod("de-de", "test","abc")]
        //public void TestMethod0()
        //{
        //    textBox1.Text += @"Das ist nur ein Test ()!" + Environment.NewLine;
        //}

        //[SpeechMethod("de-de", "test2", Key = "2")]
        public void TestMethod9()
        {
            textBox1.Text += @"Das ist nur ein Test2!" + Environment.NewLine;
        }

        //[SpeechMethod("de-de", "leeren")]
        public void TestMethod1()
        {
            textBox1.Clear();
        }

        //[SpeechMethod("de-de", "stopp")]
        public void s1()
        {
            helper.Speak("stoppe");
            helper.StopListening();
            this.Text = "Stopped";
        }

        //[SpeechMethod("de-de", "fenster schließen", "exit", "ende")]
        public void TestMethod2()
        {
            helper.Speak("bis zum nächsten mal ...");
            this.Close();
        }

        //[SpeechMethod("de-de", "sage etwas")]
        public void TestMethod3()
        {
            helper.Speak("was soll ich sagen?");
        }

        [SpeechMethod("de-de", "was kann ich sagen", "hilfe")]
        public void TestMethod4()
        {
            textBox1.Clear();
            foreach (var speechGroup in helper.AllCommands.Commands[helper.CurrentInputCulture])
            {
                textBox1.Text += "Groups: " + Environment.NewLine;

                textBox1.Text += '\t' + speechGroup.Key + " (" + speechGroup.Value.Commands.Count + ")" + Environment.NewLine;


                foreach (var speechTuple in speechGroup.Value.Commands)
                {
                    textBox1.Text += @"		" + speechTuple.Key + Environment.NewLine;
                }
            }
        }


    }
}

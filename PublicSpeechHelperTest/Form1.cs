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

    //TODO check converter match to method

    //TODO rename all the event parameter types...
    //TODO check all events maybe we can give some more information
    //TODO pretend input

    //TODO switch to { choice1, choice2 } -> function switchto (string choice) -> normal switch to speech name witch parameter
    // plus grammar builder switch to { choices } for fast access?
    //switch to class 10A

    //error when start and stop and start called

    [SpeechEnabled]
    public partial class Form1 : Form
    {

        private readonly SpeechHelper helper;
        private List<string> items;


        public Form1()
        {
            InitializeComponent();





            var clt = CultureInfo.CurrentCulture;

            var vlt = clt.DisplayName;

            helper = new SpeechHelper("de-de");
            helper.GatherConverters(typeof(Form1), this);

            //helper.OnParameterValueConvert = (p, value) =>
            //{
            //    int i;
            //    if(int.TryParse(value, out i))
            //    {
            //        return i;
            //    }
            //    return null;
            //};

            //grab some speech methods
            //helper.GatherCommands(Assembly.GetExecutingAssembly());
            helper.GatherCommands(typeof(Form1), this);

            helper.AddSimpleCommand("de-de", "abbrechen", "abc", () =>
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

            //helper.ChangeCommand("", false);
            //helper.ChangeSimpleCommand("de-de", "abbrechen", false);

            //helper.ChangeSpeechGroup("", true);
            //helper.ChangeSimpleSpeechGroup("de-de", "abc", true);


            helper.AddSimpleCommand("de-de", "los", "", () =>
            {
                textBox1.Text += "los" + Environment.NewLine;

            });
            /*
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
                if (checkBox1.Checked)
                helper.Speak("jetzt die parameter");
            });

            helper.OnParameterRecognized.Subscribe(p =>
            {
                textBox1.Text += p.RecognizedParameterNameText + ": ";
                if (checkBox1.Checked)
                helper.Speak("wert für " + p.SpeechParameterInfo.Parameter.ParameterInfo.Name + " wird erwartet");
            });

            helper.OnParameterFinished.Subscribe(p =>
            {
                textBox1.Text += p.SpeechParameterInfo.Value + ", ";
                if (checkBox1.Checked)
                helper.Speak("parameter " + p.SpeechParameterInfo.Parameter.ParameterInfo.Name + " fertig");
            });

            helper.OnBeforeMethodInvoked.Subscribe(p =>
            {
                //p.Method.
                if (checkBox1.Checked)
                helper.Speak("methode wird ausgeführt");
            });

            helper.OnLastParameterFinished.Subscribe(p =>
            {
                textBox1.Text += ")" + Environment.NewLine;
                if (checkBox1.Checked)
                    helper.Speak("methode " + p.RecognizedText + " wird jetzt ausgeführt");
            });


            items = new List<string>();

            var mmmMax = 11;
            for (int i = 0; i < mmmMax; i++)
            {
                items.Add("item " + (10 - i).ToString());
                listBox2.Items.Add(items[i]);

                if (i == mmmMax - 1)
                    helper.AddPlainPhrase(items[i], true);
                else
                    helper.AddPlainPhrase(items[i], false);
            }



            //helper.ChangeCommand("", false);


        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            helper.SpeakAsync("starte");
            helper.StartListening();
            this.Text = "Listening...";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            helper.SpeakAsync("stoppe");
            helper.StopListening();
            this.Text = "Stopped";
        }


        [SpeechParameterConverter("intConv")]
        public int ConvertInt(string value)
        {
            int i;
            if (int.TryParse(value, out i))
            {
                return i;
            }
            return -1;
        }

        [SpeechParameterConverter("choiceConv")]
        public string ConvertChoice(string value)
        {
            if (char.IsNumber(value[0]))
            {
                int i;
                if (int.TryParse(value, out i))
                {
                    if (i < this.items.Count)
                        return this.items[i];
                }
            }

            return value;
        }

        [SpeechMethod("de-de", "test", Key = "1")]
        public void TestMethod0(
            [SpeechParameter("de-de", "intConv", "x", "x gleich")]
            int x,
            [SpeechParameter("de-de", "intConv", "y", "x gleich")]
            int y
            )
        {
            textBox1.Text += @"Das ist nur ein Test (x: " + x + @", y: " + y + @")!" + Environment.NewLine;
        }

        [SpeechMethod("de-de", "gehe zu")]
        public void TestListe(
            [SpeechParameter("de-de", "choiceConv", "item")]
            string item
            )
        {
            //textBox1.Text += "gehe zu " + item + Environment.NewLine;
            listBox2.SelectedItem = item;
        }

        //[SpeechMethod("de-de", "test","abc")]
        //public void TestMethod0()
        //{
        //    textBox1.Text += @"Das ist nur ein Test ()!" + Environment.NewLine;
        //}

        [SpeechMethod("de-de", "test2", "asdasd", Key = "2")]
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

            foreach (var speechGroup in helper.AllCommands.Commands[helper.CurrentInputCulture])
            {
                textBox1.Text += "Befehlt für " + helper.CurrentInputCulture + ": " + Environment.NewLine;

                textBox1.Text += '\t' + speechGroup.Key + " (" + speechGroup.Value.Commands.Count + ")" + Environment.NewLine;


                foreach (var speechTuple in speechGroup.Value.Commands)
                {
                    textBox1.Text += @"		" + speechTuple.Key + Environment.NewLine;
                }
            }

            foreach (var langCommand in helper.AllCommands.SimpleCommands)
            {
                textBox1.Text += "Simple Groups: " + Environment.NewLine;
                textBox1.Text += '\t' + langCommand.Key + Environment.NewLine;

                foreach (var simpleSpeechGroupTuple in langCommand.Value)
                {
                    textBox1.Text += @"		Group Key: " + simpleSpeechGroupTuple.Key + Environment.NewLine;

                    foreach (var simpleCommandTuple in simpleSpeechGroupTuple.Value.Commands)
                    {
                        textBox1.Text += @"				" + simpleCommandTuple.Key + Environment.NewLine;
                    }

                }

            }

        }



    }
}

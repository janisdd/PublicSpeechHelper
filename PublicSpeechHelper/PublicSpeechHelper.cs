using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;

namespace PublicSpeechHelper
{
    public class PublicSpeechHelper
    {

        private bool AtMethodExecution;
        public PublicSpeechMethod currentMethod;
        public List<object> currentParams;

        private Subject<PublicSpeechRecognitionArgs> _OnTextRecognized;
        private Subject<MethodInfo> _OnUserInvokeMethod;

        /// <summary>
        /// occurs when speech to text is recognized
        /// </summary>
        public IObservable<PublicSpeechRecognitionArgs> OnTextRecognized
        {
            get
            {
                return _OnTextRecognized.AsObservable();
            }
        }

        public IObservable<MethodInfo> OnUserInvokeMethod
        { get { return this._OnUserInvokeMethod.AsObservable(); } }


        private SpeechSynthesizer _speaker;
        private SpeechRecognitionEngine _engine;


        private Dictionary<string, List<PublicSpeechMethod>> _commands;

        public Dictionary<string, List<PublicSpeechMethod>> Commands
        {
            get { return _commands; }
        }

        private string _currentCulture;

        public string CurrentCulture
        {
            get { return _currentCulture; }
        }



        public PublicSpeechHelper()
        {
            _OnTextRecognized = new Subject<PublicSpeechRecognitionArgs>();
            _OnUserInvokeMethod = new Subject<MethodInfo>();

            //set up speaker
            _speaker = new SpeechSynthesizer();
            this.SetVoice(VoiceGender.Female, CultureInfo.CurrentCulture);

            //set up listener
            SetInputCulture(CultureInfo.CurrentCulture);

        }

        public PublicSpeechHelper(string culture) : this()
        {
            SetCulture(culture);
        }


        public void SetCulture(string culture)
        {
            this._currentCulture = culture;
        }


        public void GatherCommands(Assembly targetAssembly)
        {
            var types = targetAssembly.GetExportedTypes();
            CrawlTypes(types);
        }


        private void CrawlTypes(Type[] types)
        {

            _commands = new Dictionary<string, List<PublicSpeechMethod>>();

            foreach (var type in types)
            {

                if (type.GetCustomAttributes(typeof(PublicSpeechEnabled), false).Length > 0)
                {
                    var methods = type.GetMethods();

                    foreach (var methodInfo in methods)
                    {
                        Dictionary<string, PublicSpeechMethod> langMethods = crawlMethod(methodInfo, type); //get all possible speech methods out of the method

                        //add them to the dictionary
                        foreach (var lang in langMethods.Keys)
                        {
                            if (_commands.ContainsKey(lang) == false)
                            {
                                var tempList = new List<PublicSpeechMethod>();
                                _commands.Add(lang, tempList);
                                tempList.Add(langMethods[lang]);
                            }
                            else
                            {
                                _commands[lang].Add(langMethods[lang]);
                            }
                        }

                    }
                }

            }
        }

        private Dictionary<string, PublicSpeechMethod> crawlMethod(MethodInfo methodInfo, Type type)
        {
            var mattributes = methodInfo.GetCustomAttributes(typeof(PublicSpeechMethodAttribute), false);

            var crawledMethods = new Dictionary<string, PublicSpeechMethod>();

            if (mattributes.Length > 0)
            {

                IEnumerable<PublicSpeechMethodAttribute> speechMethodAttributes = mattributes.Cast<PublicSpeechMethodAttribute>();

                foreach (var speechMethodAttribute in speechMethodAttributes)
                {
                    //set method parameters
                    var speechMethod = new PublicSpeechMethod();
                    speechMethod.Lang = speechMethodAttribute.Lang;
                    speechMethod.SpeechNames.AddRange(speechMethodAttribute.SpeechNames);
                    speechMethod.Info = methodInfo;

                    crawledMethods.Add(speechMethod.Lang, speechMethod);
                }

                //now get possible parameters
                var parameters = methodInfo.GetParameters();
                foreach (var parameterInfo in parameters)
                {
                    var pattributes = parameterInfo.GetCustomAttributes(typeof(PublicSpeechArgumentAttribute), false);

                    if (pattributes.Length > 0)
                    {
                        var parameterAttributes = pattributes.Cast<PublicSpeechArgumentAttribute>();

                        foreach (var publicSpeechArgumentAttribute in parameterAttributes)
                        {

                            //set parameter parameters
                            var speechParameter = new PublicSpeechArgument();
                            speechParameter.SpeechName = publicSpeechArgumentAttribute.SpeechName;

                            PublicSpeechMethod speechMethod;

                            if (crawledMethods.TryGetValue(publicSpeechArgumentAttribute.Lang, out speechMethod))
                            {
                                speechMethod.Arguments.Add(speechParameter);
                            }
                            else
                                throw new Exception("No PublicSpeechMethodAttribute specified for parameter: " +
                                    parameterInfo.Name + " for lang: " + publicSpeechArgumentAttribute.Lang + " on type: " + type.FullName);

                        }

                    }
                }
            }

            return crawledMethods;
        }


        #region speech helping reagion

        #region text to speech (output)

        /// <summary>
        /// configures the current voice
        /// </summary>
        /// <param name="voiceGender">the voice gender</param>
        /// <param name="cultureInfo">the culture info</param>
        public void SetVoice(VoiceGender voiceGender, CultureInfo cultureInfo)
        {
            _speaker.SetOutputToDefaultAudioDevice();
            _speaker.SelectVoiceByHints(voiceGender, VoiceAge.NotSet, 0, cultureInfo);
            _speaker.Rate = 1;
            _speaker.Volume = 100;
        }

        /// <summary>
        /// outputs the given text with the current voice
        /// </summary>
        /// <param name="text">the text to output</param>
        public void Speak(string text)
        {
            _speaker.Speak(text);
        }

        #endregion

        #region speech to text (input)

        /// <summary>
        /// sets the culture for the speech to text engine (input)
        /// </summary>
        /// <param name="cultureInfo"></param>
        public void SetInputCulture(CultureInfo cultureInfo)
        {
            _engine = new SpeechRecognitionEngine(cultureInfo);
            _engine.SetInputToDefaultAudioDevice();

            _engine.SpeechRecognized += EngineOnSpeechRecognized;
        }


        private void EngineOnSpeechRecognized(object sender, SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            var token = new PublicSpeechRecognitionArgs(speechRecognizedEventArgs.Result.Text);
            this._OnTextRecognized.OnNext(token);

            if (token.CancelFurtherCommands == false)
            {
                if (AtMethodExecution == false)
                {
                    //look for method
                    LookForCommands(token.Text);
                }
                else
                {
                    //look for parameters...

                    //this.currentMethod.Arguments.

                }

            }
        }


        private void LookForCommands(string text)
        {
            List<PublicSpeechMethod> possibleCommands;
            if (this._commands.TryGetValue(this._currentCulture, out possibleCommands))
            {
                foreach (var publicSpeechMethod in possibleCommands)
                {
                    if (publicSpeechMethod.SpeechNames.Contains(text))
                    {
                        currentMethod = publicSpeechMethod;

                        if (currentMethod.Arguments.Count == 0)
                        {
                            _OnUserInvokeMethod.OnNext(currentMethod.Info);
                        }
                        else
                        {
                            AtMethodExecution = true;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// rebuilds the commands with the current culture
        /// </summary>
        public void RebuildCommands()
        {
            var choices = new Choices();

            //add every method
            foreach (var publicSpeechMethod in this._commands[this._currentCulture])
            {
                publicSpeechMethod.SpeechNames.ForEach(p => choices.Add(p));

                //add every parameter of the method
                foreach (var publicSpeechArgument in publicSpeechMethod.Arguments)
                    choices.Add(publicSpeechArgument.SpeechName);
            }

            var gb = new GrammarBuilder(choices);
            var grammar = new Grammar(gb);

            _engine.LoadGrammar(grammar);

        }


        /// <summary>
        /// starts listening for commands
        /// </summary>
        public void StartListening()
        {
            _engine.RecognizeAsync(RecognizeMode.Multiple);
        }

        /// <summary>
        /// stops listening for commands
        /// </summary>
        public void StopListening()
        {
            _engine.RecognizeAsyncStop();
        }


        #endregion




        #endregion


        public void Dispose()
        {
            _engine.Dispose();
            _speaker.Dispose();
            _commands = null;
            _currentCulture = "";
        }

    }
}

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
using PublicSpeechHelper.Helpers;
using PublicSpeechHelper.SpeechApi;

namespace PublicSpeechHelper
{
    /// <summary>
    /// <para />
    /// after changing the language rebuild the commands!
    /// </summary>
    public class SpeechHelper
    {

        public Grammar PlainPhraseGrammar;
        public Grammar SimpleCommandGrammar;
        public Grammar CommandGrammar;

        /// <summary>
        /// the range from NumbersFrom to NumbersTo is added as a plain phrase
        /// </summary>
        public int NumbersFrom = 0;

        /// <summary>
        /// the range from NumbersFrom to NumbersTo is added as a plain phrase
        /// </summary>
        public int NumbersTo = 20;


        #region custom event stuff


        private readonly Subject<int> _OnAudioLevelUpdated;

        /// <summary>
        /// occurs when the aduo level changes
        /// </summary>
        public IObservable<int> OnAudioLevelUpdated
        {
            get
            {
                return _OnAudioLevelUpdated.AsObservable();
            }
        }


        #region simple command events

        private Subject<BeforeSimpleCommandInvokedArgs> _OnBeforeSimpleCommandInvoked;

        /// <summary>
        /// occurs before a simple command is invoked
        /// </summary>
        public IObservable<BeforeSimpleCommandInvokedArgs> OnBeforeSimpleCommandInvoked
        {
            get
            {
                return this._OnBeforeSimpleCommandInvoked.AsObservable();
            }
        }


        private Subject<AfterSimpleComamndInokedArgs> _OnAfterSimpleCommandInvoked;

        /// <summary>
        /// occurs after a simple command was invoked
        /// </summary>
        public IObservable<AfterSimpleComamndInokedArgs> OnAfterSimpleCommandInvoked
        {
            get
            {
                return this._OnAfterSimpleCommandInvoked.AsObservable();
            }
        }


        #endregion

        #region speech command events

        private readonly Subject<SpeechRecognitionArgs> _OnTextRecognized;
        private readonly Subject<SpeechStream> _OnListeningForParameters;

        private readonly Subject<SpeechParameterStream> _OnParameterRecognized;
        private readonly Subject<SpeechParameterStream> _OnParameterFinished;
        private readonly Subject<SpeechStream> _OnLastParameterFinished;

        private readonly Subject<SpeechTupleArgs> _OnBeforeMethodInvoked;
        private readonly Subject<SpeechTuple> _OnAfterMethodInvoked;


        /// <summary>
        /// function called when a parameter value text is recognized, 2. parameter is the string value
        /// </summary>
        //public Func<SpeechParameterStream, string, object> OnParameterValueConvert;

        /// <summary>
        /// occurs when speech to text is recognized (before any command is executed)
        /// </summary>
        public IObservable<SpeechRecognitionArgs> OnTextRecognized
        {
            get
            {
                return _OnTextRecognized.AsObservable();
            }
        }

        /// <summary>
        /// occurs when the method was detected and now waiting for parameters
        /// </summary>
        public IObservable<SpeechStream> OnListeningParameters
        {
            get
            {
                return this._OnListeningForParameters.AsObservable();
            }
        }

        /// <summary>
        /// occurs when a parameter for the current method is recognized
        /// </summary>
        public IObservable<SpeechParameterStream> OnParameterRecognized
        {
            get
            {
                return this._OnParameterRecognized.AsObservable();
            }
        }


        /// <summary>
        /// occurs when a parameter for the current method is finished (value is assigned)
        /// </summary>
        public IObservable<SpeechParameterStream> OnParameterFinished
        {
            get
            {
                return this._OnParameterFinished.AsObservable();
            }
        }

        /// <summary>
        /// occurs before a method is invoked
        /// </summary>
        public IObservable<SpeechTupleArgs> OnBeforeMethodInvoked
        {
            get
            {
                return this._OnBeforeMethodInvoked.AsObservable();
            }
        }

        /// <summary>
        /// occurs befor a method is invoked
        /// </summary>
        public IObservable<SpeechTuple> OnAfterMethodInvoked
        {
            get
            {
                return this._OnAfterMethodInvoked.AsObservable();
            }
        }

        /// <summary>
        /// occurs after the last parameter is assigned and befor the method is executed
        /// </summary>
        public IObservable<SpeechStream> OnLastParameterFinished
        {
            get
            {
                return this._OnLastParameterFinished.AsObservable();
            }
        }

        #endregion

        #endregion


        /// <summary>
        /// the current speech group key or "" to search in every speech group
        /// </summary>
        public string CurrentSpeechGroupKey { get; set; }

        /// <summary>
        /// the current simple speech group key or "" to search in every simple speech group
        /// </summary>
        public string CurrentSimpleSpeechGroupKey { get; set; }

        public readonly SpeechSynthesizer _speaker;
        private SpeechRecognitionEngine _engine;

        /// <summary>
        /// a temp var to store all command choices when adding 1 command
        /// </summary>
        private Choices _currentCommandChoices;

        /// <summary>
        /// stores all commands and information
        /// </summary>
        public SpeechDictionary AllCommands { get; set; }

        /// <summary>
        /// the current state of the helper
        /// </summary>
        public ExecutingState State { get; set; }


        /// <summary>
        /// the current speech tuple while asking for parameters
        /// </summary>
        public SpeechStream CurrentSpeechStream { get; set; }

        /// <summary>
        /// the current parameter
        /// </summary>
        public SpeechParameterStream CurrentParameter { get; set; }



        private string _currentInputCulture;
        private CultureInfo _currentInputCultureInfo;

        /// <summary>
        /// gets or sets the input culture
        /// </summary>
        public string CurrentInputCulture
        {
            get { return _currentInputCulture; }
            set
            {
                try
                {
                    _currentInputCultureInfo = CultureInfo.GetCultureInfo(value);
                }
                catch (Exception e)
                {
                    throw e;
                }

                _currentInputCulture = value;
            }
        }


        /// <summary>
        /// creates a new SpeechHelper with the current culture as input culture
        /// </summary>
        public SpeechHelper()
        {

            this.AllCommands = new SpeechDictionary();
            this.State = ExecutingState.NoCommandsLoaded;
            _OnBeforeSimpleCommandInvoked = new Subject<BeforeSimpleCommandInvokedArgs>();
            _OnAfterSimpleCommandInvoked = new Subject<AfterSimpleComamndInokedArgs>();
            _OnAudioLevelUpdated = new Subject<int>();
            _OnTextRecognized = new Subject<SpeechRecognitionArgs>();
            _OnListeningForParameters = new Subject<SpeechStream>();
            _OnParameterRecognized = new Subject<SpeechParameterStream>();
            _OnParameterFinished = new Subject<SpeechParameterStream>();
            _OnBeforeMethodInvoked = new Subject<SpeechTupleArgs>();
            _OnAfterMethodInvoked = new Subject<SpeechTuple>();
            _OnLastParameterFinished = new Subject<SpeechStream>();

            //set current culture if not set
            if (_currentInputCultureInfo == null)
                this.CurrentInputCulture = CultureInfo.CurrentCulture.Name.ToLower();


            //set up speaker
            _speaker = new SpeechSynthesizer();
            this.SetVoice(VoiceGender.Female, CultureInfo.CurrentCulture);

            //set up listener
            SetInputCulture(_currentInputCultureInfo);

        }


        /// <summary>
        /// creates a new SpeechHelper with the given culture as input culture
        /// </summary>
        /// <param name="inputCulture">the input culture</param>
        public SpeechHelper(string inputCulture)
            : this()
        {
            this.CurrentInputCulture = inputCulture;
        }



        #region gather commands and converters

        /// <summary>
        /// gathers all parameter converter methods in the types the containing type must have the SpeechEnabledAttribute and the methods the SpeechParameterConverterAttribute
        /// </summary>
        /// <param name="targetAssembly">the assembly</param>
        /// <param name="onlyPublicVisibleOnes">true: only exported types, false: all types</param>
        public void GatherConverters(Assembly targetAssembly, bool onlyPublicVisibleOnes = true)
        {
            Type[] types = onlyPublicVisibleOnes ? targetAssembly.GetExportedTypes() : targetAssembly.GetTypes();

            var converters = Crawler.CrawlConverterTypes(this.AllCommands.Converters, false, types);

            foreach (var converter in converters)
                this.AllCommands.Converters.Add(converter.Key, converter);

        }

        /// <summary>
        /// gathers all parameter converter methods in the type
        /// </summary>
        /// <param name="type">the type</param>
        /// <param name="invokingInstance">the invoking instance for the converter method</param>
        public void GatherConverters(Type type, object invokingInstance = null)
        {
            var converters = Crawler.CrawlConverterTypes(this.AllCommands.Converters, true, type);

            foreach (var converter in converters)
            {
                converter.InvokingInstance = invokingInstance;
                this.AllCommands.Converters.Add(converter.Key, converter);
            }
        }

        /// <summary>
        /// gets all speech enabled methods out of the assembly
        /// </summary>
        /// <param name="targetAssembly">the assembly</param>
        /// <param name="onlyPublicVisibleOnes">true: only exported types, false: all types</param>
        public void GatherCommands(Assembly targetAssembly, bool onlyPublicVisibleOnes = true)
        {
            Type[] types = onlyPublicVisibleOnes ? targetAssembly.GetExportedTypes() : targetAssembly.GetTypes();

            List<SpeechMethod> commands = Crawler.CrawlTypes(this.AllCommands.Converters, false, types);

            foreach (var speechMethod in commands)
                this.AllCommands.AddMethod(speechMethod, null);

        }

        /// <summary>
        /// gets all speech enabled methods out of the type
        /// <para />
        /// the type doesnt need a SpeechEnabledAttribute here
        /// </summary>
        /// <param name="type">the type to crawl</param>
        /// <param name="invokingInstance">the instance to invoke the speech methods or null to create one with the parameterless constructor (
        /// static methods doesnt need a invoking instance so leave this null)</param>
        public void GatherCommands(Type type, object invokingInstance = null)
        {
            List<SpeechMethod> commands = Crawler.CrawlTypes(this.AllCommands.Converters, true, type);

            foreach (var speechMethod in commands)
                this.AllCommands.AddMethod(speechMethod, invokingInstance);
        }

        /// <summary>
        /// crawls the given method (attributes still needed) and adds it to the speech dictionary 
        /// </summary>
        /// <param name="methodInfo">the method to crawl</param>
        /// <param name="invokingInstance">the instance to invoke the speech methods or null to create one with the parameterless constructor (
        /// static methods doesnt need a invoking instance so leave this null)</param>
        /// <param name="reloadGrammarIfNeeded">true: automatically reload the grammar, false: user need to call RebuildAllCommands</param>
        public void AddCommand(MethodInfo methodInfo, object invokingInstance = null, bool reloadGrammarIfNeeded = false)
        {
            var commands = new List<SpeechMethod>();
            Crawler.CrawMethods(methodInfo.DeclaringType, commands, this.AllCommands.Converters, methodInfo);

            bool newGrammar = false;
            foreach (var speechMethod in commands)
            {
                this.AllCommands.AddMethod(speechMethod, invokingInstance);

                if (speechMethod.Lang == this._currentInputCulture)
                {
                    foreach (var speechName in speechMethod.SpeechNames)
                    {
                        _currentCommandChoices.Add(speechName);
                        newGrammar = true;
                    }
                }
            }

            if (newGrammar && reloadGrammarIfNeeded)
                ExtendCurrentGrammar(this._currentCommandChoices, ref this.CommandGrammar);


        }


        /// <summary>
        /// adds a plain phrase to look for
        /// </summary>
        /// <param name="reloadGrammar">true: automatically reload the grammar, false: user need to call RebuildAllCommands</param>
        /// /// <param name="phrases">the phrases to remove</param>
        public void AddPlainPhrase(bool reloadGrammar = false, params string[] phrases)
        {
            foreach (var phrase in phrases)
                this.AllCommands.PlainPhrases.Add(phrase);

            if (reloadGrammar)
                this.RebuildPlainPhrases();
        }

        /// <summary>
        /// removes all given phrases from the dictionary
        /// </summary>
        /// <param name="reloadGrammar">true: automatically reload the grammar, false: user need to call RebuildAllCommands</param>
        /// <param name="phrases">the phrases to remove</param>
        public void RemovePlainPhrases(bool reloadGrammar = false, params string[] phrases)
        {
            foreach (var phrase in phrases)
                this.AllCommands.PlainPhrases.Remove(phrase);
            
            if (reloadGrammar)
                this.RebuildPlainPhrases();
        }

        /// <summary>
        /// adds a simple method to execute (no befor + after invoked events are fired for this methods)
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the phrase unique for the language</param>
        /// <param name="simpleSpeechGroupKey">the simple speech group tuple</param>
        /// <param name="action">the action to perfrom when the phrase is recognized</param>
        public void AddSimpleCommand(string lang, string text, string simpleSpeechGroupKey, Action action)
        {
            this.AllCommands.AddSimpleCommand(lang, text, simpleSpeechGroupKey, action);
        }

        #endregion


        #region speech helping region

        #region text to speech (output)

        /// <summary>
        /// sets the volume of the voice
        /// </summary>
        /// <param name="volume">the volume between 0 and 100</param>
        public void SetVoiceVolume(int volume)
        {
            _speaker.Volume = 100;
        }

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

        /// <summary>
        /// outputs the given text with the current voice async
        /// </summary>
        /// <param name="text">the text to output</param>
        public void SpeakAsync(string text)
        {
            _speaker.SpeakAsync(text);
        }

        /// <summary>
        /// cancels all async output
        /// </summary>
        public void CancelAllSpeakAsync()
        {
            _speaker.SpeakAsyncCancelAll();
        }

        #endregion


        #region speech to 'text' (command) (input)

        #region method execution

        /// <summary>
        /// executes the method and fires all events
        /// </summary>
        /// <param name="command">the command to execute</param>
        /// <param name="instance">the instance to execute the method on</param>
        /// <param name="args">the args for the method</param>
        private void ExecuteMethod(SpeechTuple command, object instance, params object[] args)
        {
            var token = new SpeechTupleArgs(command);
            this._OnBeforeMethodInvoked.OnNext(token);

            if (token.CancelCommand == false)
            {
                command.Method.MethodInfo.Invoke(instance, args); //TODO maybe wrap this method in try and error raise event...

                this._OnAfterMethodInvoked.OnNext(command);
            }
        }

        /// <summary>
        /// executes a method and sets the state to ListeningForParameters if the method requires parameters
        /// </summary>
        /// <param name="speechTuple">the tuple with all infomration</param>
        /// <param name="recognizedMethodText"></param>
        private void TryExecuteMethod(SpeechTuple speechTuple, string recognizedMethodText)
        {
            if (speechTuple.IsEnabled) //first check if the command is enabled
            {
                if (speechTuple.Method.MethodInfo.IsStatic == false)
                    if (speechTuple.InvokeInstance == null)
                    {
                        var obj = Activator.CreateInstance(speechTuple.Method.ExecutingType);
                        speechTuple.InvokeInstance = obj;
                    }

                if (speechTuple.Method.Arguments.Count == 0)
                {
                    //when the method is static speechTuple.InvokeInstance is already null
                    ExecuteMethod(speechTuple, speechTuple.InvokeInstance, null);

                }
                else
                {
                    this.State = ExecutingState.ListeningForParameters;
                    this.CurrentSpeechStream = new SpeechStream(recognizedMethodText, speechTuple);
                    _OnListeningForParameters.OnNext(CurrentSpeechStream);
                }
            }
        }



        /// <summary>
        /// aborts listening for the current parameter value and sets the state to ListeningForParameters
        /// </summary>
        public void AbortListeningForCurrentParameterValue()
        {
            this.CurrentParameter = null;
            this.State = ExecutingState.ListeningForParameters;
        }

        /// <summary>
        /// aborts listening for parameters 
        /// <para />
        /// if ListeningForParameters was the current state and then the state is set to ListeningForMethods
        /// </summary>
        public void AbortListeningForParameters()
        {
            this.CurrentParameter = null;
            this.CurrentSpeechStream = null;

            if (this.State == ExecutingState.ListeningForParameters)
                this.State = ExecutingState.ListeningForMethods;

        }

        /// <summary>
        /// we have a method and now we got text for a parameter name -> get the right parameter and set it as the current parameter (we still need a value)
        /// </summary>
        /// <param name="parameterSpeechText">the new current parameter name</param>
        private bool SetCurrentParameter(string parameterSpeechText)
        {
            //get the real parameter out of the method with the parameterSpeechText

            foreach (SpeechParameter speechParameter in this.CurrentSpeechStream.SpeechTuple.Method.Arguments)
            {
                if (speechParameter.SpeechNames.Contains(parameterSpeechText))
                {
                    this.CurrentParameter = new SpeechParameterStream(parameterSpeechText, this.CurrentSpeechStream.SpeechTuple, new SpeechParameterInfo(speechParameter));

                    SpeechParameterStream copy = null;
                    //1. look if there is already an old value for this parameter
                    //2. check if the parameter value is empty in this case the user just switched to another parameter
                    for (int i = 0; i < this.CurrentSpeechStream.SpeechParameterStreams.Count; i++)
                    {
                        var speechParameterStream = this.CurrentSpeechStream.SpeechParameterStreams[i];

                        //check if there is a speech parameter info
                        if (speechParameterStream.SpeechParameterInfo.Parameter.ParameterInfo.Name == speechParameter.ParameterInfo.Name)
                        {
                            copy = speechParameterStream;
                        }
                        if (speechParameterStream.SpeechParameterInfo.Value == null)
                        {
                            this.CurrentSpeechStream.SpeechParameterStreams.RemoveAt(i);
                            i--;
                        }
                    }

                    if (copy != null)
                        this.CurrentSpeechStream.SpeechParameterStreams.Remove(copy);

                    this.CurrentSpeechStream.SpeechParameterStreams.Add(this.CurrentParameter);


                    this.State = ExecutingState.ListeningForParameterValue;
                    this._OnParameterRecognized.OnNext(this.CurrentParameter);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// finishes the current parameter (assigns value) and if this was the last param then executes the current method
        /// </summary>
        /// <param name="valueText"></param>
        private bool FinishCurrentParameter(string valueText)
        {

            if (this.State == ExecutingState.ListeningForParameters)
            {
                //check the first left parameter
                //parameter name was left out

                int leftParameterIndex = -1;

                for (int i = 0; i < this.CurrentSpeechStream.SpeechTuple.Method.Arguments.Count; i++)
                {
                    var arg = this.CurrentSpeechStream.SpeechTuple.Method.Arguments[i];
                    if (
                        this.CurrentSpeechStream.SpeechParameterStreams.Any(
                            p => p.SpeechParameterInfo.Parameter.ParameterInfo.Name == arg.ParameterInfo.Name) == false)
                    {
                        leftParameterIndex = i;
                        break;
                    }
                }

                if (leftParameterIndex != -1)
                    this.CurrentParameter = new SpeechParameterStream("", this.CurrentSpeechStream.SpeechTuple,
                        new SpeechParameterInfo(this.CurrentSpeechStream.SpeechTuple.Method.Arguments[leftParameterIndex]));

            }

            SpeechParameterConverter converter = this.CurrentParameter.SpeechParameterInfo.Parameter.Converter;


            object erg = null;//OnParameterValueConvert(this.CurrentParameter, valueText);

            if (converter == null)
            {
                throw new Exception("converter was not set so the parameter value: " + valueText + " for parameter " +
                this.CurrentParameter.RecognizedParameterNameText + "(real name: " + this.CurrentParameter.SpeechParameterInfo.Parameter.ParameterInfo.Name +
                ") couldnt be converted");
            }
            else
            {

                if (converter.MethodInfo.IsStatic == false && converter.InvokingInstance == null)
                {
                    try
                    {
                        converter.InvokingInstance = Activator.CreateInstance(converter.ExecutingType);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("tried to create an instance of " + converter.ExecutingType.FullName + " in order to convert the parameter " +
                        this.CurrentParameter.RecognizedParameterNameText + "(real name: " + this.CurrentParameter.SpeechParameterInfo.Parameter.ParameterInfo.Name + ")" +
                        "\nException: " + ex.Message);
                    }

                }

                erg = converter.MethodInfo.Invoke(converter.InvokingInstance, new object[] { valueText });
            }


            if (erg != null)
            {
                this.CurrentParameter.SpeechParameterInfo.Value = erg;


                if (this.State == ExecutingState.ListeningForParameters) //only add if the parameter name was left out
                {
                    this.CurrentSpeechStream.SpeechParameterStreams.Add(this.CurrentParameter);
                }

                this._OnParameterFinished.OnNext(this.CurrentParameter);
            }
            else
                return false;



            //now check if this was the last parameter...
            if (this.CurrentSpeechStream.SpeechParameterStreams.Count == this.CurrentSpeechStream.SpeechTuple.Method.Arguments.Count)
            {
                //we have all parameters...
                if (this.CurrentSpeechStream.SpeechTuple.Method.MethodInfo.IsStatic == false)
                {
                    if (this.CurrentSpeechStream.SpeechTuple.InvokeInstance == null)
                    {
                        var obj = Activator.CreateInstance(this.CurrentSpeechStream.SpeechTuple.Method.ExecutingType);
                        this.CurrentSpeechStream.SpeechTuple.InvokeInstance = obj;
                    }
                }

                //reorder parameters to fit signature
                var values = new List<object>();
                foreach (var speechParameter in this.CurrentSpeechStream.SpeechTuple.Method.Arguments)
                {
                    SpeechParameterStream speechParameterInfo =
                        this.CurrentSpeechStream.SpeechParameterStreams.FirstOrDefault(
                            p => p.SpeechParameterInfo.Parameter.ParameterInfo.Name == speechParameter.ParameterInfo.Name);

                    if (speechParameterInfo != null)
                        values.Add(speechParameterInfo.SpeechParameterInfo.Value);
                    else
                    {
                        //TODO parameter has no value or parameter info ??
                    }
                }
                this._OnLastParameterFinished.OnNext(this.CurrentSpeechStream);

                //finally execute the method with the right parameter order
                ExecuteMethod(this.CurrentSpeechStream.SpeechTuple, this.CurrentSpeechStream.SpeechTuple.InvokeInstance, values.ToArray());

                this.AbortListeningForParameters();
                this.State = ExecutingState.ListeningForMethods;

            }
            else
            {
                this.State = ExecutingState.ListeningForParameters;
            }

            return true;

        }

        /// <summary>
        /// pretends the input of the given text no originalArgs (SpeechRecognizedEventArgs) are provided
        /// </summary>
        /// <param name="input"></param>
        public void PretendInput(string input)
        {
            OnRecognizedText(null, input);
        }

        private void EngineOnSpeechRecognized(object sender, SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            OnRecognizedText(speechRecognizedEventArgs);
        }


        private void OnRecognizedText(SpeechRecognizedEventArgs speechRecognizedEventArgs, string text = null)
        {
            if (speechRecognizedEventArgs == null && text == null)
                throw new Exception("#1 unknown");


            var token = new SpeechRecognitionArgs(text ?? speechRecognizedEventArgs.Result.Text, speechRecognizedEventArgs);

            if (this.State != ExecutingState.NotListening)
            {
                this._OnTextRecognized.OnNext(token);

                //first look for simple methods could contain some abort methods... through this.State
                if (LookForSimpleCommands(token.Text))
                    return;


                if (this.State == ExecutingState.ListeningForParameters)
                {
                    if (SetCurrentParameter(token.Text) == false)
                        FinishCurrentParameter(token.Text); //maybe we left the parameter name out

                }
                else if (this.State == ExecutingState.ListeningForParameterValue)
                {

                    if (FinishCurrentParameter(token.Text) == false)
                        SetCurrentParameter(token.Text); //user can change the current parameter

                }
                else
                {
                    if (token.CancelFurtherCommands == false)
                        LookForCommands(token.Text);
                }
            }
        }

        /// <summary>
        /// executes simple commands if any matches the text
        /// </summary>
        /// <param name="text">the command text to look for</param>
        /// <returns>true: command found, false: not found or disabled</returns>
        private bool LookForSimpleCommands(string text)
        {
            Dictionary<string, SimpleSpeechGroupTuple> simpleCommands;

            if (this.AllCommands.SimpleCommands.TryGetValue(this._currentInputCulture, out simpleCommands))
            {
                if (string.IsNullOrEmpty(this.CurrentSimpleSpeechGroupKey))
                {
                    //search all for a matching text
                    foreach (var simpleSpeechGroupTuple in simpleCommands)
                    {
                        SimpleCommandTuple cmd;
                        if (simpleSpeechGroupTuple.Value.Commands.TryGetValue(text, out cmd))
                        {
                            if (cmd.IsEnabled)
                            {

                                ExecuteSimpleCommand(this._currentInputCulture, text, this.CurrentSimpleSpeechGroupKey,
                                    cmd.Action);

                                return true;
                            }
                        }
                    }
                }
                else
                {
                    //just search the current simple speech group
                    SimpleSpeechGroupTuple simpleSpeechGroupTuple;
                    if (simpleCommands.TryGetValue(this.CurrentSimpleSpeechGroupKey, out simpleSpeechGroupTuple))
                    {
                        if (simpleSpeechGroupTuple.IsEnabled)
                        {
                            SimpleCommandTuple cmd;
                            if (simpleSpeechGroupTuple.Commands.TryGetValue(text, out cmd))
                            {
                                if (cmd.IsEnabled)
                                {
                                    ExecuteSimpleCommand(this._currentInputCulture, text, this.CurrentSimpleSpeechGroupKey,
                                    cmd.Action);

                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// executes a simple command
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the simple command text</param>
        /// <param name="speechGroupKey">the speech group key</param>
        /// <param name="action">the action to execute</param>
        private void ExecuteSimpleCommand(string lang, string text, string speechGroupKey, Action action)
        {
            var beforeArgs = new BeforeSimpleCommandInvokedArgs(lang, text, speechGroupKey, action, false);
            this._OnBeforeSimpleCommandInvoked.OnNext(beforeArgs);

            if (beforeArgs.IsCanceled == false)
            {
                action();

                var afterArgs = new AfterSimpleComamndInokedArgs(lang, text, speechGroupKey, action);
                this._OnAfterSimpleCommandInvoked.OnNext(afterArgs);
            }
        }

        /// <summary>
        /// looks for commands with the speech name
        /// </summary>
        /// <param name="text">the speech name</param>
        private void LookForCommands(string text)
        {
            //look for commands
            Dictionary<string, SpeechGroupTuple> speechGroups;

            if (this.AllCommands.Commands.TryGetValue(this._currentInputCulture, out speechGroups))
            {

                if (string.IsNullOrEmpty(CurrentSpeechGroupKey) == false)
                {
                    //only search in the current speech group
                    SpeechGroupTuple speechGroup;
                    if (speechGroups.TryGetValue(CurrentSpeechGroupKey, out speechGroup))
                    {
                        if (speechGroup.IsEnabled)
                        {
                            SpeechTuple speechTuple;
                            if (speechGroup.Commands.TryGetValue(text, out speechTuple))
                            {
                                if (speechTuple.IsEnabled)
                                    TryExecuteMethod(speechTuple, text);
                            }
                        }
                    }
                }
                else
                {
                    //search in every enabled speech group
                    foreach (var speechGroupTuple in speechGroups)
                    {
                        if (speechGroupTuple.Value.IsEnabled)
                        {
                            SpeechTuple speechTuple;
                            if (speechGroupTuple.Value.Commands.TryGetValue(text, out speechTuple))
                            {
                                if (speechTuple.IsEnabled)
                                {
                                    TryExecuteMethod(speechTuple, text);
                                    return; //execut only the first found speech group
                                }
                            }
                        }
                    }
                }

            }
        }

        #endregion


        /// <summary>
        /// rebuilds all plain phrases
        /// </summary>
        public void RebuildPlainPhrases()
        {
            var choices = new Choices();

            for (int i = this.NumbersFrom; i <= this.NumbersTo; i++)
                this.AllCommands.PlainPhrases.Add(i.ToString());


            //first add plain phrases
            foreach (var plainPhrase in this.AllCommands.PlainPhrases)
                choices.Add(plainPhrase);

            this.ExtendCurrentGrammar(choices, ref this.PlainPhraseGrammar);

        }

        /// <summary>
        /// rebuilds all simple commands
        /// </summary>
        public void RebuildSimpleCommands()
        {
            var choices = new Choices();

            Dictionary<string, SimpleSpeechGroupTuple> simpleSpeechGroup;
            if (this.AllCommands.SimpleCommands.TryGetValue(this._currentInputCulture, out simpleSpeechGroup))
            {
                foreach (var simpleSpeechGroupTuple in simpleSpeechGroup)//simpleSpeechGroupTuple.key is the simple speech group key
                    foreach (var simpleCommand in simpleSpeechGroupTuple.Value.Commands)
                        choices.Add(simpleCommand.Key);
            }

            this.ExtendCurrentGrammar(choices, ref this.SimpleCommandGrammar);
        }

        /// <summary>
        /// rebuild all commands
        /// </summary>
        public void RebuildCommands()
        {

            var choices = new Choices();

            Dictionary<string, SpeechGroupTuple> langCommands;

            var knownParameters = new HashSet<string>();

            if (this.AllCommands.Commands.TryGetValue(this._currentInputCulture, out langCommands))
            {
                foreach (var speechNamePair in langCommands)
                {
                    //[1. speech group name, 2. [1. method speech name, 2. (method)]]

                    foreach (var speechTuple in speechNamePair.Value.Commands)
                    {
                        //add because synonyms has different speech names
                        choices.Add(speechTuple.Key);

                        //add all parameters

                        //add every parameter of the method
                        foreach (SpeechParameter publicSpeechArgument in speechTuple.Value.Method.Arguments)
                            foreach (var paramSpeechName in publicSpeechArgument.SpeechNames) //parameter could have synonyms
                            {
                                if (knownParameters.Add(paramSpeechName)) // true = added
                                    choices.Add(paramSpeechName);
                            }
                    }
                }

                this._currentCommandChoices = choices;
                ExtendCurrentGrammar(choices, ref this.CommandGrammar);
            }
        }

        /// <summary>
        /// rebuilds the commands with the current culture
        /// </summary>
        public void RebuildAllCommands()
        {

            //just add some common numbers
            RebuildPlainPhrases();

            RebuildSimpleCommands();

            RebuildCommands();
        }


        /// <summary>
        /// unloads the old grammar replaces all choices and loads the new grammar with the given choices
        /// </summary>
        /// <param name="choices">the new choices</param>
        /// <param name="oldGrammar">the old grammar</param>
        private void ExtendCurrentGrammar(Choices choices, ref Grammar oldGrammar) //ref
        {
            //foreach (var phrase in phrases)
            //    this._currentChoices.Add(phrase);

            var beforeState = this.State;
            this.StopListening();
            //_engine.SpeechRecognized -= EngineOnSpeechRecognized;
            //var grammar = new Grammar(_currentChoices);

            var newGrammar = new Grammar(choices);

            try
            {
                if (oldGrammar != null)
                    _engine.UnloadGrammar(oldGrammar);

                _engine.LoadGrammar(newGrammar);
            }
            catch (Exception ex)
            {
                throw;
            }

            oldGrammar = newGrammar;


            //_engine.SpeechRecognized += EngineOnSpeechRecognized;
            if (beforeState == ExecutingState.ListeningForMethods || beforeState == ExecutingState.ListeningForParameters)
            {
                this.StartListening();
                this.State = beforeState;
            }
        }

        #region change commands, speech groups

        /// <summary>
        /// enables or disables all simple command with this text
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the text</param>
        /// <param name="isEnabled">true: command is processed, false: not</param>
        public bool ChangeSimpleCommand(string lang, string text, bool isEnabled)
        {
            return this.AllCommands.ChangeSimpleCommand(lang, text, isEnabled);
        }

        /// <summary>
        /// enables or disables a simple speech group
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="simpleSpeechGroupKey">the simple speech group key</param>
        /// <param name="isEnabled">true: command is processed, false: not</param>
        public bool ChangeSimpleSpeechGroup(string lang, string simpleSpeechGroupKey, bool isEnabled)
        {
            return this.AllCommands.ChangeSimpleSpeechGroup(lang, simpleSpeechGroupKey, isEnabled);
        }

        /// <summary>
        /// enables or disables all command with this key
        /// </summary>
        /// <param name="key">the method key</param>
        /// <param name="isEnabled">true: command will be processed, false: not</param>
        public bool ChangeCommand(string key, bool isEnabled)
        {
            return this.AllCommands.ChangeCommand(key, isEnabled);
        }

        /// <summary>
        /// enables or disables a speech group
        /// </summary>
        /// <param name="groupKey">the group key</param>
        /// <param name="isEnabled">true: commands will be processed, false: not</param>
        public bool ChangeSpeechGroup(string groupKey, bool isEnabled)
        {
            return this.AllCommands.ChangeSpeechGroup(groupKey, isEnabled);
        }

        #endregion

        /*
        private Grammar CreateColorGrammar()
        {
            //TODO maybe use different grammars for the parameters e.g. set x to {1,2,3,4,5...}
         * 
            // Create a Choices object that contains a set of alternative colors.
            Choices colorChoice = new Choices();
            colorChoice.Add(new string[] { "rot", "gelb", "grün" });

            // Construct the phrase.
            GrammarBuilder builder = new GrammarBuilder("Hintergrund");
            builder.Append(colorChoice);

            // Create a grammar for the phrase.
            Grammar colorGrammar = new Grammar(builder);
            colorGrammar.Name = "SetBackground";

            return colorGrammar;
        }
        */


        /// <summary>
        /// sets the culture for the speech to text engine (input)
        /// </summary>
        /// <param name="cultureInfo"></param>
        public void SetInputCulture(CultureInfo cultureInfo)
        {
            _engine = new SpeechRecognitionEngine(cultureInfo);
            _engine.SetInputToDefaultAudioDevice();
            _engine.SpeechRecognized += EngineOnSpeechRecognized;
            _engine.AudioLevelUpdated += EngineOnAudioLevelUpdated;
        }

        private void EngineOnAudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs audioLevelUpdatedEventArgs)
        {
            _OnAudioLevelUpdated.OnNext(audioLevelUpdatedEventArgs.AudioLevel);
        }


        /// <summary>
        /// starts listening for commands
        /// </summary>
        public void StartListening()
        {
            _engine.RecognizeAsync(RecognizeMode.Multiple);
            State = ExecutingState.ListeningForMethods;
        }

        /// <summary>
        /// stops listening for commands
        /// </summary>
        public void StopListening()
        {
            _engine.RecognizeAsyncStop();
            State = ExecutingState.NotListening;
        }



        #endregion




        #endregion


        public void Dispose()
        {
            this.StopListening();
            this.CancelAllSpeakAsync();
            _engine.Dispose();
            _speaker.Dispose();
            this.AllCommands = null;
            _currentInputCulture = "";
        }

    }
}

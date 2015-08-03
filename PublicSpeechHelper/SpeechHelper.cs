﻿using System;
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
    public class  SpeechHelper
    {

        public int NumbersFrom = 0;
        public int NumbersTo = 20;

        #region events

        private readonly Subject<SpeechRecognitionArgs> _OnTextRecognized;
        private readonly Subject<SpeechTuple> _OnListeningForParameters;

        private readonly Subject<SpeechParameterStream> _OnParameterRecognized;
        private readonly Subject<SpeechParameterStream> _OnParameterFinished;
        private readonly Subject<SpeechStream> _OnLastParameterFinished;

        private readonly Subject<SpeechTupleArgs> _OnBeforeMethodInvoked;
        private readonly Subject<SpeechTuple> _OnAfterMethodInvoked;

        public Func<SpeechRecognitionConflictArgs, SpeechTuple> OnResolveCommandConflicFunc;


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
        public IObservable<SpeechTuple> OnListeningParameters
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


        private SpeechSynthesizer _speaker;
        private SpeechRecognitionEngine _engine;


        private Choices _currentChoices;

        public SpeechDictionary AllCommands { get; set; }

        /// <summary>
        /// the current state of the helper
        /// </summary>
        public ExecutingState State { get; set; }


        /// <summary>
        /// the current speech tuple while asking for parameters
        /// </summary>
        public SpeechStream CurrentSpeechStream { get; set; }

        public List<SpeechParameterStream> CurrentParameters { get; set; }

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
            this.CurrentParameters = new List<SpeechParameterStream>();
            this.AllCommands = new SpeechDictionary();
            this.State = ExecutingState.NoCommandsLoaded;
            _OnTextRecognized = new Subject<SpeechRecognitionArgs>();
            _OnListeningForParameters = new Subject<SpeechTuple>();
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



        #region gather commands

        /// <summary>
        /// gets all speech enabled methods out of the assembly
        /// </summary>
        /// <param name="targetAssembly">the assembly</param>
        /// <param name="onlyPublicVisibleOnes">true: only exported types, false: all types</param>
        public void GatherCommands(Assembly targetAssembly, bool onlyPublicVisibleOnes = true)
        {
            Type[] types = onlyPublicVisibleOnes ? targetAssembly.GetExportedTypes() : targetAssembly.GetTypes();

            List<SpeechMethod> commands = Crawler.CrawlTypes(false, types);

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
            List<SpeechMethod> commands = Crawler.CrawlTypes(true, type);

            foreach (var speechMethod in commands)
                this.AllCommands.AddMethod(speechMethod, invokingInstance);
        }

        /// <summary>
        /// crawls the given method (attributes still needed) and adds it the 
        /// </summary>
        /// <param name="methodInfo">the method to crawl</param>
        /// <param name="invokingInstance">the instance to invoke the speech methods or null to create one with the parameterless constructor (
        /// static methods doesnt need a invoking instance so leave this null)</param>
        /// <param name="reloadGrammarIfNeeded">true: automatically reload the grammar, false: user need to call RebuildAllCommands</param>
        public void AddCommand(MethodInfo methodInfo, object invokingInstance = null, bool reloadGrammarIfNeeded = true)
        {
            var commands = new List<SpeechMethod>();
            Crawler.CrawMethods(methodInfo.DeclaringType, commands, methodInfo);

            bool newGrammar = false;
            foreach (var speechMethod in commands)
            {
                this.AllCommands.AddMethod(speechMethod, invokingInstance);

                if (speechMethod.Lang == this._currentInputCulture)
                {
                    foreach (var speechName in speechMethod.SpeechNames)
                    {
                        _currentChoices.Add(speechName);
                        newGrammar = true;
                    }
                }
            }

            if (newGrammar && reloadGrammarIfNeeded)
                ExtendCurrentGrammar(new string[] { });


        }


        /// <summary>
        /// registers a plain phrase to look for
        /// </summary>
        /// <param name="phrase">the phrase</param>
        /// <param name="reloadGrammar">true: automatically reload the grammar, false: user need to call RebuildAllCommands</param>
        /// <returns>true: phrase added, false: phrase was already there</returns>
        public bool RegisterPlainPhrase(string phrase, bool reloadGrammar = true)
        {
            var test = this.AllCommands.PlainPhrases.Add(phrase);

            if (reloadGrammar)
                ExtendCurrentGrammar(phrase);

            return test;
        }



        #endregion


        #region speech helping region

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


        #region speech to text (input)

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
                command.Method.MethodInfo.Invoke(instance, args);

                this._OnAfterMethodInvoked.OnNext(command);
            }
        }

        /// <summary>
        /// executes a method and sets the state to ListeningForParameters if the method requires parameters
        /// </summary>
        /// <param name="speechTuple">the tuple with all infomration</param>
        private void TryExecuteMethod(SpeechTuple speechTuple, string text)
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
                this.CurrentSpeechStream = new SpeechStream(text, speechTuple);
                _OnListeningForParameters.OnNext(speechTuple);
            }
        }


        /// <summary>
        /// we have a method and now we got text for a parameter name -> get the right parameter and set it as the current parameter (we still need a value)
        /// </summary>
        /// <param name="parameterSpeechText"></param>
        private void SetCurrentParameter(string parameterSpeechText)
        {
            //get the real parameter out of the method with the parameterSpeechText

            foreach (SpeechParameter speechParameter in this.CurrentSpeechStream.SpeechTuple.Method.Arguments)
            {
                if (speechParameter.SpeechNames.Contains(parameterSpeechText))
                {
                    this.CurrentParameter = new SpeechParameterStream(parameterSpeechText, this.CurrentSpeechStream.SpeechTuple, new SpeechParameterInfo(speechParameter));
                    this.CurrentSpeechStream.SpeechParameterStreams.Add(this.CurrentParameter);
                    this.State = ExecutingState.ListeningForParameterValue;
                    this._OnParameterRecognized.OnNext(this.CurrentParameter);
                    return;
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
        /// aborts listening for parameters if that was the current state and sets the state to ListeningForMethods
        /// </summary>
        public void AbortListeningForParameters()
        {
            this.CurrentParameter = null;
            this.CurrentParameters.Clear();

            if (this.State == ExecutingState.ListeningForParameters)
                this.State = ExecutingState.ListeningForMethods;

        }

        /// <summary>
        /// finishes the current parameter (assigns value) and if this was the last param then executes the current method
        /// </summary>
        /// <param name="valueText"></param>
        private void FinishCurrentParameter(string valueText)
        {

            int i;
            if (int.TryParse(valueText, out i))
            {
                this.CurrentParameter.SpeechParameterInfo.Value = i;
                this.CurrentParameters.Add(this.CurrentParameter);
                this._OnParameterFinished.OnNext(this.CurrentParameter);
            }


            //now check if this was the last parameter...
            if (this.CurrentParameters.Count == this.CurrentSpeechStream.SpeechTuple.Method.Arguments.Count)
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
                        this.CurrentParameters.FirstOrDefault(
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

        }

        private void EngineOnSpeechRecognized(object sender, SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            var token = new SpeechRecognitionArgs(speechRecognizedEventArgs.Result.Text, speechRecognizedEventArgs);
            this._OnTextRecognized.OnNext(token);

            //first look for simple methods could contain some abort methods... through this.State
            LookForSimpleCommands(token.Text);


            if (this.State == ExecutingState.ListeningForParameters)
            {
                //this is actually a parameter ...
                SetCurrentParameter(token.Text);
            }
            else if (this.State == ExecutingState.ListeningForParameterValue)
            {
                FinishCurrentParameter(token.Text);
            }
            else
            {
                if (token.CancelFurtherCommands == false)
                {
                    //TODO check for disabled methods
                    LookForCommands(token.Text);
                }

            }



        }

        #endregion


        /// <summary>
        /// executes simple commands if any matches the text
        /// </summary>
        /// <param name="text"></param>
        private void LookForSimpleCommands(string text)
        {
            Dictionary<string, Action> simpleCommands;

            if (this.AllCommands.SimpleMethodPhrases.TryGetValue(this._currentInputCulture, out simpleCommands))
            {
                Action action;
                if (simpleCommands.TryGetValue(text, out action))
                    action();
            }
        }

        /// <summary>
        /// looks for commands with the speech name
        /// </summary>
        /// <param name="text">the speech name</param>
        private void LookForCommands(string text)
        {
            //look for commands
            Dictionary<string, Dictionary<string, SpeechTuple>> langCommands;

            if (this.AllCommands.Commands.TryGetValue(this._currentInputCulture, out langCommands))
            {

                Dictionary<string, SpeechTuple> homophoneCommands;
                if (langCommands.TryGetValue(text, out homophoneCommands))
                {
                    var enabledSpeechTuples = new List<SpeechTuple>();
                    foreach (var homophoneCommand in homophoneCommands)
                    {
                        if (homophoneCommand.Value.IsEnabled)
                            enabledSpeechTuples.Add(homophoneCommand.Value);
                    }

                    if (enabledSpeechTuples.Count == 1)
                    {
                        TryExecuteMethod(enabledSpeechTuples[0], text);
                    }
                    else
                    {
                        //let the use decide
                        if (this.OnResolveCommandConflicFunc != null)
                        {
                            var resolvedSpeechTuple = this.OnResolveCommandConflicFunc(new SpeechRecognitionConflictArgs(text, enabledSpeechTuples.ToArray()));
                            TryExecuteMethod(resolvedSpeechTuple, text);
                        }
                        else
                        {
                            //TODO maybe add some standard handling
                        }
                    }
                }
            }
        }

        /// <summary>
        /// adds a simple method to execute (no befor & after invoked events are fired for this methods)
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the phrase unique for the language</param>
        /// <param name="action">the action to perfrom when the phrase is recognized</param>
        public void AddSimpleCommand(string lang, string text, Action action)
        {
            this.AllCommands.AddSimpleCommand(lang, text, action);
        }

        /// <summary>
        /// reloads the current grammar
        /// </summary>
        /// <param name="phrases">the phrases to add</param>
        private void ExtendCurrentGrammar(params string[] phrases)
        {
            foreach (var phrase in phrases)
                this._currentChoices.Add(phrase);


            var beforeState = this.State;
            this.StopListening();
            //_engine.SpeechRecognized -= EngineOnSpeechRecognized;
            var grammar = new Grammar(_currentChoices);
            _engine.LoadGrammar(grammar);
            //_engine.SpeechRecognized += EngineOnSpeechRecognized;
            if (beforeState == ExecutingState.ListeningForMethods || beforeState == ExecutingState.ListeningForParameters)
            {
                this.StartListening();
                this.State = beforeState;
            }
        }

        /// <summary>
        /// rebuilds the commands with the current culture
        /// </summary>
        /// <returns>true: ok, false: no commands for the current language found</returns>
        public bool RebuildAllCommands()
        {

            //just add some common numbers
            var choices = new Choices();

            Dictionary<string, Action> simpleMethods;

            if (this.AllCommands.SimpleMethodPhrases.TryGetValue(this._currentInputCulture, out simpleMethods))
                foreach (var speechName in simpleMethods.Keys)
                    choices.Add(speechName);


            for (int i = this.NumbersFrom; i <= this.NumbersTo; i++)
                this.AllCommands.PlainPhrases.Add(i.ToString());

            //first add plain phrases
            foreach (var plainPhrase in this.AllCommands.PlainPhrases)
                choices.Add(plainPhrase);



            //var t = choices0.ToGrammarBuilder();

            //add every method
            //1. language, 2. [1. speech name, 2. [1. method key, 2. (method)]]

            Dictionary<string, Dictionary<string, SpeechTuple>> langCommands;

            if (this.AllCommands.Commands.TryGetValue(this._currentInputCulture, out langCommands))
            {
                foreach (KeyValuePair<string, Dictionary<string, SpeechTuple>> speechNamePair in this.AllCommands.Commands[this._currentInputCulture])
                {
                    //[1. speech name, 2. [1. method key, 2. (method)]]

                    choices.Add(speechNamePair.Key);

                    //add all parameters

                    foreach (KeyValuePair<string, SpeechTuple> speechTuple in speechNamePair.Value)
                    {
                        //speechTuple.Key - the method key
                        //add every parameter of the method
                        foreach (SpeechParameter publicSpeechArgument in speechTuple.Value.Method.Arguments)
                            foreach (var paramSpeechName in publicSpeechArgument.SpeechNames) //parameter could have synonyms
                            {
                                choices.Add(paramSpeechName);
                            }
                    }
                }

                this._currentChoices = choices;
                ExtendCurrentGrammar(new string[] { });
                return true;
            }

            return false;

        }

        private Grammar CreateColorGrammar()
        {
            //TODO remove
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PublicSpeechHelper.SpeechApi;

namespace PublicSpeechHelper.Helpers
{
    /// <summary>
    /// a class to handle speech commands
    /// </summary>
    public class SpeechDictionary
    {
        /// <summary>
        /// all aviable commands
        /// <para />
        /// 1. language, 2. [1. speech name, 2. [1. method key, 2. (method)]]
        /// <para />
        /// e.g. 1. de-de, 2. [1. goto , [(1. goto1, 2. goto item ... 1) ; (1. goto2, 2. goto item ... 2)]]
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, SpeechTuple>>> Commands { get; set; }

        /// <summary>
        /// all aviable speech groups
        /// <para />
        /// 1. language, 2. [1. speech group key, 2. [1. enabled, 2. commands]]
        /// </summary>
        public Dictionary<string, Dictionary<string, SpeechGroupTuple>> SpeechGroups { get; set; }


        public Dictionary<string, Dictionary<string, Action>> SimpleMethodPhrases { get; set; }

        /// <summary>
        /// just some plain phrases to look for
        /// </summary>
        public HashSet<string> PlainPhrases { get; set; }

        /// <summary>
        /// creates a new speech dictionary
        /// </summary>
        public SpeechDictionary()
        {
            Commands = new Dictionary<string, Dictionary<string, Dictionary<string, SpeechTuple>>>();
            SpeechGroups = new Dictionary<string, Dictionary<string, SpeechGroupTuple>>();
            PlainPhrases = new HashSet<string>();
            SimpleMethodPhrases = new Dictionary<string, Dictionary<string, Action>>();
        }


        /// <summary>
        /// adds a simple method to execute
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the phrase a unique text for the language</param>
        /// <param name="action">the action to perfrom when the phrase is recognized</param>
        public void AddSimpleCommand(string lang, string text, Action action)
        {
            if (SimpleMethodPhrases.ContainsKey(lang) == false)
            {
                var simpleCommands = new Dictionary<string, Action>();
                simpleCommands.Add(text, action);
                SimpleMethodPhrases.Add(lang, simpleCommands);
            }
            else
            {
                var methods = SimpleMethodPhrases[lang];
                if (methods.ContainsKey(text) == false)
                {
                    methods.Add(text, action);
                }
                else
                    throw new Exception("speech dictionary already contains a simple method: " + text + " on language: " + lang);
            }
        }

        #region all commands

        /// <summary>
        /// creates a new command
        /// </summary>
        /// <param name="method">the method</param>
        /// <param name="invokeInstance">the instance to invoke the method on (or null for static methods)</param>
        public void AddMethod(SpeechMethod method, object invokeInstance)
        {
            Dictionary<string, Dictionary<string, SpeechTuple>> langCommands;

            //create if not exists
            if (Commands.TryGetValue(method.Lang, out langCommands) == false)
            {
                langCommands = new Dictionary<string, Dictionary<string, SpeechTuple>>();
                Commands.Add(method.Lang, langCommands);
            }



            if (string.IsNullOrEmpty(method.SpeechGroupKey) == false)
            {
                Dictionary<string, SpeechGroupTuple> speechGroup;

                if (SpeechGroups.TryGetValue(method.Lang, out speechGroup) == false)
                {
                    speechGroup = new Dictionary<string, SpeechGroupTuple>();
                    SpeechGroups.Add(method.Lang, speechGroup);
                }

                SpeechGroupTuple speechGroupTuple;
                if (speechGroup.TryGetValue(method.SpeechGroupKey, out speechGroupTuple) == false)
                {
                    speechGroup.Add(method.SpeechGroupKey, new SpeechGroupTuple(true, new SpeechTuple(true, method, invokeInstance)));
                }
            }


            //add all synonyms
            foreach (var speechName in method.SpeechNames)
            {
                Dictionary<string, SpeechTuple> commands;

                //create if not exists
                if (langCommands.TryGetValue(speechName, out commands) == false)
                {
                    commands = new Dictionary<string, SpeechTuple>();
                    langCommands.Add(speechName, commands);
                }

                //a speech name could have multiple methods

                if (commands.ContainsKey(method.Key))
                    throw new Exception("speech dictionary already contains a method with speech name: " + method.SpeechNames + " and key: " + method.Key);

                commands.Add(method.Key, new SpeechTuple(true, method, invokeInstance));

            }
        }

        #endregion


        #region speech groups



        #endregion

    }
}

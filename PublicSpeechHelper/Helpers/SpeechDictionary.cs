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
        /// 1. language, 2. [1. speech group name, 2. [1. speech name, 2. (method)]]
        /// <para />
        /// e.g. 1. de-de, 2. [1. goto group , [(1. goto1, 2. goto item ... 1) ; (1. goto2, 2. goto item ... 2)]]
        /// </summary>
        public Dictionary<string, Dictionary<string, SpeechGroupTuple>> Commands { get; set; }

        /// <summary>
        /// a list of simple commands
        /// //1. language, 2. [1. speech text, 2. method]
        /// </summary>
        public Dictionary<string, Dictionary<string, SimpleCommandTuple>> SimpleCommands { get; set; }

        /// <summary>
        /// just some plain phrases to look for
        /// </summary>
        public HashSet<string> PlainPhrases { get; set; }

        /// <summary>
        /// creates a new speech dictionary
        /// </summary>
        public SpeechDictionary()
        {
            Commands = new Dictionary<string, Dictionary<string, SpeechGroupTuple>>();
            PlainPhrases = new HashSet<string>();
            SimpleCommands = new Dictionary<string, Dictionary<string, SimpleCommandTuple>>();
        }



        #region all commands


        /// <summary>
        /// adds a simple method to execute
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the phrase a unique text for the language</param>
        /// <param name="action">the action to perfrom when the phrase is recognized</param>
        public void AddSimpleCommand(string lang, string text, Action action)
        {
            if (SimpleCommands.ContainsKey(lang) == false)
            {
                var simpleCommands = new Dictionary<string, SimpleCommandTuple>();
                simpleCommands.Add(text, new SimpleCommandTuple(action, true));
                SimpleCommands.Add(lang, simpleCommands);
            }
            else
            {
                var methods = SimpleCommands[lang];
                if (methods.ContainsKey(text) == false)
                {
                    methods.Add(text, new SimpleCommandTuple(action, true));
                }
                else
                    throw new Exception("speech dictionary already contains a simple method: " + text + " on language: " + lang);
            }
        }

        /// <summary>
        /// enables or disables a simple command
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the text</param>
        /// <param name="isEnabled">true: command is processed, false: not</param>
        public bool ChangeSimpleCommand(string lang, string text, bool isEnabled)
        {
            Dictionary<string, SimpleCommandTuple> langSimpleCommands;
            if (SimpleCommands.TryGetValue(lang, out langSimpleCommands))
            {
                SimpleCommandTuple simpleCommand;
                if (langSimpleCommands.TryGetValue(text, out simpleCommand))
                {
                    simpleCommand.IsEnabled = isEnabled;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// enables or disables a command
        /// </summary>
        /// <param name="key">the method key</param>
        /// <param name="isEnabled">true: command will be processed, false: not</param>
        public bool ChangeCommand(string key, bool isEnabled)
        {
            bool found = false;
            foreach (var speechGroup in this.Commands.Values)
            {
                foreach (var speechMethod in speechGroup.Values)
                {
                    foreach (var speechTuple in speechMethod.Commands)
                    {
                        if (speechTuple.Key == key)
                        {
                            speechTuple.Value.IsEnabled = false;
                            found = true;
                        }
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// enables or disables a speech group
        /// </summary>
        /// <param name="groupKey">the group key</param>
        /// <param name="isEnabled">true: commands will be processed, false: not</param>
        public bool ChangeSpeechGroup(string groupKey, bool isEnabled)
        {
            bool found = false;
            foreach (var langPair in this.Commands)
            {
                foreach (var speechMethod in langPair.Value)
                {
                    if (speechMethod.Key == groupKey)
                    {
                        speechMethod.Value.IsEnabled = false;
                        found = true;
                    }
                }
            }


            return found;
        }


        /// <summary>
        /// creates a new command
        /// </summary>
        /// <param name="method">the method</param>
        /// <param name="invokeInstance">the instance to invoke the method on (or null for static methods)</param>
        public void AddMethod(SpeechMethod method, object invokeInstance)
        {
           Dictionary<string, SpeechGroupTuple> langCommands;

            //create lang if not exists
            if (Commands.TryGetValue(method.Lang, out langCommands) == false)
            {
                langCommands = new Dictionary<string, SpeechGroupTuple>();
                Commands.Add(method.Lang, langCommands);
            }


            //add the method to the right speech group

            SpeechGroupTuple speechGroup;

            if (langCommands.TryGetValue(method.SpeechGroupKey, out speechGroup) == false)
            {
                speechGroup = new SpeechGroupTuple(true);
                langCommands.Add(method.SpeechGroupKey, speechGroup);
            }

            //add all synonyms


            //check all synonyms

            if (method.SpeechNames.Any(p => speechGroup.Commands.ContainsKey(p)))
            {
                //there is a synonyme that is already another speech name in this speech group -> invalid
                throw new Exception("speech dictionary already contains a method with speech name: " + method.SpeechNames + " and key: " + method.Key);
            }
            else
            {
                var cmd = new SpeechTuple(true, method, invokeInstance);
                foreach (var speechName in method.SpeechNames)
                    speechGroup.Commands.Add(speechName, cmd); //same command with different names...
            }
        }

        #endregion


    }
}

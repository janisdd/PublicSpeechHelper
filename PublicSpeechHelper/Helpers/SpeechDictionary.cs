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
        /// all converters
        /// </summary>
        public Dictionary<string, SpeechParameterConverter> Converters { get; set; }

        /// <summary>
        /// all normal (complex) commands
        /// <para />
        /// 1. language, 2. [1. speech group name, 2. [1. speech name, 2. (method)]]
        /// </summary>
        public Dictionary<string, Dictionary<string, SpeechGroupTuple>> Commands { get; set; }

        /// <summary>
        /// a list of simple commands
        /// 1. language, 2. [1. speech group name, [1. speech name, 2. method]]
        /// </summary>
        public Dictionary<string, Dictionary<string, SimpleSpeechGroupTuple>> SimpleCommands { get; set; }

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
            SimpleCommands = new Dictionary<string, Dictionary<string, SimpleSpeechGroupTuple>>();
            Converters = new Dictionary<string, SpeechParameterConverter>();
        }



        #region all commands

        /// <summary>
        /// adds a simple method to execute
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the phrase a unique text for the language</param>
        /// <param name="simpleSpeechGroupKey">the simple speech group key for this simple command</param>
        /// <param name="action">the action to perfrom when the phrase is recognized</param>
        public void AddSimpleCommand(string lang, string text, string simpleSpeechGroupKey, Action action)
        {
            Dictionary<string, SimpleSpeechGroupTuple> langSpeechGroups;

            if (this.SimpleCommands.TryGetValue(lang, out langSpeechGroups) == false)
            {
                //no language tuple for simple commands
                langSpeechGroups = new Dictionary<string, SimpleSpeechGroupTuple>();
                this.SimpleCommands.Add(lang, langSpeechGroups);
            }

            SimpleSpeechGroupTuple simpleSpeechGroup;
            if (langSpeechGroups.TryGetValue(simpleSpeechGroupKey, out simpleSpeechGroup) == false)
            {
                //no speech group with this key here
                simpleSpeechGroup = new SimpleSpeechGroupTuple(true);
                langSpeechGroups.Add(simpleSpeechGroupKey, simpleSpeechGroup);
            }


            if (simpleSpeechGroup.Commands.ContainsKey(text))
            {
                throw new Exception("speech dictionary already contains a simple method: " + text + "on speech group: " + 
                    simpleSpeechGroupKey + " on language: " + lang);
            }

            simpleSpeechGroup.Commands.Add(text, new SimpleCommandTuple(action, true));
        }

        /// <summary>
        /// enables or disables a simple command with this text
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the text</param>
        /// <param name="isEnabled">true: command is processed, false: not</param>
        public bool ChangeSimpleCommand(string lang, string text, bool isEnabled)
        {
            bool found = false;
            foreach (var speechGroup in this.SimpleCommands.Values)
            {
                foreach (var speechMethod in speechGroup.Values)
                {
                    foreach (KeyValuePair<string, SimpleCommandTuple> speechTuple in speechMethod.Commands)
                    {
                        //speechTuple.Key is the speech name
                        if (speechTuple.Key == text)
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
        /// enables or disables a simple speech group
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="simpleGroupKey">the group key</param>
        /// <param name="isEnabled">true: commands will be processed, false: not</param>
        public bool ChangeSimpleSpeechGroup(string lang, string simpleGroupKey, bool isEnabled)
        {
            bool found = false;

            Dictionary<string, SimpleSpeechGroupTuple> langCommands;

            if (this.SimpleCommands.TryGetValue(lang, out langCommands))
            {
                foreach (var speechMethod in langCommands)
                {
                    if (speechMethod.Key == simpleGroupKey)
                    {
                        speechMethod.Value.IsEnabled = false;
                        found = true;
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// enables or disables all commands with this key
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
                    foreach (KeyValuePair<string, SpeechTuple> speechTuple in speechMethod.Commands)
                    {
                        //speechTuple.key is the speech name
                        if (speechTuple.Value.Method.Key == key)
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

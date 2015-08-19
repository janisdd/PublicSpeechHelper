using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper.Helpers
{
    /// <summary>
    /// encapsules the fields for the OnBeforeSimpleCommandInvoked event
    /// </summary>
    public class BeforeSimpleCommandInvokedArgs
    {
        /// <summary>
        /// the language
        /// </summary>
        public string Lang { get; private set; }

        /// <summary>
        /// the text of the simple command
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// the speech group key of the simple command
        /// </summary>
        public string SpeechGroupKey { get; private set; }

        /// <summary>
        /// the action that will be executed
        /// </summary>
        public Action Action { get; private set; }

        /// <summary>
        /// true: cancels the invokation, false: executes the method
        /// </summary>
        public bool IsCanceled { get; set; }


        /// <summary>
        /// creates a new BeforeSimpleCommandInvokedArgs
        /// </summary>
        /// <param name="lang">the language</param>
        /// <param name="text">the simple command text</param>
        /// <param name="speechGroupKey">the speech group key</param>
        /// <param name="action">the action to execute</param>
        /// <param name="isCanceled">true: cancels the invokation, false: executes the method</param>
        public BeforeSimpleCommandInvokedArgs(string lang, string text, string speechGroupKey, Action action, bool isCanceled)
        {
            this.Lang = lang;
            this.Text = text;
            this.SpeechGroupKey = speechGroupKey;
            this.Action = action;
            this.IsCanceled = isCanceled;
        }

    }
}

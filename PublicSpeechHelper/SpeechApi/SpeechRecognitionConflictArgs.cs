using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PublicSpeechHelper.Helpers;

namespace PublicSpeechHelper.SpeechApi
{

    public class SpeechRecognitionConflictArgs
    {
        /// <summary>
        /// the recognized text
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// all conflicting commands/methods
        /// </summary>
        public List<SpeechTuple> Methods { get; set; }

        public SpeechRecognitionConflictArgs(string text, params SpeechTuple[] speechMethods)
        {
            Text = text;
            this.Methods = speechMethods.ToList();
        }
    }
}

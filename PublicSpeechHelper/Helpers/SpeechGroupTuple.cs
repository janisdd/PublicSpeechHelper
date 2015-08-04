using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PublicSpeechHelper.SpeechApi;

namespace PublicSpeechHelper.Helpers
{
    /// <summary>
    /// a SpeechGroupTuple
    /// </summary>
    public class SpeechGroupTuple
    {
        /// <summary>
        /// true: group is enabled, false: not
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// all commands in this group
        /// </summary>
        public Dictionary<string, SpeechTuple> Commands { get; set; }

        public SpeechGroupTuple(bool isEnabled)
        {
            this.IsEnabled = isEnabled;
            Commands = new Dictionary<string, SpeechTuple>();
        }
    }
}

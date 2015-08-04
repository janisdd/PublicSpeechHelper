using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper.Helpers
{
    public class SimpleSpeechGroupTuple
    {
        /// <summary>
        /// true: group is enabled, false: not
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// all commands in this group
        /// </summary>
        public Dictionary<string, SimpleCommandTuple> Commands { get; set; }

        public SimpleSpeechGroupTuple(bool isEnabled)
        {
            this.IsEnabled = isEnabled;
            Commands = new Dictionary<string, SimpleCommandTuple>();
        }
    }
}

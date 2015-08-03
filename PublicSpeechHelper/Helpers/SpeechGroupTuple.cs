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
        /// all methods in this group
        /// </summary>
        public List<SpeechTuple> Methods { get; set; }

        public SpeechGroupTuple(bool isEnabled, params SpeechTuple[] methods)
        {
            this.IsEnabled = isEnabled;
            this.Methods = methods.ToList();
        }
    }
}

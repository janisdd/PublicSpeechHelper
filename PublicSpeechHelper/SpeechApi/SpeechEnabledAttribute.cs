using System;

namespace PublicSpeechHelper.SpeechApi
{
    /// <summary>
    /// marks a class to be visible to the crawler
    /// <para />
    /// only methods in a class with this attribute are recognized by the crawler
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SpeechEnabledAttribute : Attribute
    {
    }
}

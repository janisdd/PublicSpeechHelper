using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PublicSpeechHelper.SpeechApi;

namespace PublicSpeechHelper.Helpers
{
    /// <summary>
    /// a crawler to look for speech enabled types and methods and parameters
    /// </summary>
    public static class Crawler
    {
        /// <summary>
        /// cralws the given types for speech enabled types and methods and parameters
        /// </summary>
        /// <param name="ignoreSpeechEnabledAttribute">true: crawl each type, false: crawl only types with SpeechEnabledAttribute</param>
        /// <param name="types">the types to crawl</param>
        /// <returns></returns>
        public static List<SpeechMethod> CrawlTypes(bool ignoreSpeechEnabledAttribute = false, params Type[] types)
        {
            var commands = new List<SpeechMethod>();

            foreach (var type in types)
            {
                if (ignoreSpeechEnabledAttribute == false && type.GetCustomAttributes(typeof(SpeechEnabledAttribute), false).Length > 0)
                {
                    CrawMethods(type, commands, type.GetMethods());
                }
                else
                {
                    CrawMethods(type, commands, type.GetMethods());
                }

            }

            return commands;
        }

        /// <summary>
        /// crawls all given methods
        /// </summary>
        /// <param name="type">the type</param>
        /// <param name="methodInfos">the method infos</param>
        /// <param name="commands">the resulting commands</param>
        public static void CrawMethods(Type type, List<SpeechMethod> commands, params MethodInfo[] methodInfos)
        {
            foreach (var methodInfo in methodInfos)
            {
                Dictionary<string, SpeechMethod> langMethods = CrawlMethod(methodInfo, type); //get all possible speech methods out of the method

                commands.AddRange(langMethods.Values);
            }
        }


        /// <summary>
        /// crawls a method 
        /// </summary>
        /// <param name="methodInfo">the method info</param>
        /// <param name="type">the class of the method</param>
        /// <returns>[1. lang, 2. speech method]</returns>
        private static Dictionary<string, SpeechMethod> CrawlMethod(MethodInfo methodInfo, Type type)
        {
            var mattributes = methodInfo.GetCustomAttributes(typeof(SpeechMethodAttribute), false);

            var crawledMethods = new Dictionary<string, SpeechMethod>();

            if (mattributes.Length > 0)
            {

                IEnumerable<SpeechMethodAttribute> speechMethodAttributes = mattributes.Cast<SpeechMethodAttribute>();

                foreach (var speechMethodAttribute in speechMethodAttributes)
                {
                    //set method parameters
                    var speechMethod = new SpeechMethod();
                    speechMethod.Key = speechMethodAttribute.Key;
                    speechMethod.SpeechGroupKey = speechMethodAttribute.SpeechGroupKey;
                    speechMethod.Lang = speechMethodAttribute.Lang;
                    speechMethod.SpeechNames.AddRange(speechMethodAttribute.SpeechNames);
                    speechMethod.MethodInfo = methodInfo;
                    speechMethod.ExecutingType = type;

                    crawledMethods.Add(speechMethod.Lang, speechMethod);
                }

                //now get possible parameters
                var parameters = methodInfo.GetParameters();
                foreach (var parameterInfo in parameters)
                {
                    var pattributes = parameterInfo.GetCustomAttributes(typeof(SpeechParameterAttribute), false);

                    if (pattributes.Length > 0)
                    {
                        var parameterAttributes = pattributes.Cast<SpeechParameterAttribute>();

                        foreach (var publicSpeechArgumentAttribute in parameterAttributes)
                        {

                            //set parameter parameters
                            var speechParameter = new SpeechParameter();
                            speechParameter.SpeechNames = publicSpeechArgumentAttribute.SpeechNames;
                            speechParameter.ParameterInfo = parameterInfo;

                            SpeechMethod speechMethod;

                            if (crawledMethods.TryGetValue(publicSpeechArgumentAttribute.Lang, out speechMethod))
                            {
                                if (speechMethod.Arguments.Any(p => p.ParameterInfo.Name == parameterInfo.Name))
                                {
                                    throw new Exception("only one parameter attribute per language is allowed, language: " + publicSpeechArgumentAttribute.Lang
                                   + " parameter: " + parameterInfo.Name);
                                }
                                else
                                {
                                    speechMethod.Arguments.Add(speechParameter);
                                }
                            }
                            else
                                throw new Exception("No " + typeof(SpeechMethodAttribute).FullName + " specified for parameter: " +
                                    parameterInfo.Name + " for lang: " + publicSpeechArgumentAttribute.Lang + " on type: " + type.FullName);
                        }

                    }
                }
            }

            return crawledMethods;
        }
    }
}

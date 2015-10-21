using System;
//kkk
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

        #region converters

        /// <summary>
        /// crawls the given types for speech enabled types and methods that are speech parameter converters
        /// </summary>
        /// <param name="declaredConverters">the converters that are already known</param>
        /// <param name="ignoreSpeechEnabledAttribute">true: crawl each type, false: crawl only types with SpeechEnabledAttribute</param>
        /// <param name="types">the types to crawl</param>
        /// <returns></returns>
        public static List<SpeechParameterConverter> CrawlConverterTypes(Dictionary<string, SpeechParameterConverter> declaredConverters, 
            bool ignoreSpeechEnabledAttribute = false, params Type[] types)
        {

            var converters = new Dictionary<string, SpeechParameterConverter>();

            foreach (var type in types)
            {
                if (ignoreSpeechEnabledAttribute == false && type.GetCustomAttributes(typeof(SpeechEnabledAttribute), false).Length > 0)
                {
                    CrawlConverterMethods(type, converters, declaredConverters,type.GetMethods());
                }
                else
                {
                    CrawlConverterMethods(type, converters, declaredConverters,type.GetMethods());
                }

            }

            return converters.Values.ToList();


        }

        /// <summary>
        /// crawls the given type for parameter converter methods
        /// </summary>
        /// <param name="type">the type to crawl</param>
        /// <param name="converters">the resulting converters</param>
        /// <param name="declaredConverters">the converters that are already known</param>
        /// <param name="methodInfos">the method infos</param>
        public static void CrawlConverterMethods(Type type, Dictionary<string, SpeechParameterConverter> converters,
            Dictionary<string, SpeechParameterConverter> declaredConverters,  params MethodInfo[] methodInfos)
        {
            foreach (var methodInfo in methodInfos)
            {
                var cattributes = methodInfo.GetCustomAttributes(typeof(SpeechParameterConverterAttribute), false);


                if (cattributes.Length > 0)
                {
                    IEnumerable<SpeechParameterConverterAttribute> speechMethodAttributes = cattributes.Cast<SpeechParameterConverterAttribute>();

                    //should be only one

                    var attrib = speechMethodAttributes.FirstOrDefault();

                    if (attrib == null)
                    {
                        throw new Exception("#0 unknown");
                    }
                    else
                    {

                        //check if its a valid method... 1 string argument, 1 return parameter 

                        ParameterInfo[] parameters = methodInfo.GetParameters();

                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string) && methodInfo.ReturnType != typeof(void))
                        {
                            var converter = new SpeechParameterConverter();
                            converter.Key = attrib.Key;
                            converter.ExecutingType = type;
                            converter.MethodInfo = methodInfo;


                            if (converters.ContainsKey(converter.Key) || declaredConverters.ContainsKey(converter.Key))
                                throw new Exception("the parameter converter method key: " + methodInfo.Name + " on type: " + type.FullName +
                                    " is already used by another method");

                            converters.Add(converter.Key, converter);
                        }
                        else
                        {
                            throw new Exception("a parameter converter method must have 1 string parameter and returning the correct type for " +
                                                "the methods that use this converter (-> never void).\ntype: " + type.FullName + "method: " + methodInfo.Name);
                        }
                    }
                }
            }

        }

        #endregion

        #region methods

        /// <summary>
        /// cralws the given types for speech enabled types and methods and parameters
        /// </summary>
        /// <param name="converters">the list of converters</param>
        /// <param name="ignoreSpeechEnabledAttribute">true: crawl each type, false: crawl only types with SpeechEnabledAttribute</param>
        /// <param name="types">the types to crawl</param>
        /// <returns></returns>
        public static List<SpeechMethod> CrawlTypes(Dictionary<string,SpeechParameterConverter> converters, bool ignoreSpeechEnabledAttribute = false, params Type[] types)
        {
            var commands = new List<SpeechMethod>();
            
            foreach (var type in types)
            {
                if (ignoreSpeechEnabledAttribute == false && type.GetCustomAttributes(typeof(SpeechEnabledAttribute), false).Length > 0)
                {
                    CrawMethods(type, commands,converters, type.GetMethods());
                }
                else
                {
                    CrawMethods(type, commands,converters, type.GetMethods());
                }

            }

            return commands;
        }

        /// <summary>
        /// crawls all given methods
        /// </summary>
        /// <param name="type">the type</param>
        /// <param name="converters">the list of converters</param>
        /// <param name="methodInfos">the method infos</param>
        /// <param name="commands">the resulting commands</param>
        public static void CrawMethods(Type type, List<SpeechMethod> commands, Dictionary<string,SpeechParameterConverter> converters, params MethodInfo[] methodInfos)
        {
            foreach (var methodInfo in methodInfos)
            {
                Dictionary<string, SpeechMethod> langMethods = CrawlMethod(methodInfo, type, converters); //get all possible speech methods out of the method

                commands.AddRange(langMethods.Values);
            }
        }


        /// <summary>
        /// crawls a method 
        /// </summary>
        /// <param name="methodInfo">the method info</param>
        /// <param name="type">the class of the method</param>
        /// <param name="converters">the list of converters</param>
        /// <returns>[1. lang, 2. speech method]</returns>
        private static Dictionary<string, SpeechMethod> CrawlMethod(MethodInfo methodInfo, Type type, Dictionary<string, SpeechParameterConverter> converters)
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


                            //check if we have a converter for this parameter
                            SpeechParameterConverter converter;
                            if (converters.TryGetValue(publicSpeechArgumentAttribute.ConverterKey, out converter))
                            {
                                //converter return type and parameter type must be equal
                                if (converter.MethodInfo.ReturnType == parameterInfo.ParameterType)
                                {
                                    speechParameter.Converter = converter;
                                }
                                else
                                {
                                    throw new Exception("the type returned by the converter: " + converter.Key + " ("+ converter.MethodInfo.ReturnType + ") is not equal to the " +
                                                        "parameter type: " + parameterInfo.ParameterType + "(type: " + type.FullName + " method:" + 
                                                        methodInfo.Name + " parameter "+ parameterInfo.Name + ")");   
                                }
                            }
                            else
                            {
                                throw new Exception("cannot find parameter converter with key: " + publicSpeechArgumentAttribute.ConverterKey + "\ntype: " +
                                type.FullName + " method: " + methodInfo.Name + " parameter: " + parameterInfo.Name);
                            }

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
                                throw new Exception("no " + typeof(SpeechMethodAttribute).FullName + " specified for parameter: " +
                                    parameterInfo.Name + " for lang: " + publicSpeechArgumentAttribute.Lang + " on type: " + type.FullName);
                        }

                    }
                }
            }

            return crawledMethods;
        }

        #endregion
    }
}

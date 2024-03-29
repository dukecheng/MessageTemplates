﻿// Copyright 2013-2015 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using MessageTemplates.Core;
using MessageTemplates.Debugging;
using MessageTemplates.Parsing;
using MessageTemplates.Structure;

namespace MessageTemplates.Parameters
{
    // Performance relevant - on the hot path when creating log events from existing templates.
    class PropertyBinder
    {
        readonly PropertyValueConverter _valueConverter;

        static readonly TemplateProperty[] NoPropertiesArray = new TemplateProperty[0];
        static readonly TemplatePropertyList NoProperties = new TemplatePropertyList(NoPropertiesArray);

        public PropertyBinder(PropertyValueConverter valueConverter)
        {
            _valueConverter = valueConverter;
        }

        /// <summary>
        /// Create properties based on an ordered list of provided values.
        /// </summary>
        /// <param name="messageTemplate">The template that the parameters apply to.</param>
        /// <param name="messageTemplateParameters">Objects corresponding to the properties
        /// represented in the message template.</param>
        /// <returns>A list of properties; if the template is malformed then
        /// this will be empty.</returns>
        public TemplatePropertyList ConstructProperties(MessageTemplate messageTemplate, IReadOnlyDictionary<string, object> messageTemplateParameters)
        {
            if (messageTemplateParameters == null)
            {
                if (messageTemplate.NamedProperties != null || messageTemplate.PositionalProperties != null)
                    SelfLog.WriteLine("Required properties not provided for: {0}", messageTemplate);

                return NoProperties;
            }

            //if (messageTemplate.PositionalProperties != null)
            //    return ConstructPositionalProperties(messageTemplate, messageTemplateParameters);

            return ConstructNamedProperties(messageTemplate, messageTemplateParameters);
        }

        //TemplatePropertyList ConstructPositionalProperties(MessageTemplate template, object[] messageTemplateParameters)
        //{
        //    var positionalProperties = template.PositionalProperties;

        //    if (positionalProperties.Length != messageTemplateParameters.Length)
        //        SelfLog.WriteLine("Positional property count does not match parameter count: {0}", template);

        //    var result = new TemplateProperty[messageTemplateParameters.Length];
        //    foreach (var property in positionalProperties)
        //    {
        //        if (property.TryGetPositionalValue(out var position))
        //        {
        //            if (position < 0 || position >= messageTemplateParameters.Length)
        //                SelfLog.WriteLine("Unassigned positional value {0} in: {1}", position, template);
        //            else
        //                result[position] = ConstructProperty(property, messageTemplateParameters[position]);
        //        }
        //    }

        //    var next = 0;
        //    for (var i = 0; i < result.Length; ++i)
        //    {
        //        if (result[i] != null)
        //        {
        //            result[next] = result[i];
        //            ++next;
        //        }
        //    }

        //    if (next != result.Length)
        //        Array.Resize(ref result, next);

        //    return new TemplatePropertyList(result);
        //}

        TemplatePropertyList ConstructNamedProperties(MessageTemplate template, IReadOnlyDictionary<string, object> messageTemplateParameters)
        {
            var namedProperties = template.NamedProperties;
            if (namedProperties == null)
                return NoProperties;

            var matchedRun = namedProperties.Length;
            //if (namedProperties.Length != messageTemplateParameters.Length)
            //{
            //    matchedRun = Math.Min(namedProperties.Length, messageTemplateParameters.Length);
            //    SelfLog.WriteLine("Named property count does not match parameter count: {0}", template);
            //}

            //var pis = GetablePropertyFinder.GetPropertiesRecursive(messageTemplateParameters.GetType());
            var result = new TemplateProperty[matchedRun];
            for (var i = 0; i < matchedRun; ++i)
            {
                var property = template.NamedProperties[i];
                if (!messageTemplateParameters.ContainsKey(property.PropertyName))
                {
                    continue;
                }

                result[i] = ConstructProperty(property, messageTemplateParameters[property.PropertyName]);
            }

            return new TemplatePropertyList(result);
        }

        TemplateProperty ConstructProperty(PropertyToken propertyToken, object value)
        {
            return new TemplateProperty(
                        propertyToken.PropertyName,
                        _valueConverter.CreatePropertyValue(value, propertyToken.Destructuring));
        }
    }
}

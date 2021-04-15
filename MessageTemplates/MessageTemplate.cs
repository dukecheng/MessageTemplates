using MessageTemplates.Core;
using MessageTemplates.Debugging;
using MessageTemplates.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MessageTemplates
{
    public class MessageTemplate
    {
        readonly string _text;
        readonly MessageTemplateToken[] _tokens;

        // Optimisation for when the template is bound to
        // property values.
        readonly PropertyToken[] _positionalProperties;
        readonly PropertyToken[] _namedProperties;

        /// <summary>
        /// Construct a message template using manually-defined text and property tokens.
        /// </summary>
        /// <param name="text">The full text of the template; used by MessageTemplates internally to avoid unneeded
        /// string concatenation.</param>
        /// <param name="tokens">The text and property tokens defining the template.</param>
        public MessageTemplate(string text, IEnumerable<MessageTemplateToken> tokens)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            _text = text;
            _tokens = tokens.ToArray();

            var propertyTokens = _tokens.OfType<PropertyToken>().Distinct().ToArray();
            if (propertyTokens.Length != 0)
            {
                var allPositional = true;
                var anyPositional = false;
                foreach (var propertyToken in propertyTokens)
                {
                    if (propertyToken.IsPositional)
                        anyPositional = true;
                    else
                        allPositional = false;
                }

                if (allPositional)
                {
                    _positionalProperties = propertyTokens;
                }
                else
                {
                    if (anyPositional)
                        SelfLog.WriteLine("Message template is malformed: {0}", text);

                    _namedProperties = propertyTokens;
                }
            }
        }

        /// <summary>
        /// The raw text describing the template.
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Render the template as a string.
        /// </summary>
        /// <returns>The string representation of the template.</returns>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// The tokens parsed from the template.
        /// </summary>
        public IEnumerable<MessageTemplateToken> Tokens => _tokens;

        internal PropertyToken[] NamedProperties => _namedProperties;

        internal PropertyToken[] PositionalProperties => _positionalProperties;

        public static string Format(
            IFormatProvider formatProvider,
            string templateMessage,
            IReadOnlyDictionary<string, object> values)
        {
            var sw = new StringWriter(formatProvider);
            Format(formatProvider, sw, templateMessage, values);
            sw.Flush();
            return sw.ToString();
        }

        public static void Format(
          IFormatProvider formatProvider,
          TextWriter output,
          string templateMessage,
          IReadOnlyDictionary<string, object> value)
        {
            var template = Parse(templateMessage);
            template.Format(formatProvider, output, value);
        }

        /// <summary>
        /// Render
        /// </summary>
        public void Format(IFormatProvider formatProvider, TextWriter output, IReadOnlyDictionary<string, object> value)
        {
            var props = Capture(this, value);
            this.Render(new TemplatePropertyValueDictionary(props), output, formatProvider);
        }


        /// <summary>
        /// Captures properties from the given message template and
        /// provided values.
        /// </summary>
        public static TemplatePropertyList Capture(
            MessageTemplate template, IReadOnlyDictionary<string, object> value)
        {
            var binder = new Parameters.PropertyBinder(
                new Parameters.PropertyValueConverter(
                    10,
                    Enumerable.Empty<Type>(),
                    Enumerable.Empty<IDestructuringPolicy>()));

            //var values = value.GetType().GetProperties().Select(x => x.GetValue(value)).ToArray();
            return binder.ConstructProperties(template, value);
        }

        public void Render(TemplatePropertyValueDictionary properties, TextWriter output, IFormatProvider formatProvider = null)
        {
            foreach (var token in _tokens)
            {
                token.Render(properties, output, formatProvider);
            }
        }

        /// <summary>
        /// Parses a message template (e.g. "hello, {name}") into a
        /// <see cref="MessageTemplate"/> structure.
        /// </summary>
        /// <param name="templateMessage">A message template (e.g. "hello, {name}")</param>
        /// <returns>The parsed message template.</returns>
        public static MessageTemplate Parse(string templateMessage)
        {
            return new MessageTemplateParser().Parse(templateMessage);
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;

namespace MessageTemplates.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        [Fact]
        public void AnAnonymousObjectUseFormatProvider()
        {
            var valueString = JsonConvert.SerializeObject(new { Income = 1234.567, Date = new DateTime(2013, 5, 20) });
            var objValues = JsonConvert.DeserializeObject(valueString) as JObject;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            foreach (var item in objValues)
            {
                parameters.Add(item.Key, GetRealValue(item.Value));
            }

            var messageTemplate = "Income was {Income} at {Date:d}";
            IFormatProvider formatProvider = new CultureInfo("fr-FR");
            var m = MessageTemplate.Format(formatProvider, messageTemplate, parameters);
            Assert.Equal("Income was 1234,567 at 20/05/2013", m);
        }

        private object GetRealValue(JToken value)
        {
            switch (value.GetType().Name)
            {
                case "JValue":
                    return ((JValue)value).Value;
                default:
                    _testOutputHelper.WriteLine($"unknow json type:{value.GetType().Name}");
                    return value;
            }
        }
    }
}

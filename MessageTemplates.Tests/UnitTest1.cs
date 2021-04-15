using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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

            var messageTemplate = "{Income} Income was {Income} at {Date:d}";
            IFormatProvider formatProvider = new CultureInfo("fr-FR");
            var m = MessageTemplate.Format(formatProvider, messageTemplate, parameters);
            Assert.Equal("1234,567 Income was 1234,567 at 20/05/2013", m);
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

        [Fact]
        public void QueueTest()
        {
            int maxQueueCapcity = 2;
            Queue<string> queue = new Queue<string>(maxQueueCapcity);
            for (int i = 0; i < 10; i++)
            {
                queue.Enqueue(i.ToString());
                if (queue.Count > 2)
                {
                    for (int j = maxQueueCapcity; j < queue.Count; j++)
                    {
                        queue.TryDequeue(out _);
                    }
                }
            }
            Assert.Equal(2, queue.Count);
        }
    }
}

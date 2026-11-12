using System.Text.Json;
using Xunit;

namespace UnlockedData.Chartist.Blazor.UnitTests.Core
{
    public class ChartistMouseEventArgsShould
    {
        private static readonly JsonSerializerOptions JsInteropOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [Fact]
        public void HavePointMetaInfoWhenConstructed()
        {
            var args = new ChartistMouseEventArgs();

            Assert.NotNull(args.PointMetaInfo);
            Assert.Equal("No Label", args.PointMetaInfo.Label);
        }

        [Theory]
        [InlineData("""{"y":1}""")]
        [InlineData("""{"y":1,"pointMetaInfo":{}}""")]
        public void HaveDefaultPointMetaInfoWhenTheEventHasNoMeta(string json)
        {
            var args = JsonSerializer.Deserialize<ChartistMouseEventArgs>(json, JsInteropOptions)!;

            Assert.NotNull(args.PointMetaInfo);
            Assert.Equal("No Label", args.PointMetaInfo.Label);
        }

        [Fact]
        public void BindMetaAndSeriesName()
        {
            const string json = """{"x":2,"y":1,"pointMetaInfo":{"id":"a","label":"Monday"},"seriesName":"Series 1"}""";

            var args = JsonSerializer.Deserialize<ChartistMouseEventArgs>(json, JsInteropOptions)!;

            Assert.Equal("a", args.PointMetaInfo.Id);
            Assert.Equal("Monday", args.PointMetaInfo.Label);
            Assert.Equal("Series 1", args.SeriesName);
        }

        [Fact]
        public void HaveNoSeriesNameForAnUnnamedSeries()
        {
            var args = JsonSerializer.Deserialize<ChartistMouseEventArgs>("""{"y":1,"seriesName":null}""", JsInteropOptions)!;

            Assert.Null(args.SeriesName);
        }
    }
}

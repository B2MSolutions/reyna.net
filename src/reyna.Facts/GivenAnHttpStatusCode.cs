namespace reyna.Facts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using Xunit.Extensions;
    using reyna.Interfaces;
    using reyna.Extensions;
    using Xunit;

    public class GivenAnHttpStatusCode
    {
        public static IEnumerable<object[]> OkResultPropertyData
        {
            get
            {
                return Enumerable.Range(200, 100)
                    .Select(i => new object[] { (HttpStatusCode)i, Result.Ok });
            }
        }

        public static IEnumerable<object[]> PermanentErrorResultPropertyData
        {
            get
            {
                return Enumerable.Range(300, 200)
                    .Select(i => new object[] { (HttpStatusCode)i, Result.PermanentError });
            }
        }

        public static IEnumerable<object[]> TemporaryErrorResultPropertyData
        {
            get
            {
                return Enumerable.Range(500, 100)
                    .Select(i => new object[] { (HttpStatusCode)i, Result.TemporaryError });
            }
        }

        [Theory]
        [PropertyData("OkResultPropertyData")]
        [PropertyData("PermanentErrorResultPropertyData")]
        [PropertyData("TemporaryErrorResultPropertyData")]
        public void WhenCallingToResultShouldReturnExpected(HttpStatusCode httpStatusCode, Result expected)
        {
            Assert.Equal(expected, httpStatusCode.ToResult());
        }
    }
}

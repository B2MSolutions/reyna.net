namespace Reyna.Facts
{
    using Xunit;

    public class GivenAnAcceptAllCertificatePolicy
    {
        [Fact]
        public void WhenCallingCheckValidationResultShouldReturnTrue()
        {
            var acceptAllCeritificatePolicy = new AcceptAllCertificatePolicy();
            Assert.True(acceptAllCeritificatePolicy.CheckValidationResult(null, null, null, 0));
        }
    }
}

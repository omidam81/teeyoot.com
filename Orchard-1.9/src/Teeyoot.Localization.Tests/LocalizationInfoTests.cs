using NUnit.Framework;
using Teeyoot.Localization.GeoLocation;

namespace Teeyoot.Localization.Tests
{
    [TestFixture]
    public class LocalizationInfoTests
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            var ipAddressProvider = new MockIpAddressProvider("113.210.0.0");
            LocalizationInfoFactory.Init(ipAddressProvider, new WebServiceGeoLocationInfoProvider(106603, "WmvDZnIe7c3I"));
        }

        [Test]
        public void Malaysia()
        {
            var localizationInfo = LocalizationInfoFactory.GetCurrentLocalizationInfo();

            Assert.AreEqual(Country.Malaysia, localizationInfo.Country);
        }

        [Test]
        public void StillMalaysia()
        {
            var localizationInfo = LocalizationInfoFactory.GetCurrentLocalizationInfo();

            Assert.AreEqual(Country.Malaysia, localizationInfo.Country);
        }
    }
}

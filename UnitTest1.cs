using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json.Linq;
using System.IO;

namespace probavamo
{
    public class Tests
    {
        private IWebDriver driver1;
        private string baseUrl1 = "https://corehr.qa.hrcloud.net/Start/#/Authentication/Login";

        [SetUp]
        public void Setup()
        {
            driver1 = new ChromeDriver();
            driver1.Manage().Window.Maximize();
        }

        [TearDown]
        public void TeardownTest()
        {
            driver1.Dispose();
        }

        [TestCaseSource(nameof(GetLoginData))]
        public void PositiveTests(LoginModel loginData)
        {
            driver1.Navigate().GoToUrl(baseUrl1);

            IWebElement usernameInput = driver1.FindElement(By.Name("username"));
            usernameInput.SendKeys(loginData.UserName);

            IWebElement passwordInput = driver1.FindElement(By.Name("password"));
            passwordInput.SendKeys(loginData.Password);

            IWebElement loginButton = driver1.FindElement(By.XPath("//button[contains(., 'Sign In Securely')]"));

            loginButton.Click();


            Thread.Sleep(5000);
            // Primjer validacije da li je uspješno prijavljen
            Assert.IsTrue(driver1.Url.Contains("HomePage"));
        }



        [TestCaseSource(nameof(GetLoginData))]
        public void NegativeTests(LoginModel loginData)
        {
            driver1.Navigate().GoToUrl(baseUrl1);

            IWebElement usernameInput = driver1.FindElement(By.Name("username"));
            usernameInput.SendKeys(loginData.UserName);

            IWebElement passwordInput = driver1.FindElement(By.Name("password"));
            passwordInput.SendKeys(loginData.Password);

            IWebElement loginButton = driver1.FindElement(By.XPath("//button[contains(., 'Sign In Securely')]"));

            loginButton.Click();


            Thread.Sleep(5000);
            // Primjer validacije da li je uspješno prijavljen
            Assert.IsTrue(driver1.FindElement(By.CssSelector(".alert-feedback-block.alert.alert-danger")).Displayed, "Greska bi trebala biti vidljiva");
        }

        public static IEnumerable<LoginModel> GetLoginData()
        {
            var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "podaci.json");
            var jsonString = File.ReadAllText(jsonFilePath);

            // Uvijek provjerite da li je jsonString null prije parsiranja
            if (jsonString == null)
            {
                // Ako jsonString nije pronađen ili nije moguće pročitati, vratite praznu listu
                return new List<LoginModel>();
            }

            var loginData = JArray.Parse(jsonString).ToObject<List<LoginModel>>();

            // Osigurajte da loginData nikada ne bude null, već uvijek lista s podacima
            return loginData ?? new List<LoginModel>();
        }
    }
}
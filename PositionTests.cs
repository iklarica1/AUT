using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Newtonsoft.Json.Linq;
using SeleniumExtras.WaitHelpers;

namespace probavamo
{
    public class PositionTests
    {
        private IWebDriver driver;
        private string baseUrl = "https://corehr.qa.hrcloud.net/Start/#/Authentication/Login";

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
        }

        [TearDown]
        public void TeardownTest()
        {
            driver.Dispose();
        }

        [Test]
        public void LoginTest()
        {
            driver.Navigate().GoToUrl(baseUrl);

            // Wait for the username input field to be visible
            IWebElement usernameInput = driver.FindElement(By.Name("username"));
            usernameInput.SendKeys("admin+iklarica@neogov.com");

            // Wait for the password input field to be visible
            IWebElement passwordInput = driver.FindElement(By.Name("password"));
            passwordInput.SendKeys("Welcome$2u");

            // Wait for the login button to be clickable, then click it
            IWebElement loginButton = driver.FindElement(By.XPath("//button[contains(., 'Sign In Securely')]"));
            loginButton.Click();

            Thread.Sleep(5000);
            Assert.IsTrue(driver.Url.Contains("HomePage"));
        }

        [TestCaseSource(nameof(GetPositionData))]
        public void AddPositionTest(PositionModel positionData)
        {
            // Perform login
            LoginTest();

            // Assuming the cookie banner can be dismissed via JavaScript
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("document.getElementById('hs-eu-cookie-confirmation-inner').style.display='none';");

            Thread.Sleep(5000);
            // Navigate to People -> Positions
            NavigateToPositions();

            // Click on add button to add new position
            IWebElement addButton = driver.FindElement(By.XPath("//a[contains(@class, 'aut-button-add')]"));
            addButton.Click();

            Thread.Sleep(5000);

            // Fill in the position form
            FillPositionForm(positionData);
            Thread.Sleep(3000);

            // Save the position
            IWebElement saveButton = driver.FindElement(By.XPath("//button[contains(., 'Save')]"));
            saveButton.Click();

            Thread.Sleep(5000);

            // Check for errors if fields are empty
            CheckForErrors(positionData);
        }

        private void NavigateToPositions()
        {
            // Navigate to People app
            IWebElement peopleApp = driver.FindElement(By.XPath("//a[@ng-href='#/CoreHr/People']"));
            Thread.Sleep(5000);
            peopleApp.Click();

            Thread.Sleep(5000);
            // Click on Positions
            IWebElement positionsMenu = driver.FindElement(By.ClassName("aut-button-positions"));
            positionsMenu.Click();
            Thread.Sleep(5000);
        }

        private void FillPositionForm(PositionModel positionData)
        {
            // Fill in the position form
            IWebElement positionNameInput = driver.FindElement(By.Name("xPosition-xPositionTitle"));
            positionNameInput.SendKeys(positionData.PositionName);

            IWebElement positionCodeInput = driver.FindElement(By.Name("xPosition-xPositionCode"));
            positionCodeInput.SendKeys(positionData.PositionCode);

            // Click on the dropdown button to display options
            IWebElement recordStatusButton = driver.FindElement(By.XPath("//button[contains(@class, 'aut-dropdown-xPosition-xRecordStatus')]"));
            recordStatusButton.Click();

            // Sleep for a while to ensure options are displayed
            Thread.Sleep(3000); // Adjust the sleep duration as necessary

            // Select the desired option from the dropdown
            IList<IWebElement> options = driver.FindElements(By.XPath("//a[contains(@class, 'aut-dropdown-xPosition-xRecordStatus')]"));
            foreach (IWebElement option in options)
            {
                if (option.Text == positionData.RecordStatus)
                {
                    option.Click();
                    break;
                }
            }
        }

        private void CheckForErrors(PositionModel positionData)
        {
            // Check for errors if fields are empty
            // Check for errors if fields are empty
            if (string.IsNullOrEmpty(positionData.PositionName))
            {
                Assert.That(driver.FindElement(By.CssSelector("span[data-valmsg-for='xPosition-xPositionTitle']")).Displayed, Is.True, "Position Name is required.");
            }

            if (string.IsNullOrEmpty(positionData.PositionCode))
            {
                // Check for the error message specific to Position Code using data-valmsg-for
                Assert.That(driver.FindElement(By.CssSelector("span[data-valmsg-for='xPosition-xPositionCode']")).Displayed, Is.True, "Position Code is required.");
            }

            // Check for Record Status
            IList<IWebElement> options = driver.FindElements(By.XPath("//a[contains(@class, 'aut-dropdown-xPosition-xRecordStatus')]"));
            bool isEnterRecordStatusSelected = false;

            foreach (IWebElement option in options)
            {
                if (option.Text == "Enter record status...")
                {
                    isEnterRecordStatusSelected = true;
                    break;
                }
            }

            if (isEnterRecordStatusSelected && positionData.RecordStatus == "Enter record status...")
            {
                Assert.That(driver.FindElement(By.XPath("//span[@data-valmsg-for='xPosition-xRecordStatus']")).Displayed, Is.True, "Error: Record status is required");
            }
        }

        


        public static IEnumerable<PositionModel> GetPositionData()
        {
            var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "podaci1.json");
            var jsonString = File.ReadAllText(jsonFilePath);

            // Uvijek provjerite da li je jsonString null prije parsiranja
            if (jsonString == null)
            {
                // Ako jsonString nije pronađen ili nije moguće pročitati, vratite praznu listu
                return new List<PositionModel>();
            }

            var positionData = JArray.Parse(jsonString).ToObject<List<PositionModel>>();

            // Osigurajte da positionData nikada ne bude null, već uvijek lista s podacima
            return positionData ?? new List<PositionModel>();
        }
    }
}
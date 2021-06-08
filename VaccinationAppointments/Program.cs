using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VaccinationAppointments
{
    class Program
    {
        static IWebDriver driver = new ChromeDriver();
        static List<IWebElement> availableVaccinationCenters = new List<IWebElement>();
        static IWebElement panelManage;
        static int zipcode;
        static bool found;
        static int retryPeriod;
        static int maxAttempts = 10;

        static void Main(string[] args)
        {
            found = false;
            int attempts = 0;
            driver.Navigate().GoToUrl("https://emvolio.gov.gr/app#/CovidVaccine/appointmentSearch");
            Console.WriteLine("Please wait...");
            WaitUntilLoaded();
            Login();
            GetZipCodeAndProceed();
            GetAvailableVaccinationCenters();
            retryPeriod = 60;
            Console.WriteLine($"I\'ll now start searching for appointments. I\'ll make {maxAttempts} attempts.");
            while (!found)
            {
                attempts++;
                FindAvailableAppointment();
                if (attempts == maxAttempts)
                {
                    Console.Write($"Timed out. No available appointments found near the area with zipcode {zipcode}.");
                    break;
                }
                if (!found)
                {
                    Thread.Sleep(retryPeriod * 1000);
                    Console.WriteLine("Retrying...");
                }
            }
            Console.WriteLine("Exiting...");
            driver.Quit();
        }

        static void WaitUntilLoaded()
        {
            new WebDriverWait(driver, new TimeSpan(0, 0, 1)).Until(
                d => ((IJavaScriptExecutor)d)
                .ExecuteScript("return document.readyState")
                .Equals("complete"));
        }

        static List<string> AskForCredentials()
        {
            List<string> credentials = new List<string>();
            Console.Write("Please enter your TaxisNet username: ");
            string username = Console.ReadLine();
            credentials.Add(username);
            Console.Write("Please enter your TaxisNet password: ");
            string password = Console.ReadLine();
            credentials.Add(password);
            return credentials;
        }

        static void Login()
        {
            List<string> credentials = AskForCredentials();

            while (true)
            {
                driver.FindElement(By.Name("j_username")).SendKeys(credentials[0]);
                driver.FindElement(By.Name("j_password")).SendKeys(credentials[1]);
                driver.FindElement(By.Id("btn-login-submit")).Click();
                WaitUntilLoaded();

                if (driver.Url == "https://www1.gsis.gr/oauth2server/login.jsp?authentication_error=true")
                {
                    Console.WriteLine("Invalid credentials. Please try again.");
                    credentials = AskForCredentials();
                    continue;
                }
                else
                {
                    Console.WriteLine("Successfully logged in.");
                    Console.WriteLine("Please wait...");
                    break;
                }
            }
            driver.FindElement(By.Id("btn-submit")).Click();
            WaitUntilLoaded();
        }

        static void GetZipCodeAndProceed()
        {
            Thread.Sleep(10000);
            zipcode = int.Parse(driver.FindElement(By.Id("userTK")).Text);
            Console.WriteLine($"Your current zip code is {zipcode}.");
            driver.FindElement(By.XPath("//*[@id=\"bottomScreen\"]/div/button[2]")).Click();
            Thread.Sleep(10000);
        }

        static void GetAvailableVaccinationCenters()
        {
            panelManage = driver.FindElement(By.XPath("//*[@id=\"panelManage\"]/div[3]/span[2]/span"));
            panelManage.Click();
            Thread.Sleep(10000);

            IWebElement dropdownList = driver.FindElement(By.ClassName("k-animation-container")).FindElements(By.XPath(".//*"))[0].FindElements(By.XPath(".//*"))[1].FindElements(By.XPath(".//*"))[0];
            IReadOnlyCollection<IWebElement> listOptions = dropdownList.FindElements(By.XPath(".//*"));
            foreach (IWebElement option in listOptions)
            {
                availableVaccinationCenters.Add(option);
            }
        }

        static void FindAvailableAppointment()
        {
            Console.WriteLine("Searching for appointments. Please be patient...");
            int iterator = 0;
            int maxIterator = availableVaccinationCenters.Count();

            // Reset
            panelManage.Click();
            Thread.Sleep(500);
            IWebElement activeElement = driver.SwitchTo().ActiveElement();
            for (int i=0; i<maxIterator; i++)
            {
                activeElement.SendKeys(Keys.ArrowUp);
                Thread.Sleep(100);
            }
            activeElement.SendKeys(Keys.Return);
            Thread.Sleep(5000);

            // Search
            for (iterator = 0; iterator < maxIterator; iterator++)
            {
                IWebElement currentVaccinationCenter = availableVaccinationCenters.ElementAt(iterator);
                IWebElement error = driver.FindElement(By.ClassName("errorMessageNotAvailableAppointment"));
                string errorStyle = error.GetAttribute("style");
                if (errorStyle.Contains("display: none;"))
                {
                    found = true;
                    Console.WriteLine($"APPOINTMENT FOUND AT {currentVaccinationCenter.GetAttribute("innerHTML")}.");
                    SoundPlayer player = new SoundPlayer(Properties.Resources.Alarm01);
                    Console.WriteLine("Press any key to continue...");
                    player.PlaySync();
                    player.PlaySync();
                    player.PlaySync();
                    player.PlaySync();
                    player.PlaySync();
                    Console.ReadLine();
                    Console.WriteLine("You have to RSVP the appointment. I\'m not capable yet to do this for you.");
                }

                panelManage.Click();
                Thread.Sleep(500);
                activeElement = driver.SwitchTo().ActiveElement();
                activeElement.SendKeys(Keys.ArrowDown);
                Thread.Sleep(500);
                activeElement.SendKeys(Keys.Return);
                Thread.Sleep(5000);
            }

            // Not Found actions
            if (!found)
            {
                Console.WriteLine($"No available appointments found near the area with zipcode {zipcode}.");
                Console.WriteLine($"I\'ll retry in {retryPeriod} seconds.");
            }

        }
    }
}

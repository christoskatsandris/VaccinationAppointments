using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccinationAppointments
{
    class Program
    {
        static IWebDriver driver = new ChromeDriver();

        static void Main(string[] args)
        {
            driver.Navigate().GoToUrl("https://emvolio.gov.gr/app#/CovidVaccine/appointmentSearch");
            WaitUntilLoaded();
            List<string> credentials = AskForCredentials();
            Login(credentials);
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

        static void Login(List<string> credentials)
        {
            driver.FindElement(By.Name("j_username")).SendKeys(credentials[0]);
            driver.FindElement(By.Name("j_password")).SendKeys(credentials[1]);
            driver.FindElement(By.Id("btn-login-submit")).Click();
            WaitUntilLoaded();
            driver.FindElement(By.Id("btn-submit")).Click();
            WaitUntilLoaded();
        }
    }
}

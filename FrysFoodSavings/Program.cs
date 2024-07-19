// See https://aka.ms/new-console-template for more information

//Use selenium to click coupons on Fry's Food and Drug website
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Edge;

namespace FrysFoodSavings
{
    class Program
    {
        static void Main(string[] args)
        {
            //Ask for user input into console for username and password
            Console.WriteLine("Enter your Fry's Food and Drug username: ");
            string username = Console.ReadLine();
            Console.WriteLine("Enter your Fry's Food and Drug password: ");
            //Hide password input
            string password = "";
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);



            EdgeOptions options = new EdgeOptions();

            // Add options to disable automation flags
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");
            options.AddArguments("disable-infobars");
            options.AddArgument("--guest");
            options.AddArgument("--auto-open-devtools-for-tabs");
            options.AddUserProfilePreference("enable_do_not_track", true);

            // Now, create a new instance of the Edge driver with the configured options
            IWebDriver driver = new EdgeDriver(options);

            //SignIn to Fry's Food and Drug website
            driver.Navigate().GoToUrl("https://www.frysfood.com/signin?redirectUrl=/savings/cl/coupons/");
            //Wait for the page to load
            Thread.Sleep(2500);
            //Get url
            string url = driver.Url;
            //nav to url
            driver.Navigate().GoToUrl(url);
            driver.Navigate().Refresh();// Refresh because of error
            Thread.Sleep(2500);
            //Notify user to sign in using windows dialog popup
            driver.FindElement(By.Id("signInName")).SendKeys(username);
            driver.FindElement(By.Id("password")).SendKeys(password);
            //Press enter on password field
            driver.FindElement(By.Id("password")).SendKeys(Keys.Enter);

            Thread.Sleep(1000);

            driver.FindElement(By.Id("next")).Click();
            Thread.Sleep(10000);
            try
            {
                // Find the button by its aria-label and click it
                IWebElement closeButton = driver.FindElement(By.XPath("//button[@aria-label='Close pop-up']"));
                closeButton.Click();
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine("Close pop-up button not found.");
                // Handle the case where the button is not found as needed
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Handle other exceptions
            }
            // Wait for the elements to be visible and clickable
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
   
            // After navigating to the website and before closing the browser

            //check if page is loaded
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//button[contains(text(), 'Clip')]")));
           bool bQuit = false;
            try
            {
                while (true)
                {
                    // Assuming "CouponActionButton" and "Clip" text are consistent attributes
                    IList<IWebElement> clipButtons = driver.FindElements(By.XPath("//button[contains(text(), 'Clip')]"));

                    foreach (IWebElement button in clipButtons)
                    {
                        // Wait for the button to be clickable
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(button));
                        button.Click();
                        Thread.Sleep(1000); // Consider using more sophisticated wait conditions

                        try
                        {
                            // Attempt to find the element containing the specific message
                            IWebElement messageElement = driver.FindElement(By.XPath("//p[contains(@class, 'kds-Paragraph') and contains(@class, 'kds-GlobalMessage-body')]/span[contains(text(), 'You have reached the maximum of 200 coupons loaded to your card.')]"));

                            // If the element is found, the message is present on the page
                            Console.WriteLine("Too many coupons loaded. Time to quit");
                            //Close the browser
                            bQuit = true;
                            break;
                        }
                        catch (NoSuchElementException)
                        {
                           
                        }
                        catch (Exception ex)
                        {
                            // Handle any other exceptions that might occur
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                    }
                    if (bQuit)
                        break;
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("No more coupons to clip");
                // Handle exceptions or errors as needed
            }
            //Close the browser
            driver.Quit();
        }
    }
}

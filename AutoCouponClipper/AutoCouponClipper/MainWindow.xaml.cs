using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
//Use selenium to click coupons on Fry's Food and Drug website
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.Graphics;
using OpenQA.Selenium.Edge;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AutoCouponClipper
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        bool bRunning = false;
        public MainWindow()
        {
            this.InitializeComponent();
            SizeInt32 size = new SizeInt32() { Height = 400, Width = 400 };
            this.AppWindow.Resize(size);

        }


        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            string username = userNameTextBox.Text;
            string password = passwordBox.Password;

            if (bRunning)
            {
                return;
            }
            bRunning = true;
            //Call Run function on a new thread
            Thread thread = new Thread(() => Run(username, password));
            thread.Start();
        }

        //function Run. Take username and password
        private void Run(string username, string password)
        {
            try
            {
                EdgeOptions options = new EdgeOptions();

                // Add options to disable automation flags
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddArgument("--disable-notifications");
                options.AddArgument("--disable-popup-blocking");
                options.AddArguments("disable-infobars");
                options.AddArgument("--guest");
                options.AddArgument("--window-size=800,768");
                options.AddUserProfilePreference("enable_do_not_track", true);

                // Configure EdgeDriverService to hide the console window
                EdgeDriverService service = EdgeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;

                // Now, create a new instance of the Edge driver with the configured options
                IWebDriver driver = new EdgeDriver(service, options);




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
                // Wait for the elements to be visible and clickable
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                try
                {
                    //Wait for url to change to coupons page
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.UrlContains("https://www.frysfood.com/savings"));

                    //Wait until the pop-up is visible
                    wait.Timeout = TimeSpan.FromSeconds(10);
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//button[@aria-label='Close pop-up']")));

                    // Find the button by its aria-label and click it
                    IWebElement closeButton = driver.FindElement(By.XPath("//button[@aria-label='Close pop-up']"));
                    closeButton.Click();
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Close pop-up button not found.");
                    // Handle the case where the button is not found as needed
                }
                catch (Exception)
                {
                    Console.WriteLine("Close pop-up button not found.");
                }

                wait.Timeout = TimeSpan.FromSeconds(30);
                bool bQuit = false;
                try
                {
                    //check if page is loaded
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//button[contains(text(), 'Clip')]")));
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
                catch (Exception)
                {
                    Console.WriteLine("No more coupons to clip");
                    // Handle exceptions or errors as needed
                }
                //Close the browser
                driver.Quit();
                bRunning = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                bRunning = false;
            }
        }
    }
}

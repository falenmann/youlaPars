using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace youlaPars
{
    public class CarModel
    {
        public string Model { get; set; }
        public List<string> Generations { get; set; }
    }

    public class CarMake
    {
        public string Make { get; set; }
        public List<CarModel> Models { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            List<string> completedMakes = new List<string>();
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "manufacturer_*.json");
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var makeName = fileName.Replace("manufacturer_", "");
                completedMakes.Add(makeName);
            }

            ChromeOptions options = new ChromeOptions();

            // Настройка мобильной эмуляции для iPhone X
            options.EnableMobileEmulation("iPhone X");

            IWebDriver driver = new ChromeDriver(options);

            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                driver.Navigate().GoToUrl("https://youla.ru/product/create");
                Thread.Sleep(200);
                driver.Navigate().GoToUrl("https://youla.ru/product/create");
                Thread.Sleep(600);
                var carElement = driver.FindElement(By.XPath("//span[text()='Легковые автомобили']/ancestor::li"));
                carElement.Click();
                Thread.Sleep(400);
                var carElement1 = driver.FindElement(By.XPath("//span[text()='С пробегом']/ancestor::li"));
                carElement1.Click();
                Thread.Sleep(400);
                var brandElement = driver.FindElement(By.XPath("//div[@data-name='attributes.auto_brand']"));
                brandElement.Click();
                Thread.Sleep(400);
                
                List<string?> GenerationsName = new List<string?>();

                var radioButtons = driver.FindElements(By.XPath("//div[@class='sc-idAmOS hbkyRM']"));

                for (int i = 143; i < radioButtons.Count; i++)
                {
                    List<CarMake> carsMakes = new List<CarMake>();
                    List<CarModel> carsModels = new List<CarModel>();
                    
                    // Найти элементы заново на каждой итерации
                    radioButtons = driver.FindElements(By.XPath("//div[@class='sc-idAmOS hbkyRM']"));

                    var radioButton = radioButtons[i];

                    var makeName = radioButton.FindElement(By.XPath("./label")).Text;
                    radioButton.Click();
                    Thread.Sleep(1200);
                    if (driver.FindElements(By.XPath("//div[@data-name = 'attributes.auto_model']")).Count == 0)
                    {
                        carsMakes.Add(new CarMake
                        {
                            Make = makeName,
                            Models = carsModels
                        });
                      
                        // Сохранение данных в файл JSON
                        string json1 = JsonConvert.SerializeObject(carsMakes, Formatting.Indented);
                        string filePath1 = Path.Combine(Directory.GetCurrentDirectory(), $"manufacturer_{makeName}.json");
                        File.WriteAllText(filePath1, json1);
                        brandElement = driver.FindElement(By.XPath("//div[@data-name='attributes.auto_brand']"));
                        brandElement.Click();
                        Thread.Sleep(400);
                        continue;
                    }
                    var radioButtonModels = driver.FindElement(By.XPath("//div[@data-name = 'attributes.auto_model']"));
                    radioButtonModels.Click();
                    Thread.Sleep(800);

                    // Получение списка моделей
                    var radioButtonsModels = driver.FindElements(By.XPath("//div[@class='sc-idAmOS hbkyRM']"));

                    for (int j = 0; j < radioButtonsModels.Count; j++)
                    {
                        Thread.Sleep(800);
                        // Повторное нахождение элементов на каждой итерации
                        radioButtonsModels = driver.FindElements(By.XPath("//div[@class='sc-idAmOS hbkyRM']"));

                        var radioButtonModel = radioButtonsModels[j];
                       

                        GenerationsName = new List<string>();
                        var modelName = radioButtonModel.FindElement(By.XPath("./label")).Text;
                        radioButtonModel.Click();
                        Thread.Sleep(1400);

                        // Проверка на существование поколения и работа с ним
                        var generationElementExists = driver.FindElements(By.CssSelector("div[data-name='attributes.auto_generation']")).Count > 0;
                        if (generationElementExists)
                        {
                            var divElement = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div[data-name='attributes.auto_generation']")));
                            divElement.Click();

                            var radioButtonsGeneration = driver.FindElements(By.XPath("//div[@class='sc-idAmOS hbkyRM']"));
                            var radioButtonGeneration = radioButtonsGeneration[0];
                            for (int k = 0; k < radioButtonsGeneration.Count; k++)
                            {
                                // Повторное нахождение элементов поколения
                                radioButtonsGeneration = driver.FindElements(By.XPath("//div[@class='sc-idAmOS hbkyRM']"));

                                 radioButtonGeneration = radioButtonsGeneration[k];
                                var GenerationName = radioButtonGeneration.FindElement(By.XPath("./label")).Text;
                                GenerationsName.Add(GenerationName);
                                Thread.Sleep(800);
                            }
                            radioButtonGeneration.Click();
                        }
                        else
                        {
                            // Если поколений нет, добавляем "1 поколение"
                            GenerationsName.Add("1 поколение");
                        }

                        carsModels.Add(new CarModel
                        {
                            Model = modelName,
                            Generations = GenerationsName
                        });
                        Thread.Sleep(800);
                        var radioButtonModels1 = driver.FindElement(By.XPath("//div[@data-name = 'attributes.auto_model']"));
                        radioButtonModels1.Click();
                    }

                    carsMakes.Add(new CarMake
                    {
                        Make = makeName,
                        Models = carsModels
                    });
                    radioButtonsModels = driver.FindElements(By.XPath("//div[@class='sc-idAmOS hbkyRM']"));

                    var radioButtonModel1 = radioButtonsModels[0];
                    // Сохранение данных в файл JSON
                    string json = JsonConvert.SerializeObject(carsMakes, Formatting.Indented);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"manufacturer_{makeName}.json");
                    File.WriteAllText(filePath, json);
                    
                    radioButtonModel1.Click();
                    Thread.Sleep(400);
                    
                    // Переход к следующему бренду
                    brandElement = driver.FindElement(By.XPath("//div[@data-name='attributes.auto_brand']"));
                    brandElement.Click();
                    Thread.Sleep(400);
                }

                // Закрытие браузера после завершения всех операций
                driver.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                driver.Quit();
            }
        }
    }
}

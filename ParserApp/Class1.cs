using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;

namespace NetApp
{
    public class Group
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public List<string>[] Table { get; set; }

        public XElement GetXElement()
        {
            XElement table = new XElement("table");
            foreach (var elem in this.Table)
            {
                XElement day = new XElement("day", new XAttribute("day", elem[0]));
                for (int i = 1; i < elem.Count; i++)
                {
                    if (i % 2 == 1) day.Add(new XElement("subject", elem[i]));
                    else day.Add(new XElement("room", elem[i]));
                }
                table.Add(day);
            }
            return new XElement("group",
                    new XAttribute("name", this.Name),
                    new XElement("date", this.Date),
                    table
                );
        }
        public XDocument GetXDocument()
        {
            return new XDocument(GetXElement());
        }
        public void Save(string path)
        {
            GetXDocument().Save(path);
        }
    }
    public class Area
    {
        private string address;
        private List<Group> groups;

        public Area(string address)
        {
            this.address = address;
            CreateValue();
        }
        public List<Group> Groups { get => groups; }

        private async void CreateValue()
        {
            Console.WriteLine("Задача запускается!");
            Task<List<Group>> task1 = Task.Run(() => new Parser(address).ParsePage());
            Console.WriteLine("Задача создана!");
            this.groups = await task1;
            Console.WriteLine("Задача завершена!");
            if (groups != null) Console.WriteLine("Лист \"groups\" не равен нулю!");
        }
        public void PrintGroups()
        {
            foreach (var elem1 in groups)
            {
                Console.WriteLine(elem1.Name);
                Console.WriteLine(elem1.Date);
                foreach (var elem2 in elem1.Table)
                {
                    foreach (var elem3 in elem2)
                    {
                        Console.WriteLine(elem3);
                    }
                }
            }
        }
        public void SaveGroups(string path)
        {
            XElement xElem = new XElement("groups");
            for (int i = 0; i < groups.Count; i++)
            {
                xElem.Add(groups[i].GetXElement());
            }
            new XDocument(xElem).Save(path);
        }
    }
    public class Downloader
    {
        public static void GetSitePage(string _address)
        {
            //http://mgke.minsk.edu.by/ru/main.aspx?guid=3791
            WebClient client = new WebClient();
            client.DownloadFile(_address, "c:\\Downloads\\page.html");
        }
    }
    public class Parser
    {
        private string address;
        public Parser(string address)
        {
            this.address = address;
        }
        private string StreamReader()
        {
            string text = "";
            using (StreamReader sr = new StreamReader(address))
            {
                string str;
                while ((str = sr.ReadLine()) != null)
                {
                    text += str + "\n";
                }
            }
            return text;
        }
        public async Task<List<Group>> ParsePage()
        {
            string PageText = StreamReader();
            List<string> names = new List<string>();
            List<string> dates = new List<string>();
            List<IElement> tables = new List<IElement>();
            List<Group> groups = new List<Group>();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(PageText));
            var doc = document.DocumentElement;

            foreach (var elem in doc.GetElementsByTagName("h2"))
            {
                names.Add(elem.Text());
            }
            foreach (var elem in doc.GetElementsByTagName("h3"))
            {
                dates.Add(elem.Text());

            }
            foreach (var elem in doc.GetElementsByTagName("tbody"))
            {
                tables.Add(elem);
            }

            for (int i = 0; i < names.Count; i++)
            {
                Group group = new Group();
                group.Name = names[i];
                group.Date = dates[i];
                group.Table = TableParser(tables[i]);
                groups.Add(group);
            }

            Console.WriteLine("Парсинг завершён!");
            return groups;
        }
        public List<string>[] TableParser(IElement table)
        {
            //в таблице изредка отсутствует суббота
            List<string>[] tp = new List<string>[table.Children[0].GetElementCount() - 1];
            
            try
            {
                foreach (var elem1 in table.ChildNodes)
                {
                    //пропуск пустых контейнеров о  т удвоенных ячеек
                    if (elem1.GetElementCount() == 0) continue;
                    //заполнение заголовков, дней недели
                    else if (elem1.GetElementCount() == 7 
                        | elem1.GetElementCount() == 6)
                    {
                        int x = -1;
                        //пропуск пустой ячейки
                        if (elem1.Index() == 0) continue;
                        foreach (var elem2 in elem1.ChildNodes)
                        {
                            //пропуск пустых нечётных ячеек
                            if (elem2.Index() % 2 == 0) continue;
                            //пропуск первой ячейки с №
                            if (elem2.Index() == 1) continue;
                            x = (elem2.Index() - 2 + (elem2.Index() % 2)) / 2 - 1;
                            Console.Write("Заголовок. Строка №" 
                                + elem2.Index() + ", день №" 
                                + x.ToString() + ": " + elem2.Text());

                            tp[x] = new List<string>();
                            tp[x].Add(elem2.Text().TrimEnd().TrimStart());
                        }
                        continue;
                    }
                    //заголовки столбцов: "предмет" и "ауд."
                    else if (elem1.GetElementCount() == 12) continue;
                    else
                    {
                        int x;
                        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                        foreach (var elem2 in elem1.ChildNodes)
                        {
                            if (elem2.Index() == 1) continue;
                            else if (elem2.Index() % 2 == 0) continue;

                            //Пипец какая дичь в вычислениях!

                            x = ((((elem2.Index() - 2) + ((elem2.Index() - 2) % 2)) / 2) - ((elem2.Index() - 2) % 2)) / 2;
                            Console.WriteLine("Значение elem2: " + (elem2.Index() - 2).ToString() + ", Индекс x: " + x.ToString());
                            tp[x].Add(elem2.Text().TrimEnd().TrimStart());
                            Console.Write(elem2.Text());
                        }
                    }
                    Console.WriteLine("Кол-во в elem1: " + elem1.GetElementCount() + ", индекс: " + elem1.Index());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Исключение: " + e.Message);
            }
            return tp;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetApp
{
    class Program
    {
        static async void TestAsync()
        {
            Area area = new Area("page.html");
            while (area.Groups == null)
            {
                Console.WriteLine("Ожидание...");
                await Task.Delay(100);
            }
            List<Group> groups = area.Groups;
            groups[3].Save("c:\\SomeDir\\Groups\\group.xml");
            Console.WriteLine("Кол-во групп: " + groups.Count);
            area.SaveGroups("c:\\SomeDir\\Groups\\groups.xml");
        }
        static void Main(string[] args)
        {
            Console.Title = "";

            TestAsync();

            Console.ReadKey();
        }
    }
}

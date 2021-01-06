using BusinessLayer.Interfaces;
using BusinessLayer.Managers;
using BusinessLayer.Model;
using BusinessLayer.Tools;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ConsoleAppKlantBestellingen
{
    class Program
    {
        static void Main(string[] args)
        {
            IDFactory idF = new IDFactory(0,100,5000); //klant,bestelling,product
            //var kM = new /*Db*/KlantManager() as IManager<Klant>;
            //var pM = new ProductManager() as IManager<Product>;
            //var bM = new BestellingManager() as IManager<Bestelling>;
            string connectionString = @"Data Source=LAPTOP-263M7I30\SQLEXPRESS;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            var kM = new SQLKlantManager(connectionString) as IManager<Klant>;
            var pM = new SQLProductManager(connectionString) as IProductManager;
            var bM = new SQLBestellingManager(connectionString) as IManager<Bestelling>;

            pM.VoegToe(new Product("product 1", 10.0));
            pM.VoegToe(new Product("product 2", 12.0));
            pM.VoegToe(new Product("product 3", 13.0));
            foreach (var x in pM.HaalOp()) Console.WriteLine(x);

            Console.WriteLine("-----------------");

            kM.VoegToe(new Klant("klant 1", "adres 1"));
            kM.VoegToe(new Klant("klant 2", "adres 2"));
            foreach (var x in kM.HaalOp()) //Console.WriteLine(x);
                x.Show();


            Console.WriteLine("-----------------");

            bM.VoegToe(new Bestelling(0, kM.HaalOp().Last(), DateTime.Now, new Dictionary<Product, int> { { pM.HaalOp("product 1"), 1 }, { pM.HaalOp("product 2"), 2 } }));
            bM.VoegToe(new Bestelling(0, kM.HaalOp().Last(), DateTime.Now, new Dictionary<Product, int> { { pM.HaalOp("product 1"), 4 }, { pM.HaalOp("product 3"), 3 } }));

            //Bestelling b = bM.HaalOp(1);
            //b.VoegProductToe(pM.HaalOp(5001),8);
            //b.VoegProductToe(pM.HaalOp(5002), 7);
            //Console.WriteLine($"Prijs:{b.Kostprijs()}, {b.PrijsBetaald}");
            //b.ZetBetaald();
            //Console.WriteLine($"Prijs:{b.Kostprijs()}, {b.PrijsBetaald}");

            foreach (var x in bM.HaalOp()) //Console.WriteLine(x);
                x.Show();
            
            /*
            Console.WriteLine("--------HAALOP() met DELEGATE---------");
            foreach (var x in pM.HaalOp(k => k.Naam.Contains('1'))) //Console.WriteLine(x);
                Console.WriteLine(x);
            */

            /*
            Console.WriteLine("-------VERWIJDER PRODUCT----------");
            pM.Verwijder(pM.HaalOp().Last());
            foreach (var x in pM.HaalOp()) Console.WriteLine(x);

            Console.WriteLine("-------VERWIJDER KLANT----------");
            kM.Verwijder(kM.HaalOp().Last());
            foreach (var x in kM.HaalOp()) Console.WriteLine(x);

            Console.WriteLine("-------VERWIJDER BESTELLING----------");
            bM.Verwijder(bM.HaalOp().Last());
            foreach (var x in bM.HaalOp()) Console.WriteLine(x);
            */
        }
    }
}

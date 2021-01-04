using BusinessLayer.Interfaces;
using BusinessLayer.Managers;
using BusinessLayer.Model;
using BusinessLayer.Tools;
using System;

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
            var pM = new SQLProductManager(connectionString) as IManager<Product>;
            var bM = new SQLBestellingManager(connectionString) as IManager<Bestelling>;

            pM.VoegToe(ProductFactory.MaakProduct("product 1", 10.0, idF));
            pM.VoegToe(ProductFactory.MaakProduct("product 2", 12.0, idF));
            pM.VoegToe(ProductFactory.MaakProduct("product 3", 13.0, idF));
            foreach (var x in pM.HaalOp()) Console.WriteLine(x);

            Klant klant1 = KlantFactory.MaakKlant("klant 1", "adres 1", idF);
            Klant klant2 = KlantFactory.MaakKlant("klant 2", "adres 2", idF);
            //kM.VoegToe(KlantFactory.MaakKlant("klant 1", "adres 1", idF));
            //kM.VoegToe(KlantFactory.MaakKlant("klant 2", "adres 2", idF));
            kM.VoegToe(klant1);
            kM.VoegToe(klant2);
            foreach (var x in kM.HaalOp()) //Console.WriteLine(x);
                x.Show();


            bM.VoegToe(BestellingFactory.MaakBestelling(klant1,idF));
            bM.VoegToe(BestellingFactory.MaakBestelling(kM.HaalOp(2),idF));

            Bestelling b = bM.HaalOp(7);
            //b.VoegProductToe(pM.HaalOp(5001),8);
            //b.VoegProductToe(pM.HaalOp(5002), 7);
            Console.WriteLine($"Prijs:{b.Kostprijs()}, {b.PrijsBetaald}");
            b.ZetBetaald();
            Console.WriteLine($"Prijs:{b.Kostprijs()}, {b.PrijsBetaald}");

            foreach (var x in bM.HaalOp()) //Console.WriteLine(x);
                x.Show();
            foreach (var x in kM.HaalOp()) //Console.WriteLine(x);
                x.Show();
            Console.WriteLine("-----------------");
            Klant k1 = kM.HaalOp(1);
            k1.Show();
            k1.VoegToeBestelling(b);
            k1.Show();
        }
    }
}

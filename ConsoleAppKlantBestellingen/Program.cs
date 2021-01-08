﻿using BusinessLayer.Interfaces;
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

            Klant k = new Klant("klant 1", "adres 1");
            //k = kM.HaalOp().Last();
            foreach (var x in bM.HaalOp(b => b.Klant.Naam == k.Naam)) //Console.WriteLine(x);
                x.Show();
            /*
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
            foreach (var x in bM.HaalOp()) //Console.WriteLine(x);
                x.Show();
            */
            /*
            Console.WriteLine("--------VOEGTOE() BESTELLING UPDATE---------");
            foreach (var x in bM.HaalOp()) //Console.WriteLine(x);
                x.Show();
            Console.WriteLine("-----------------");
            Bestelling b = bM.HaalOp().Last();
            b.VoegProductToe(pM.HaalOp().Last(), 8);
            bM.VoegToe(b);
            foreach (var x in bM.HaalOp()) //Console.WriteLine(x);
                x.Show();
            */

            /*
            Console.WriteLine("--------VOEGTOE() KLANT UPDATE---------");
            kM.VoegToe(new Klant("klant 3", "adres 3"));
            foreach (var x in kM.HaalOp()) //Console.WriteLine(x);
                x.Show();
            Console.WriteLine("-----------------");
            Klant k = kM.HaalOp().Last();
            Bestelling b = new Bestelling(0, DateTime.Now);
            pM.VoegToe(new Product("product 4", 14.0));
            b.VoegProductToe(pM.HaalOp("product 4"), 10);
            k.VoegToeBestelling(b);
            kM.VoegToe(k);
            foreach (var x in kM.HaalOp()) //Console.WriteLine(x);
                x.Show();
            foreach (Bestelling a in kM.HaalOp().Last().GetBestellingen())
                a.Show();
            */

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

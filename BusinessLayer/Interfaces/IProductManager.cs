using BusinessLayer.Model;
using System;
using System.Collections.Generic;

namespace BusinessLayer.Managers
{
    public interface IProductManager
    {
        IReadOnlyList<Product> HaalOp();
        IReadOnlyList<Product> HaalOp(Func<Product, bool> predicate);
        Product HaalOp(long productId);
        Product HaalOp(string naam);
        void Verwijder(Product product);
        void VoegToe(Product product);
    }
}
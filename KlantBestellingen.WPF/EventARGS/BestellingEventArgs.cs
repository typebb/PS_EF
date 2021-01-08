using BusinessLayer.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace KlantBestellingen.WPF.EventARGS
{
    public class BestellingEventArgs : EventArgs
    {
       public Bestelling bestelling { get; set; }
    }
}

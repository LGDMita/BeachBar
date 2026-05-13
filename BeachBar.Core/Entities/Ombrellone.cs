using System;
using System.Collections.Generic;
using System.Text;

namespace BeachBar.Core.Entities
{
    public class Ombrellone
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public bool Occupato { get; set; }
        public ICollection<Sessione> Sessioni { get; set; } = new List<Sessione>();
    }
}

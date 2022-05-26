using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModel
{
    public class ElementView
    {
        public Entity.Element Element { get; set; }
        public bool VisibleBySearch { get; set; }
    }
}

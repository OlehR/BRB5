using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRB6.Template
{
    interface IViewRDI:IView
    {
        public BRB5.Model.RaitingDocItem Data { get; set; }
    }
}

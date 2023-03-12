using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class Warehouse
    {
        /// <summary>
        /// Унікальний Код може бути такий же як і Number
        /// </summary>
        public int Code {get;set;}
        /// <summary>
        /// Номер в випадку Sim23 4 значний код 1104
        /// </summary>
        public string Number {get;set;}
        public int CodeWarehouse { get { try { return Convert.ToInt32(Number); } catch { return 0; } }}
        /// <summary>
        /// Назва складу
        /// </summary>
        public string Name {get;set;}

        public string Address { get;set;}

        public string Url {get;set;}
        public string InternalIP {get;set;}
        public string ExternalIP {get;set;}
        /// <summary>
        /// gps координати магазину 23.3451,45.2344
        /// </summary>
        public string Location { get;set;}

        public double GPSX { get { return Location.Split(',').Length == 2 ? Convert.ToDouble(Location.Split(',')[0]):0d; } }
        public double GPSY { get { return Location.Split(',').Length == 2 ? Convert.ToDouble(Location.Split(',')[1]):0d; } }
        /// <summary>
        /// Дистанція
        /// </summary>
        public double Distance { get; set; }

    }
}
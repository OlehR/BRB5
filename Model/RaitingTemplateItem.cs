using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BRB5.Model
{
    public  class RaitingTemplateItem
    {
        public int IdTemplate { get; set; }
        public int Id { get; set; }
        public int Parent { get; set; }
        
        [JsonIgnore]
        // заголовок групи
        public bool IsHead { get; set; }       
        
        public string Text { get; set; }        
        /// <summary>
        /// Доступні варіанти відповіді 1 -погано + 2-так собі + 4 - добре + 8 - відсутня відповідь
        /// </summary>
        public int RatingTemplate { get; set; }
        /// <summary>
        /// Порядок сортування
        /// </summary>
        public int OrderRS { get; set; } 
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace PropertyTool.DataBase
{
    public class Property
    {
        [Key]
        public string Id { get; set; }

        public string Address { get; set; }

        public DateTime Sold { get; set; }

        public int Price { get; set; }

        public string PropertyType { get; set; }

        public int LandSize { get; set; }
    }
}
namespace HomeWork_18_WPF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int Money { get; set; }

        public int Department { get; set; }

        public int Deposit { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOpen { get; set; }

        public int? Days { get; set; }

        public double? Rate { get; set; }
    }
}

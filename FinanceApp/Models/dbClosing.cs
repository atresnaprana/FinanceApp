using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
namespace FinanceApp.Models
{
    public class dbClosing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string description { get; set; }
        public int periode { get; set; }
        public int year { get; set; }
        public DateTime datefrom { get; set; }
        public DateTime dateto { get; set; }

        public string isclosed { get; set; }
        public string entry_user { get; set; }
        public string update_user { get; set; }

        public DateTime update_date { get; set; }
        public DateTime entry_date { get; set; }

        [NotMapped]
        public bool isclosebool { get; set; }


    }
}

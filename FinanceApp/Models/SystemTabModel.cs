using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BaseLineProject.Models;
namespace BaseLineProject.Models
{
    public class SystemTabModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string TAB_DESC { get; set; }
        public string ROLE_ID { get; set; }
        public string TAB_TXT { get; set; }
        public string FLAG_AKTIF { get; set; }
        public string ENTRY_USER { get; set; }
        public string UPDATE_USER { get; set; }
        public DateTime ENTRY_DATE { get; set; }
        public DateTime UPDATE_DATE { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
        [NotMapped]
        public List<SystemMenuModel> menuList { get; set; }



    }
}

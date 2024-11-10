using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseLineProject.Models
{
    public class SystemMenuModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int TAB_ID { get; set; }
        public string MENU_DESC { get; set; }
        public string ROLE_ID { get; set; }
        public string MENU_TXT { get; set; }
        public string MENU_LINK { get; set; }
        public string FLAG_AKTIF { get; set; }
        public string ENTRY_USER { get; set; }
        public string UPDATE_USER { get; set; }
        public DateTime ENTRY_DATE { get; set; }
        public DateTime UPDATE_DATE { get; set; }
        [NotMapped]
        public List<SystemTabModel> ddTab { get; set; }
        [NotMapped]
        public bool IsSelected { get; set; }

    }
}

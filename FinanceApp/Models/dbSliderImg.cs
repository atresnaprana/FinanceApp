using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace BaseLineProject.Models
{
    public class dbSliderImg
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string IMG_DESC { get; set; }
        public string FILE_NAME { get; set; }
        public byte[] SLIDE_IMG_BLOB { get; set; }
        public string FLAG_AKTIF { get; set; }
        public string ENTRY_USER { get; set; }
        public string UPDATE_USER { get; set; }
        public DateTime ENTRY_DATE { get; set; }
        public DateTime UPDATE_DATE { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileImg { get; set; }
        [NotMapped]
        public string imgUrl { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace BaseLineProject.Models
{
    public class dbCustomer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string EDP { get; set; }
        public string CUST_NAME { get; set; }
        public string COMPANY_ID { get; set; }
        public string COMPANY { get; set; }
        public string NPWP { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string postal { get; set; }
        public string BANK_NAME { get; set; }
        public string BANK_NUMBER { get; set; }
        public string BANK_BRANCH { get; set; }
        public string BANK_COUNTRY { get; set; }
        public DateTime REG_DATE { get; set; }
        public string BL_FLAG { get; set; }
        public DateTime ENTRY_DATE { get; set; }
        public DateTime UPDATE_DATE { get; set; }
        public string ENTRY_USER { get; set; }
        public string UPDATE_USER { get; set; }
        public string FLAG_AKTIF { get; set; }
        public string Email { get; set; }
        public string KTP { get; set; }
        public string PHONE1 { get; set; }
        public string PHONE2 { get; set; }
        public string isApproved { get; set; }
        public string VA1 { get; set; }
        public string VA2 { get; set; }
        public string VA1NOTE { get; set; }
        public string VA2NOTE { get; set; }
        public byte[] FILE_KTP { get; set; }
        public byte[] FILE_AKTA { get; set; }
        public byte[] FILE_REKENING { get; set; }
        public byte[] FILE_NPWP { get; set; }
        public byte[] FILE_TDP { get; set; }
        public byte[] FILE_SIUP { get; set; }
        public byte[] FILE_NIB { get; set; }
        public byte[] FILE_SPPKP { get; set; }
        public byte[] FILE_SKT { get; set; }

        public string FILE_KTP_NAME { get; set; }
        public string FILE_AKTA_NAME { get; set; }
        public string FILE_REKENING_NAME { get; set; }
        public string FILE_NPWP_NAME { get; set; }
        public string FILE_TDP_NAME { get; set; }
        public string FILE_SIUP_NAME { get; set; }
        public string FILE_NIB_NAME { get; set; }
        public string FILE_SPPKP_NAME { get; set; }
        public string FILE_SKT_NAME { get; set; }
        public string isApproved2 { get; set; }
        public string store_area { get; set; }
        public decimal? discount_customer { get; set; }
        public int totalstoreconfig { get; set; }

        [NotMapped]
        public bool isBlackList { get; set; }
        [NotMapped]
        public bool isApproveBool { get; set; }
        [NotMapped]
        public bool isApproveBool2 { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileKtp { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileAkta { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileRekening { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileNPWP { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileTdp { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileSIUP { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileNIB { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileSPPKP { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile fileSKT { get; set; }

        [NotMapped]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [NotMapped]
        public string errmsg { get; set; }
    }
}

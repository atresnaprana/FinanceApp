using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.Security.Policy;
using BaseLineProject.Models;
using MimeDetective.Storage;
using FinanceApp.Models;

namespace BaseLineProject.Data
{
    public class FormDBContext : DbContext
    {
        public DbSet<SystemTabModel> TabTbl { get; set; }
        public DbSet<SystemMenuModel> MenuTbl { get; set; }
        public DbSet<dbSliderImg> SlideTbl { get; set; }
        public DbSet<dbCustomer> CustomerTbl { get; set; }

        public DbSet<dbAccount> AccountTbl { get; set; }

        public DbSet<dbKas> KastTbl { get; set; }

        public FormDBContext(DbContextOptions<FormDBContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Use Fluent API to configure  

            // Map entities to tables  


            modelBuilder.Entity<dbCompanySeq>().ToTable("dbCompany_seq");

            modelBuilder.Entity<dbCustomerSeq>().ToTable("dbCustomer_seq");
            modelBuilder.Entity<SystemTabModel>().ToTable("SystemTabTbl");
            modelBuilder.Entity<SystemMenuModel>().ToTable("SystemMenuTbl");
            modelBuilder.Entity<dbSliderImg>().ToTable("dbSliderImg");
            modelBuilder.Entity<dbCustomer>().ToTable("dbCustomer");
            modelBuilder.Entity<dbAccount>().ToTable("dbAccount");
            modelBuilder.Entity<dbKas>().ToTable("dbKas");




            modelBuilder.Entity<SystemTabModel>().HasKey(ug => ug.ID).HasName("PK_TAB_Seq");
            modelBuilder.Entity<SystemMenuModel>().HasKey(ug => ug.ID).HasName("PK_MENU_Seq");

            modelBuilder.Entity<dbSliderImg>().HasKey(ug => ug.ID).HasName("PK_SliderImg");
            modelBuilder.Entity<dbCustomer>().HasKey(ug => ug.id).HasName("PK_Customer");
            modelBuilder.Entity<dbCompanySeq>().HasKey(ug => ug.id).HasName("PK_Company_Seq");
            modelBuilder.Entity<dbCustomerSeq>().HasKey(ug => ug.id).HasName("PK_Cust_Seq");
            modelBuilder.Entity<dbAccount>().HasKey(ug => ug.id).HasName("PK_Account");
            modelBuilder.Entity<dbAccount>().HasKey(ug => ug.id).HasName("PK_Kas");

            //modelBuilder.Entity<dbSalesDtl>().HasKey(ug => new { ug.store_id, ug.invoice, ug.transdate, ug.article }).HasName("PKSalesdtl");


            // Configure indexes  
            //modelBuilder.Entity<UserGroup>().HasIndex(p => p.Name).IsUnique().HasDatabaseName("Idx_Name");  
            //modelBuilder.Entity<User>().HasIndex(u => u.FirstName).HasDatabaseName("Idx_FirstName");  
            //modelBuilder.Entity<User>().HasIndex(u => u.LastName).HasDatabaseName("Idx_LastName");  



            //Model Sequence
            modelBuilder.Entity<dbCustomerSeq>().Property(ug => ug.id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<dbCompanySeq>().Property(ug => ug.id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();

            //SystemTabTbl
            modelBuilder.Entity<SystemTabModel>().Property(ug => ug.ID).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<SystemTabModel>().Property(ug => ug.TAB_DESC).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<SystemTabModel>().Property(ug => ug.ROLE_ID).HasColumnType("varchar(75)").IsRequired(false);
            modelBuilder.Entity<SystemTabModel>().Property(ug => ug.TAB_TXT).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<SystemTabModel>().Property(ug => ug.FLAG_AKTIF).HasColumnType("varchar(1)").IsRequired(false);
            modelBuilder.Entity<SystemTabModel>().Property(ug => ug.ENTRY_USER).HasColumnType("varchar(50)").IsRequired(false);
            modelBuilder.Entity<SystemTabModel>().Property(ug => ug.UPDATE_USER).HasColumnType("varchar(60)").IsRequired();
            modelBuilder.Entity<SystemTabModel>().Property(ug => ug.ENTRY_DATE).HasColumnType("date");
            modelBuilder.Entity<SystemTabModel>().Property(ug => ug.UPDATE_DATE).HasColumnType("date");

            //SystemMenuTbl
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.ID).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.TAB_ID).HasColumnType("int");
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.MENU_DESC).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.ROLE_ID).HasColumnType("varchar(75)").IsRequired(false);
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.MENU_TXT).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.FLAG_AKTIF).HasColumnType("varchar(1)").IsRequired(false);
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.ENTRY_USER).HasColumnType("varchar(50)").IsRequired(false);
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.UPDATE_USER).HasColumnType("varchar(60)").IsRequired();
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.ENTRY_DATE).HasColumnType("date");
            modelBuilder.Entity<SystemMenuModel>().Property(ug => ug.UPDATE_DATE).HasColumnType("date");




            //SystemSlideTbl
            modelBuilder.Entity<dbSliderImg>().Property(ug => ug.ID).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<dbSliderImg>().Property(ug => ug.IMG_DESC).HasColumnType("int");
            modelBuilder.Entity<dbSliderImg>().Property(ug => ug.FILE_NAME).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbSliderImg>().Property(ug => ug.IMG_DESC).HasColumnType("MediumBlob").IsRequired(false);
            modelBuilder.Entity<dbSliderImg>().Property(ug => ug.FLAG_AKTIF).HasColumnType("varchar(1)").IsRequired(false);
            modelBuilder.Entity<dbSliderImg>().Property(ug => ug.ENTRY_USER).HasColumnType("varchar(50)").IsRequired(false);
            modelBuilder.Entity<dbSliderImg>().Property(ug => ug.UPDATE_USER).HasColumnType("varchar(60)").IsRequired();
            modelBuilder.Entity<dbSliderImg>().Property(ug => ug.ENTRY_DATE).HasColumnType("date");
            modelBuilder.Entity<dbSliderImg>().Property(ug => ug.UPDATE_DATE).HasColumnType("date");

            #region appmodel
            //CustomerTbl
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.EDP).HasColumnType("varchar(5)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.CUST_NAME).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.COMPANY).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.NPWP).HasColumnType("varchar(25)").IsRequired(false);
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.address).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.city).HasColumnType("varchar(80)").IsRequired(false);
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.province).HasColumnType("varchar(80)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.postal).HasColumnType("varchar(10)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.BANK_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.BANK_NUMBER).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.BANK_BRANCH).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.BANK_COUNTRY).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.REG_DATE).HasColumnType("date");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.BL_FLAG).HasColumnType("varchar(1)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.ENTRY_DATE).HasColumnType("date");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.UPDATE_DATE).HasColumnType("date");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.ENTRY_USER).HasColumnType("varchar(80)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.UPDATE_USER).HasColumnType("varchar(80)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FLAG_AKTIF).HasColumnType("varchar(1)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.Email).HasColumnType("varchar(100)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.KTP).HasColumnType("varchar(100)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.PHONE1).HasColumnType("varchar(100)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.PHONE2).HasColumnType("varchar(100)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.isApproved).HasColumnType("varchar(1)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.isApproved2).HasColumnType("varchar(1)");

            modelBuilder.Entity<dbCustomer>().Property(ug => ug.VA1).HasColumnType("varchar(100)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.VA1NOTE).HasColumnType("varchar(100)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.VA2).HasColumnType("varchar(100)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.VA2NOTE).HasColumnType("varchar(100)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_KTP).HasColumnType("MediumBlob");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_AKTA).HasColumnType("MediumBlob");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_REKENING).HasColumnType("MediumBlob");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_NPWP).HasColumnType("MediumBlob");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_TDP).HasColumnType("MediumBlob");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_SIUP).HasColumnType("MediumBlob");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_NIB).HasColumnType("MediumBlob");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_SPPKP).HasColumnType("MediumBlob");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_SKT).HasColumnType("MediumBlob");

            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_KTP_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_AKTA_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_REKENING_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_NPWP_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_TDP_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_SIUP_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_NIB_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_SPPKP_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.FILE_SKT_NAME).HasColumnType("varchar(255)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.store_area).HasColumnType("varchar(50)");
            modelBuilder.Entity<dbCustomer>().Property(ug => ug.discount_customer).HasColumnType("varchar(50)");

            //dbaccount model
            modelBuilder.Entity<dbAccount>().Property(ug => ug.id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<dbAccount>().Property(ug => ug.account_no).HasColumnType("int");
            modelBuilder.Entity<dbAccount>().Property(ug => ug.hierarchy).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbAccount>().Property(ug => ug.account_name).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbAccount>().Property(ug => ug.akundk).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbAccount>().Property(ug => ug.akunnrlr).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbAccount>().Property(ug => ug.flag_aktif).HasColumnType("varchar(1)").IsRequired(false);
            modelBuilder.Entity<dbAccount>().Property(ug => ug.account_Type).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbAccount>().Property(ug => ug.entry_user).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbAccount>().Property(ug => ug.update_user).HasColumnType("varchar(255)").IsRequired();
            modelBuilder.Entity<dbAccount>().Property(ug => ug.entry_date).HasColumnType("date");
            modelBuilder.Entity<dbAccount>().Property(ug => ug.update_date).HasColumnType("date");

            //dbkas model 
            modelBuilder.Entity<dbKas>().Property(ug => ug.id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<dbKas>().Property(ug => ug.TransDate).HasColumnType("date");
            modelBuilder.Entity<dbKas>().Property(ug => ug.Trans_no).HasColumnType("varchar(50)").IsRequired(false);
            modelBuilder.Entity<dbKas>().Property(ug => ug.Description).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbKas>().Property(ug => ug.Akun_Debit).HasColumnType("int");
            modelBuilder.Entity<dbKas>().Property(ug => ug.Akun_Credit).HasColumnType("int");
            modelBuilder.Entity<dbKas>().Property(ug => ug.Debit).HasColumnType("int(20)");
            modelBuilder.Entity<dbKas>().Property(ug => ug.Credit).HasColumnType("int(20)");
            modelBuilder.Entity<dbKas>().Property(ug => ug.Saldo).HasColumnType("int(20)");
            modelBuilder.Entity<dbKas>().Property(ug => ug.TransDateStr).HasColumnType("varchar(50)").IsRequired(false);
            modelBuilder.Entity<dbKas>().Property(ug => ug.MonthStr).HasColumnType("varchar(50)").IsRequired(false);
            modelBuilder.Entity<dbKas>().Property(ug => ug.YearStr).HasColumnType("varchar(50)").IsRequired(false);

            modelBuilder.Entity<dbKas>().Property(ug => ug.flag_aktif).HasColumnType("varchar(1)").IsRequired(false);
            modelBuilder.Entity<dbKas>().Property(ug => ug.entry_user).HasColumnType("varchar(255)").IsRequired(false);
            modelBuilder.Entity<dbKas>().Property(ug => ug.update_user).HasColumnType("varchar(255)").IsRequired();
            modelBuilder.Entity<dbKas>().Property(ug => ug.entry_date).HasColumnType("datetime");
            modelBuilder.Entity<dbKas>().Property(ug => ug.update_date).HasColumnType("datetime");

            #endregion appmodel





            //modelBuilder.Entity<VaksinModel>().Property(ug => ug.CreationDateTime).HasColumnType("datetime").IsRequired();  
            //modelBuilder.Entity<VaksinModel>().Property(ug => ug.LastUpdateDateTime).HasColumnType("datetime").IsRequired(false);  

            //modelBuilder.Entity<User>().Property(u => u.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();  
            //modelBuilder.Entity<User>().Property(u => u.FirstName).HasColumnType("nvarchar(50)").IsRequired();  
            //modelBuilder.Entity<User>().Property(u => u.LastName).HasColumnType("nvarchar(50)").IsRequired();  
            //modelBuilder.Entity<User>().Property(u => u.UserGroupId).HasColumnType("int").IsRequired();  
            //modelBuilder.Entity<User>().Property(u => u.CreationDateTime).HasColumnType("datetime").IsRequired();  
            //modelBuilder.Entity<User>().Property(u => u.LastUpdateDateTime).HasColumnType("datetime").IsRequired(false);  

            // Configure relationships  
            //modelBuilder.Entity<VaksinModel>().HasOne<XstoreModel>().WithMany().HasPrincipalKey(ug => ug.edp).HasForeignKey(u => u.EDP_CODE).OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_xStoredata_orcl");
            //modelBuilder.Entity<dbRekapTraining>().HasOne<dbTrainer>().WithMany().HasPrincipalKey(ug => ug.idTrainer).HasForeignKey(u => u.idTrainer).OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_idTrainerdt");
            //modelBuilder.Entity<dbRekapTraining>().HasOne<XstoreModel>().WithMany().HasPrincipalKey(ug => ug.edp).HasForeignKey(u => u.EDP).OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_xStoredata_orcl_2");

        }
    }
}

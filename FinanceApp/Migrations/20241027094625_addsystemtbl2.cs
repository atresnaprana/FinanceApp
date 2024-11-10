using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseLineProject.Migrations
{
    public partial class addsystemtbl2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dbCompany_seq",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company_Seq", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dbCustomer",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EDP = table.Column<string>(type: "varchar(5)", nullable: true),
                    CUST_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    COMPANY_ID = table.Column<string>(nullable: true),
                    COMPANY = table.Column<string>(type: "varchar(255)", nullable: true),
                    NPWP = table.Column<string>(type: "varchar(25)", nullable: true),
                    address = table.Column<string>(type: "varchar(255)", nullable: true),
                    city = table.Column<string>(type: "varchar(80)", nullable: true),
                    province = table.Column<string>(type: "varchar(80)", nullable: true),
                    postal = table.Column<string>(type: "varchar(10)", nullable: true),
                    BANK_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    BANK_NUMBER = table.Column<string>(type: "varchar(255)", nullable: true),
                    BANK_BRANCH = table.Column<string>(type: "varchar(255)", nullable: true),
                    BANK_COUNTRY = table.Column<string>(type: "varchar(255)", nullable: true),
                    REG_DATE = table.Column<DateTime>(type: "date", nullable: false),
                    BL_FLAG = table.Column<string>(type: "varchar(1)", nullable: true),
                    ENTRY_DATE = table.Column<DateTime>(type: "date", nullable: false),
                    UPDATE_DATE = table.Column<DateTime>(type: "date", nullable: false),
                    ENTRY_USER = table.Column<string>(type: "varchar(80)", nullable: true),
                    UPDATE_USER = table.Column<string>(type: "varchar(80)", nullable: true),
                    FLAG_AKTIF = table.Column<string>(type: "varchar(1)", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", nullable: true),
                    KTP = table.Column<string>(type: "varchar(100)", nullable: true),
                    PHONE1 = table.Column<string>(type: "varchar(100)", nullable: true),
                    PHONE2 = table.Column<string>(type: "varchar(100)", nullable: true),
                    isApproved = table.Column<string>(type: "varchar(1)", nullable: true),
                    VA1 = table.Column<string>(type: "varchar(100)", nullable: true),
                    VA2 = table.Column<string>(type: "varchar(100)", nullable: true),
                    VA1NOTE = table.Column<string>(type: "varchar(100)", nullable: true),
                    VA2NOTE = table.Column<string>(type: "varchar(100)", nullable: true),
                    FILE_KTP = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_AKTA = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_REKENING = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_NPWP = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_TDP = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_SIUP = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_NIB = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_SPPKP = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_SKT = table.Column<byte[]>(type: "MediumBlob", nullable: true),
                    FILE_KTP_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    FILE_AKTA_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    FILE_REKENING_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    FILE_NPWP_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    FILE_TDP_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    FILE_SIUP_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    FILE_NIB_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    FILE_SPPKP_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    FILE_SKT_NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    isApproved2 = table.Column<string>(type: "varchar(1)", nullable: true),
                    store_area = table.Column<string>(type: "varchar(50)", nullable: true),
                    discount_customer = table.Column<string>(type: "varchar(50)", nullable: true),
                    totalstoreconfig = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dbCustomer_seq",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cust_Seq", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dbCompany_seq");

            migrationBuilder.DropTable(
                name: "dbCustomer");

            migrationBuilder.DropTable(
                name: "dbCustomer_seq");
        }
    }
}

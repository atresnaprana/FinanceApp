namespace FinanceApp.Models.Enums
{
    public class TaxEnums
    {
        public enum TaxScheme
        {
            PPH_FINAL_UMKM = 1,   // 0.5% omzet
            PPH_BADAN = 2         // Laba kena pajak
        }

        public enum EntityType
        {
            PERORANGAN = 1,
            CV = 2,
            PT = 3
        }

        public enum TaxPeriodStatus
        {
            UMKM_ACTIVE = 1,
            UMKM_EXPIRED = 2,
            NON_UMKM = 3
        }
    }
}

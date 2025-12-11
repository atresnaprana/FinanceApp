using BaseLineProject.Data;
using FinanceApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceApp.Services
{
    public interface IFiscalService
    {
        List<FiscalAdjustmentViewModel> GetMap(string companyId);
        void Save(string companyId, int accountNo, string type, string reason, string user);
        string ResolveFiscalType(string companyId, int accountNo);
    }

    public class FiscalService : IFiscalService
    {
        private readonly FormDBContext _context;

        public FiscalService(FormDBContext db)
        {
            _context = db;
        }

        public List<FiscalAdjustmentViewModel> GetMap(string companyId)
        {
            var accountList = _context.AccountTbl
                .Where(x => x.company_id == companyId)
                .OrderBy(x => x.account_no)
                .ToList();

            var adj = _context.FiscalAdjustment
                .Where(x => x.company_id == companyId)
                .ToDictionary(x => x.account_no);

            List<FiscalAdjustmentViewModel> result = new List<FiscalAdjustmentViewModel>();

            foreach (var acc in accountList)
            {
                adj.TryGetValue(acc.account_no, out var overrideRec);

                result.Add(new FiscalAdjustmentViewModel
                {
                    AccountNo = acc.account_no,
                    AccountName = acc.account_name,
                    DefaultFiscalType = acc.fiscal_type,
                    OverrideFiscalType = overrideRec?.override_fiscal_type,
                    Reason = overrideRec?.reason
                });
            }

            return result;
        }

        public void Save(string companyId, int accountNo, string type, string reason, string user)
        {
            var existing = _context.FiscalAdjustment
                .FirstOrDefault(x => x.company_id == companyId && x.account_no == accountNo);

            if (existing == null)
            {
                existing = new dbFiscalAdjustment
                {
                    company_id = companyId,
                    account_no = accountNo,
                    override_fiscal_type = type,
                    reason = reason,
                    entry_user = user,
                    update_user = user,
                    entry_date = DateTime.Now,
                    update_date = DateTime.Now,
                    flag_aktif = "Y"
                };
                _context.FiscalAdjustment.Add(existing);
            }
            else
            {
                existing.override_fiscal_type = type;
                existing.reason = reason;
                existing.update_user = user;
                existing.update_date = DateTime.Now;
            }

            _context.SaveChanges();
        }

        public string ResolveFiscalType(string companyId, int accNo)
        {
            var adj = _context.FiscalAdjustment
                .FirstOrDefault(x => x.company_id == companyId && x.account_no == accNo);

            if (adj != null)
                return adj.override_fiscal_type;

            var acc = _context.AccountTbl
                .FirstOrDefault(x => x.company_id == companyId && x.account_no == accNo);

            return acc?.fiscal_type ?? "DEDUCTIBLE";
        }
    }
}

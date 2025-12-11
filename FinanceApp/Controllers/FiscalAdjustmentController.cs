using Microsoft.AspNetCore.Mvc;
using BaseLineProject.Data;
using FinanceApp.Models;
using System;
using System.Linq;
using System.Collections.Generic; // Required for List
using Microsoft.AspNetCore.Authorization; // Assuming you need authorization

public class FiscalAdjustmentController : Controller
{
    private readonly FormDBContext _context;

    public FiscalAdjustmentController(FormDBContext ctx)
    {
        _context = ctx;
    }

    // -----------------------------------
    // INDEX: Show all accounts and their override status
    // -----------------------------------
    [Authorize]
    public IActionResult Index()
    {
        // Note: You will need to get the user's company ID here.
        // For now, I will use a placeholder. Replace "user_company_id" with your actual logic.
        var datas = _context.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
        string companyId = datas.COMPANY_ID; // <-- TODO: REPLACE WITH YOUR LOGIC

        var accounts = _context.AccountTbl
            .Where(a => a.company_id == companyId && a.flag_aktif == "1")
            .ToList();

        var adjustments = _context.FiscalAdjustment
            .Where(f => f.company_id == companyId && f.flag_aktif == "1")
            .ToList();

        var model = (
            from acc in accounts
            join adj in adjustments on acc.account_no equals adj.account_no into gj
            from x in gj.DefaultIfEmpty() // This performs a LEFT JOIN
            select new FiscalAdjustmentViewModel
            {
                id = (x == null) ? 0 : x.id, // Pass the adjustment ID if it exists
                AccountNo = acc.account_no,
                AccountName = acc.account_name,
                DefaultFiscalType = acc.fiscal_type,
                OverrideFiscalType = x?.override_fiscal_type,
                Reason = x?.reason,
                CompanyId = acc.company_id
            }
        ).OrderBy(a => a.AccountNo).ToList();

        return View(model);
    }


    // -----------------------------------
    // CREATE - GET
    // -----------------------------------
    [Authorize]
    public IActionResult Create(int accountNo, string companyId)
    {
        var acc = _context.AccountTbl
            .FirstOrDefault(a => a.account_no == accountNo && a.company_id == companyId);

        if (acc == null) return NotFound();

        var model = new FiscalAdjustmentViewModel
        {
            AccountNo = accountNo,
            AccountName = acc.account_name,
            DefaultFiscalType = acc.fiscal_type,
            CompanyId = companyId
        };

        return View(model);
    }

    // -----------------------------------
    // CREATE - POST
    // -----------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Create(FiscalAdjustmentViewModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = new dbFiscalAdjustment
            {
                company_id = model.CompanyId, // Pass the company ID
                account_no = model.AccountNo,
                override_fiscal_type = model.OverrideFiscalType,
                reason = model.Reason,
                entry_user = User.Identity.Name,
                update_user = User.Identity.Name,
                entry_date = DateTime.Now,
                update_date = DateTime.Now,
                flag_aktif = "1"
            };

            _context.FiscalAdjustment.Add(entity);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        return View(model);
    }


    // -----------------------------------
    // EDIT - GET
    // -----------------------------------
    [Authorize]
    public IActionResult Edit(int id)
    {
        var entity = _context.FiscalAdjustment.FirstOrDefault(x => x.id == id);
        if (entity == null) return NotFound();

        var acc = _context.AccountTbl
            .FirstOrDefault(a => a.account_no == entity.account_no && a.company_id == entity.company_id);

        if (acc == null) return NotFound();

        var model = new FiscalAdjustmentViewModel
        {
            id = entity.id,
            AccountNo = entity.account_no,
            AccountName = acc.account_name,
            DefaultFiscalType = acc.fiscal_type,
            OverrideFiscalType = entity.override_fiscal_type,
            Reason = entity.reason,
            CompanyId = entity.company_id
        };

        return View(model);
    }

    // -----------------------------------
    // EDIT - POST
    // -----------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Edit(FiscalAdjustmentViewModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _context.FiscalAdjustment.FirstOrDefault(x => x.id == model.id);
            if (entity == null) return NotFound();

            entity.override_fiscal_type = model.OverrideFiscalType;
            entity.reason = model.Reason;
            entity.update_user = User.Identity.Name;
            entity.update_date = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        return View(model);
    }


    // -----------------------------------
    // DELETE - GET (Confirmation Page)
    // -----------------------------------
    [Authorize]
    public IActionResult Delete(int id)
    {
        var entity = _context.FiscalAdjustment.FirstOrDefault(x => x.id == id);
        if (entity == null) return NotFound();

        // You can pass the entity directly to the delete view
        return View(entity);
    }

    // -----------------------------------
    // DELETE - POST (Perform Soft Delete)
    // -----------------------------------
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult DeleteConfirmed(int id)
    {
        var entity = _context.FiscalAdjustment.FirstOrDefault(x => x.id == id);
        if (entity == null) return NotFound();

        entity.flag_aktif = "0"; // Soft delete
        entity.update_user = User.Identity.Name;
        entity.update_date = DateTime.Now;

        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}
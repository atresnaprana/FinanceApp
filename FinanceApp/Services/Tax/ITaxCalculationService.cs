using FinanceApp.Models;
using MimeDetective.Storage.Xml.v2;
using OfficeOpenXml.DataValidation;
using System;
using System.Collections.Generic;

namespace FinanceApp.Services.Tax
{
    public interface ITaxCalculationService
    {
        TaxSummaryResult CalculateAnnualTax(
            string companyId,
            int year,
            IEnumerable<dbJpn> jpn,
            IEnumerable<dbJpb> jpb,
            IEnumerable<dbJm> jm,
            string taxFlagPercentage,
            DateTime registrationDate,
            string customertype
        );
    }
}

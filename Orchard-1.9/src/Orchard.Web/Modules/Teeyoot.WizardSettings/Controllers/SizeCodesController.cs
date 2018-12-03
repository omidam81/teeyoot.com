using Orchard;
using Orchard.Data;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teeyoot.Module.Models;

namespace Teeyoot.WizardSettings.Controllers
{
    public class SizeCodesController : Controller
    {

        private readonly ISiteService _siteService;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<ProductColorRecord> _productColourRepository;
        private readonly IRepository<SwatchRecord> _swatchColourRepository;
        private readonly IRepository<SizeCodeRecord> _sizesRecord;

        private readonly IWorkContextAccessor _workContextAccessor;
        private string cultureUsed = string.Empty;

        private dynamic Shape { get; set; }

        public SizeCodesController(ISiteService siteService, IOrchardServices OrchardService, IRepository<ProductColorRecord> ProductColorRecord, IRepository<SwatchRecord> SwatchRecord, IRepository<SizeCodeRecord> ProductSizeRecords)
        {
            _sizesRecord = ProductSizeRecords;
            _siteService = siteService;
            _productColourRepository = ProductColorRecord;
            _swatchColourRepository = SwatchRecord;
            _orchardServices = OrchardService;

        }
        // GET: SizeCodes
        public ActionResult Index()
        {
            return View("Index", _sizesRecord.Table);
        }

        public ActionResult Edit(int id)
        {
            return View("Edit", _sizesRecord.Table.FirstOrDefault(aa => aa.Id == id));
        }

        [HttpPost]
        public ActionResult Edit(SizeCodeRecord record)
        {
            _sizesRecord.Update(record);
            return RedirectToAction("Index");
        }



        public ActionResult Delete(int id)
        {
            var size = _sizesRecord.Table.FirstOrDefault(aa => aa.Id == id);
            _sizesRecord.Delete(size);
            return RedirectToAction("Index");
        }

        public ActionResult Add()
        {
            return View("Add");
        }

        [HttpPost]

        public ActionResult Add(SizeCodeRecord size)
        {
            _sizesRecord.Create(size);
            return RedirectToAction("Index");
        }
    }
}
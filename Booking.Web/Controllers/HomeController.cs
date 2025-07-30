using AspNetCoreGeneratedDocument;
using Booking.Application.Common.Interfaces;
using Booking.Application.Common.Utility;
using Booking.Application.Services.Interface;
using Booking.Web.Models;
using Booking.Web.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.DocIO.DLS;
using Syncfusion.Presentation;
using System.ComponentModel;
using ListType = Syncfusion.Presentation.ListType;

namespace Booking.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IVillaService villaService, IWebHostEnvironment webHostEnvironment)
        {
            _villaService = villaService;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int page = 1, string? searchTerm = null, double? minPrice = null, 
                                  double? maxPrice = null, int? minOccupancy = null, int? maxOccupancy = null)
        {
            //return BadRequest();
            //return StatusCode(500);
            //return NotFound();
            //return StatusCode(403);
            // return error page if the page number is invalid
            if (page < 1)
            {
                return RedirectToAction(nameof(Error));
            }
            IEnumerable<Domain.Entities.Villa> villas;

            if (!string.IsNullOrEmpty(searchTerm) || minPrice.HasValue || maxPrice.HasValue || 
                minOccupancy.HasValue || maxOccupancy.HasValue)
            {
                villas = _villaService.SearchVillas(searchTerm, minPrice, maxPrice, minOccupancy, maxOccupancy);
            }
            else
            {
                villas = _villaService.GetAllVillas();
            }

            var paginatedVillas = PaginatedList<Domain.Entities.Villa>.Create(villas, page, SD.PageSize);

            HomeVM homeVM = new()
            {
                VillaList = paginatedVillas,
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                CurrentPage = page,
                PageSize = SD.PageSize,
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinOccupancy = minOccupancy,
                MaxOccupancy = maxOccupancy
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate, int page = 1,
                                           string? searchTerm = null, double? minPrice = null, 
                                           double? maxPrice = null, int? minOccupancy = null, int? maxOccupancy = null)
        {
            IEnumerable<Domain.Entities.Villa> villas;

            // Always apply search filters first, then check availability
            if (!string.IsNullOrEmpty(searchTerm) || minPrice.HasValue || maxPrice.HasValue || 
                minOccupancy.HasValue || maxOccupancy.HasValue)
            {
                villas = _villaService.SearchVillasWithAvailability(searchTerm, minPrice, maxPrice, 
                                                                   minOccupancy, maxOccupancy, nights, checkInDate);
            }
            else
            {
                villas = _villaService.GetVillasAvailabilityByDate(nights, checkInDate);
            }

            var paginatedVillas = PaginatedList<Domain.Entities.Villa>.Create(villas, page, SD.PageSize);

            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = paginatedVillas,
                Nights = nights,
                CurrentPage = page,
                PageSize = SD.PageSize,
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinOccupancy = minOccupancy,
                MaxOccupancy = maxOccupancy
            };

            return PartialView("_VillaList", homeVM);
        }

        [HttpGet]
        public IActionResult LoadPage(int page, int nights, DateOnly? checkInDate,
                                     string? searchTerm = null, double? minPrice = null, 
                                     double? maxPrice = null, int? minOccupancy = null, int? maxOccupancy = null)
        {
            HomeVM homeVM;
            IEnumerable<Domain.Entities.Villa> villas;

            if (checkInDate.HasValue && checkInDate > DateOnly.FromDateTime(DateTime.Now))
            {
                // Load filtered villas by date with search
                if (!string.IsNullOrEmpty(searchTerm) || minPrice.HasValue || maxPrice.HasValue || 
                    minOccupancy.HasValue || maxOccupancy.HasValue)
                {
                    villas = _villaService.SearchVillasWithAvailability(searchTerm, minPrice, maxPrice, 
                                                                       minOccupancy, maxOccupancy, nights, checkInDate.Value);
                }
                else
                {
                    villas = _villaService.GetVillasAvailabilityByDate(nights, checkInDate.Value);
                }

                var paginatedVillas = PaginatedList<Domain.Entities.Villa>.Create(villas, page, SD.PageSize);

                homeVM = new()
                {
                    CheckInDate = checkInDate.Value,
                    VillaList = paginatedVillas,
                    Nights = nights,
                    CurrentPage = page,
                    PageSize = SD.PageSize,
                    SearchTerm = searchTerm,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    MinOccupancy = minOccupancy,
                    MaxOccupancy = maxOccupancy
                };
            }
            else
            {
                // Load all villas with search
                if (!string.IsNullOrEmpty(searchTerm) || minPrice.HasValue || maxPrice.HasValue || 
                    minOccupancy.HasValue || maxOccupancy.HasValue)
                {
                    villas = _villaService.SearchVillas(searchTerm, minPrice, maxPrice, minOccupancy, maxOccupancy);
                }
                else
                {
                    villas = _villaService.GetAllVillas();
                }

                var paginatedVillas = PaginatedList<Domain.Entities.Villa>.Create(villas, page, SD.PageSize);

                homeVM = new()
                {
                    VillaList = paginatedVillas,
                    Nights = nights,
                    CheckInDate = checkInDate ?? DateOnly.FromDateTime(DateTime.Now),
                    CurrentPage = page,
                    PageSize = SD.PageSize,
                    SearchTerm = searchTerm,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    MinOccupancy = minOccupancy,
                    MaxOccupancy = maxOccupancy
                };
            }

            return PartialView("_VillaList", homeVM);
        }

        [HttpPost]
        public IActionResult SearchVillas(HomeVM model)
        {
            model.CurrentPage = 1; // Reset to first page on new search
            
            IEnumerable<Domain.Entities.Villa> villas;

            if (model.CheckInDate > DateOnly.FromDateTime(DateTime.Now))
            {
                villas = _villaService.SearchVillasWithAvailability(model.SearchTerm, model.MinPrice, model.MaxPrice,
                                                                   model.MinOccupancy, model.MaxOccupancy, 
                                                                   model.Nights, model.CheckInDate);
            }
            else
            {
                villas = _villaService.SearchVillas(model.SearchTerm, model.MinPrice, model.MaxPrice,
                                                   model.MinOccupancy, model.MaxOccupancy);
            }

            var paginatedVillas = PaginatedList<Domain.Entities.Villa>.Create(villas, 1, SD.PageSize);
            model.VillaList = paginatedVillas;

            return PartialView("_VillaList", model);
        }

        // Add new action to check availability with current search filters
        [HttpPost]
        public IActionResult CheckAvailabilityWithFilters(HomeVM model)
        {
            model.CurrentPage = 1; // Reset to first page

            // Always check availability with current search filters
            IEnumerable<Domain.Entities.Villa> villas = _villaService.SearchVillasWithAvailability(
                model.SearchTerm, model.MinPrice, model.MaxPrice,
                model.MinOccupancy, model.MaxOccupancy, 
                model.Nights, model.CheckInDate);

            var paginatedVillas = PaginatedList<Domain.Entities.Villa>.Create(villas, 1, SD.PageSize);
            model.VillaList = paginatedVillas;

            return PartialView("_VillaList", model);
        }

        [HttpGet]
        public IActionResult GeneratePPTExport(int Id)
        {
            var villa = _villaService.GetVillaById(Id);
            if (villa is null)
            {
                return RedirectToAction(nameof(Error));
            }

            string basePath = _webHostEnvironment.WebRootPath;
            string filePath = basePath + @"/Exports/ExportVillaDetails.pptx";

            using IPresentation presentation = Presentation.Open(filePath);

            ISlide slide = presentation.Slides[0];

            IShape? shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaName") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaDescription") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Description;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtOccupancy") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy : {0} adults", villa.Occupancy);
            }
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaSize") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Villa Size: {0} sqft", villa.SquareFeet);
            }
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtPricePerNight") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("USD {0}/night", villa.Price.ToString("C"));
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if (shape is not null)
            {
                List<string> listItems = villa.VillaAmenity.Select(x => x.Name).ToList();

                shape.TextBody.Text = "";

                foreach (var item in listItems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);

                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);
                }
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "imgVilla") as IShape;
            if (shape is not null)
            {
                byte[] imageData;
                string imageUrl;
                try
                {
                    imageUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                catch (Exception)
                {
                    imageUrl = string.Format("{0}{1}", basePath, "/images/placeholder.png");
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                slide.Shapes.Remove(shape);
                using MemoryStream imageStream = new(imageData);
                IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);
            }

            MemoryStream memoryStream = new();
            presentation.Save(memoryStream);
            memoryStream.Position = 0;
            return File(memoryStream, "application/pptx", "villa.pptx");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

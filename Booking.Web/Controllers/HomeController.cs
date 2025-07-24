using Booking.Application.Common.Interfaces;
using Booking.Application.Common.Utility;
using Booking.Web.Models;
using Booking.Web.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.DocIO.DLS;
using Syncfusion.Presentation;
using System.ComponentModel;

namespace Booking.Web.Controllers
{
    public class HomeController : Controller
    {
        //Implement IWEbHostEnvironment and inject it into the constructor
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var homeVM = new HomeVM
            {
                VillaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity"),
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                CheckOutDate = null,
                Nights = 1
            };
            return View(homeVM);
        }
        //[HttpPost]
        //public IActionResult Index(HomeVM homeVM)
        //{
        //    homeVM.VillaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity");
        //    foreach (var villa in homeVM.VillaList)
        //    {
        //        if (villa.Id%2==0)
        //        {
        //            villa.IsAvailable = false;
        //        }
        //    }
        //    return View(homeVM);
        //}
        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var VillaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "VillaAmenity");
            var villaNumbersList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(u=>u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();
            foreach (var villa in VillaList)
            {
                villa.IsAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbersList, checkInDate, nights, bookedVillas) > 0;
            }
            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = VillaList,
                Nights = nights
            };
            //Thread.Sleep(2000); // Simulate a delay for demonstration purposes
            return PartialView("_VillaList", homeVM);
        }
        [HttpPost]
        public IActionResult GeneratePPTExport(int id)
        {
            var villa = _unitOfWork.VillaRepository.Get(u => u.Id == id, includeProperties: "VillaAmenity");
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

                    paragraph.ListFormat.Type = Syncfusion.Presentation.ListType.Bulleted;
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
        public IActionResult Error()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movies.Models;
using Movies.Viewmodels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Movies.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _Context;

        public MoviesController(ApplicationDbContext context)
        {
            _Context = context;
        }

        public async Task<IActionResult> Index()
        {
            var Movies = await _Context.Movies.ToListAsync();
            return View(Movies);
        }
        public async Task<IActionResult> Create()
        {
            var Viewmodel = new MovieFormViewModel
            {
                Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync(),
            };
            return View(Viewmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();

                return View(model);

            }
            var File = Request.Form.Files;
            if (!File.Any())
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("poster", "Please select poster");

                return View(model);
            }
            var poster = File.FirstOrDefault();
            var allowdExtention = new List<string> { ".jpg", ".png" };
            if (!allowdExtention.Contains(Path.GetExtension(poster.FileName).ToLower())) {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("poster", "only .png .jpg");

                return View(model);
            };

            if (poster.Length > 1048576)
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("poster", "1 mega byte");

                return View(model);
            }
            using var dataStream = new MemoryStream();
            await poster.CopyToAsync(dataStream);
            var Movies = new Movie
            {
                Title = model.Title,
                 Year=model.Year,
                StoryLine=model.StoryLine,
                poster=dataStream.ToArray(),
                GenreId=model.GenreId
            };
           await _Context.AddAsync(Movies);
            await _Context.SaveChangesAsync();
            return RedirectToAction("index");
            
        }
    }
}

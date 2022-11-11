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
        //private readonly IToastNotification _toastNotification;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;

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
            return View("MovieForm", Viewmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();

                return View("MovieForm",model);

            }
            var File = Request.Form.Files;
            if (!File.Any())
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("poster", "Please select poster");

                return View("MovieForm",model);
            }
            var poster = File.FirstOrDefault();
            var allowdExtention = new List<string> { ".jpg", ".png" };
            if (!allowdExtention.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("poster", "only .png .jpg");

                return View("MovieForm",model);
            };

            if (poster.Length > 1048576)
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("poster", "1 mega byte");

                return View("MovieForm",model);
            }
            using var dataStream = new MemoryStream();
            await poster.CopyToAsync(dataStream);
            var Movies = new Movie
            {
                Title = model.Title,
                Year = model.Year,
                StoryLine = model.StoryLine,
                poster = dataStream.ToArray(),
                GenreId = model.GenreId
                ,
                Rate = model.Rate,
            };
            await _Context.AddAsync(Movies);
            await _Context.SaveChangesAsync();
            return RedirectToAction("MovieForm");

        }
       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var Movie = await _Context.Movies.FindAsync(id);
            if (Movie == null)
                return NotFound();
            var ViewModel = new MovieFormViewModel
            {
                Id = Movie.Id,
                Title = Movie.Title,
                Year = Movie.Year,
                StoryLine = Movie.StoryLine,
                poster = Movie.poster,
                GenreId = Movie.GenreId,
                Rate = Movie.Rate,
                Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync(),

            };
            return View("MovieForm", ViewModel);
         }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
       
        public async Task<IActionResult> Edit(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("MovieForm", model);
            }

            var movie = await _Context.Movies.FindAsync(model.Id);

            if (movie == null)
                return NotFound();

            var files = Request.Form.Files;

            if (files.Any())
            {
                var poster = files.FirstOrDefault();

                using var dataStream = new MemoryStream();

                await poster.CopyToAsync(dataStream);

                model.poster = dataStream.ToArray();

                if (!_allowedExtenstions.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Only .PNG, .JPG images are allowed!");
                    return View("MovieForm", model);
                }

                if (poster.Length > _maxAllowedPosterSize)
                {
                    model.Genres = await _Context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                    return View("MovieForm", model);
                }

                movie.poster = model.poster;
            }

            movie.Title = model.Title;
            movie.GenreId = model.GenreId;
            movie.Year = model.Year;
            movie.Rate = model.Rate;
            movie.StoryLine = model.StoryLine;

            _Context.SaveChanges();

            //_toastNotification.AddSuccessToastMessage("Movie updated successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();
            var movie = await _Context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);
            if (movie == null)
                return NotFound();
           
            return View(movie);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _Context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            _Context.Movies.Remove(movie);
            _Context.SaveChanges();

            return Ok();
        }
    }
}
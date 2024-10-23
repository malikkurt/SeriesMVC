using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SeriesMvc.Data;
using SeriesMvc.Models;


namespace SeriesMvc.Controllers
{
    public class MoviesController : Controller
    {
        private readonly SeriesMvcContext _context;

        public MoviesController(SeriesMvcContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string movieActor, string movieCategory, string searchString)
        {
            // Tüm filmleri, aktörler ve kategorilerle birlikte çek
            var movies = await _context.Movie
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
                .ToListAsync();

            // Filtreleri uygula
            if (!string.IsNullOrEmpty(movieActor))
            {
                movies = movies.Where(m => m.MovieActors.Any(ma => ma.Actor.Name == movieActor)).ToList();
            }

            if (!string.IsNullOrEmpty(movieCategory))
            {
                movies = movies.Where(m => m.MovieCategories.Any(mc => mc.Category.Name == movieCategory)).ToList();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title!.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            // ViewModel oluştur
            var movieViewModels = movies.Select(movie => new MovieActorCategoryViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Actors = movie.MovieActors.Select(ma => ma.Actor.Name).ToList(),
                Categories = movie.MovieCategories.Select(mc => mc.Category.Name).ToList()
            }).ToList();

            return View(movieViewModels);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieActorCategoryViewModel model)
        {
            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Title == model.Title);

            if (movie == null)
            {
                movie = new Movie { Title = model.Title };
                _context.Movie.Add(movie);
            }

            foreach (var actorNames in model.Actors)
            {
                var actorList = actorNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var actorName in actorList)
                {
                    var trimmedActorName = actorName.Trim();

                    var actor = await _context.Actor.FirstOrDefaultAsync(a => a.Name == trimmedActorName);
                    if (actor == null)
                    {
                        actor = new Actor { Name = trimmedActorName };
                        _context.Actor.Add(actor);
                    }

                    if (!_context.MovieActor.Any(ma => ma.MovieId == movie.MovieId && ma.ActorId == actor.ActorId))
                    {
                        _context.MovieActor.Add(new MovieActor
                        {
                            Movie = movie,
                            Actor = actor
                        });
                    }
                }
            }

            foreach (var categoryNames in model.Categories)
            {
                var categoryList = categoryNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var categoryName in categoryList)
                {
                    var trimmedcategoryName = categoryName.Trim();

                    var category = await _context.Category.FirstOrDefaultAsync(a => a.Name == trimmedcategoryName);
                    if (category == null)
                    {
                        category = new Category { Name = trimmedcategoryName };
                        _context.Category.Add(category);
                    }

                    if (!_context.MovieCategory.Any(mc => mc.MovieId == movie.MovieId && mc.CategoryId == category.CategoryId))
                    {
                        _context.MovieCategory.Add(new MovieCategory
                        {
                            Movie = movie,
                            Category = category
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieId,Title")] Movie movie)
        {
            if (id != movie.MovieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.MovieId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.MovieId == id);
        }
    }
}

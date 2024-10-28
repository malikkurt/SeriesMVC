using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SeriesMvc.Data;
using SeriesMvc.Models;
using SeriesMvc.Services;


namespace SeriesMvc.Controllers
{
    public class MoviesController : Controller
    {
        private readonly SeriesMvcContext _context;
        private readonly ICacheService _cacheService;

        public MoviesController(SeriesMvcContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string movieActor, string movieCategory, string searchString)
        {
            var cacheKey = $"movies_{movieActor}_{movieCategory}_{searchString}"; // Cache anahtarı
            var cachedMovies = await _cacheService.GetAsync<List<MovieActorCategoryViewModel>>(cacheKey);

            if (cachedMovies != null)
            {
                return View(cachedMovies);
            }

            var movies = await _context.Movie
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
                .ToListAsync();

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

            var movieViewModels = movies.Select(movie => new MovieActorCategoryViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Actors = movie.MovieActors.Select(ma => ma.Actor.Name).ToList(),
                Categories = movie.MovieCategories.Select(mc => mc.Category.Name).ToList()
            }).ToList();

            await _cacheService.SetAsync(cacheKey, movieViewModels, TimeSpan.FromMinutes(3)); // Cache'e ekleme

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
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            var movieViewModel = new MovieActorCategoryViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Actors = movie.MovieActors.Select(ma => ma.Actor.Name).ToList(),
                Categories = movie.MovieCategories.Select(mc => mc.Category.Name).ToList()
            };

            return View(movieViewModel);
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
        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            var movieViewModel = new MovieActorCategoryViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Actors = movie.MovieActors.Select(ma => ma.Actor.Name).ToList(),
                Categories = movie.MovieCategories.Select(mc => mc.Category.Name).ToList()
            };

            return View(movieViewModel);
        }


        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieActorCategoryViewModel model)
        {
            if (id != model.MovieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var movie = await _context.Movie
                    .Include(m => m.MovieActors)
                    .Include(m => m.MovieCategories)
                    .FirstOrDefaultAsync(m => m.MovieId == id);

                if (movie == null)
                {
                    return NotFound();
                }

                movie.Title = model.Title;

                
                movie.MovieActors.Clear(); 
                foreach (var actorName in model.Actors)
                {
                    var trimmedActorName = actorName.Trim();

                    var actor = await _context.Actor.FirstOrDefaultAsync(a => a.Name == trimmedActorName);
                    if (actor == null)
                    {
                        actor = new Actor { Name = trimmedActorName };
                        _context.Actor.Add(actor);
                    }

                    movie.MovieActors.Add(new MovieActor
                    {
                        Movie = movie,
                        Actor = actor
                    });
                }

                movie.MovieCategories.Clear(); 
                foreach (var categoryName in model.Categories)
                {
                    var trimmedCategoryName = categoryName.Trim();

                    var category = await _context.Category.FirstOrDefaultAsync(c => c.Name == trimmedCategoryName);
                    if (category == null)
                    {
                        category = new Category { Name = trimmedCategoryName };
                        _context.Category.Add(category);
                    }

                    movie.MovieCategories.Add(new MovieCategory
                    {
                        Movie = movie,
                        Category = category
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
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

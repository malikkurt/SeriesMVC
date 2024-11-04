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

        private readonly ILogger<MoviesController> _logger;

        public MoviesController(SeriesMvcContext context, ICacheService cacheService, ILogger<MoviesController> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string movieActor, string movieCategory, string searchString)
        {

            _logger.LogInformation("Movies Index action called with parameters: actor={actor}, category={category}, search={search}", movieActor, movieCategory, searchString);

            var cacheKey = $"movies_{movieActor}_{movieCategory}_{searchString}";
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

            await _cacheService.SetAsync(cacheKey, movieViewModels, TimeSpan.FromMinutes(3));
            _logger.LogInformation("Movies stored in cache with key {cacheKey}", cacheKey);

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

            _logger.LogInformation("Movie '{title}' open detail screen successfully. ", movie.Title);
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
            _logger.LogInformation("Create action called with movie title: {title}", model.Title);

            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Title == model.Title);

            if (movie == null)
            {
                _logger.LogInformation("Movie not found, creating new movie with title: {title}", model.Title);
                movie = new Movie { Title = model.Title };
                _context.Movie.Add(movie);
            }
            else
            {
                _logger.LogWarning("Movie with title '{title}' already exists.", model.Title);
                return RedirectToAction(nameof(Index)); 
            }

            var actors = model.Actors?.Select(a => a.Trim()).Where(a => !string.IsNullOrEmpty(a)).ToList() ?? new List<string>();
            foreach (var actorName in actors)
            {
                var actor = await _context.Actor.FirstOrDefaultAsync(a => a.Name == actorName);
                if (actor == null)
                {
                    actor = new Actor { Name = actorName };
                    _context.Actor.Add(actor); 
                }

                movie.MovieActors.Add(new MovieActor
                {
                    Movie = movie,
                    Actor = actor
                });
                _logger.LogInformation("Actor '{actorName}' added to movie '{title}'.", actorName, model.Title);
            }

            var categories = model.Categories?.Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList() ?? new List<string>();
            foreach (var categoryName in categories)
            {
                var category = await _context.Category.FirstOrDefaultAsync(c => c.Name == categoryName);
                if (category == null)
                {
                    category = new Category { Name = categoryName };
                    _context.Category.Add(category); 
                }

                movie.MovieCategories.Add(new MovieCategory
                {
                    Movie = movie,
                    Category = category
                });
                _logger.LogInformation("Category '{categoryName}' added to movie '{title}'.", categoryName, model.Title);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Movie '{title}' created successfully with associated actors and categories.", model.Title);

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
            _logger.LogInformation("Movie '{title}' edited successfully. ", movie.Title);

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
            _logger.LogInformation("Movie '{title}' deleted successfully. ", movie.Title);
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.MovieId == id);
        }

        // GET: Movies/GetActors
        public async Task<IActionResult> GetActors(string query)
        {
            var actors = await _context.Actor
                .Where(a => a.Name.Contains(query))
                .Select(a => a.Name)
                .ToListAsync();

            return Json(actors);
        }

        // GET: Movies/GetCategories
        public async Task<IActionResult> GetCategories(string query)
        {
            var categories = await _context.Category
                .Where(c => c.Name.Contains(query))
                .Select(c => c.Name)
                .ToListAsync();

            return Json(categories);
        }


    }
}

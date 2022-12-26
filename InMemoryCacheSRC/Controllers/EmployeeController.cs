using InMemoryCacheSRC.Models;
using InMemoryCacheSRC.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace InMemoryCacheSRC.Controllers
{
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private const string empListCacheKey = "empList";
        private readonly IDataRepository<Employee> _empRepo;
        private IMemoryCache _cache;
        private ILogger<EmployeeController> _logger;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public EmployeeController(IDataRepository<Employee> empRepo, IMemoryCache cache, ILogger<EmployeeController> logger)
        {
            _empRepo=empRepo ?? throw new ArgumentNullException(nameof(empRepo));
            _cache = cache?? throw new ArgumentNullException(nameof(cache));
            _logger= logger?? throw new ArgumentNullException(nameof(logger));
        }
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            _logger.Log(LogLevel.Information, " Trying to fetch the list of employees from cache");

            if(_cache.TryGetValue(empListCacheKey, out IEnumerable<Employee> emps))
            {
                _logger.Log(LogLevel.Information, "Employee list found in cache");
            }
            else
            {
                try
                {
                    await semaphore.WaitAsync();
                    if (_cache.TryGetValue(empListCacheKey, out emps))
                        _logger.Log(LogLevel.Information, "Employee list found in cache");
                    else
                    {
                        _logger.Log(LogLevel.Information, "Employee list not found in cache. Fetching from DB");
                        emps = _empRepo.GetAll();
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                                                .SetSlidingExpiration(TimeSpan.FromSeconds(60)) // how long a cache entry can be inactiva before it is removed from the cache
                                                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600)) // making sire that the cache entry expires by an absolute time irrespective of whether it is still active or not
                                                .SetPriority(CacheItemPriority.Normal) // when server try to free up the memory, the priority will determone if it will be removed from the cache
                                                .SetSize(1024);

                        _cache.Set(empListCacheKey, emps, cacheEntryOptions);
                    }                    
                }
                finally
                {
                    semaphore.Release();
                }
                
            }
            return Ok(emps);
        }

        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {
            Employee employee = _empRepo.Get(id);
            if(employee == null)
                return NotFound("The employee record coouldn't be found");

            return Ok(employee);
        }

        [HttpPost]
        public IActionResult Add([FromBody]Employee emp)
        {
            if(emp == null)
                return BadRequest("Employee is null");

            _empRepo.Add(emp);

            _cache.Remove(empListCacheKey);

            //return CreatedAtRoute("Get", new { Id = emp.EmployeeId }, emp );
            return new ObjectResult(emp) { StatusCode = (int)HttpStatusCode.Created };
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public IActionResult Put(long id, [FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest("Employee is null.");
            }
            Employee employeeToUpdate = _empRepo.Get(id);
            if (employeeToUpdate == null)
            {
                return NotFound("The Employee record couldn't be found.");
            }
            _empRepo.Update(employeeToUpdate, employee);
            return NoContent();
        }
        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            Employee employee = _empRepo.Get(id);
            if (employee == null)
            {
                return NotFound("The Employee record couldn't be found.");
            }
            _empRepo.Delete(employee);
            return NoContent();
        }
    }
}

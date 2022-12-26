using Castle.Core.Logging;
using InMemoryCacheSRC.Controllers;
using InMemoryCacheSRC.Models;
using InMemoryCacheSRC.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public class EmployeeControllerTest
    {
        private const string empListCacheKey = "empList";

        [Fact]
        public void WhenAddNewEmp_ReturnSuccess()
        {
            var employee = GetEmployees().First();
            var repository = new Mock<IDataRepository<Employee>>();
            repository.Setup(r => r.Add(It.IsAny<Employee>())).Verifiable();

            var cachedEmployees = GetCacheEmployees();
            var cache = new MemoryCache(new MemoryCacheOptions());
            cache.Set(empListCacheKey, cachedEmployees);

            var logger = new Mock<ILogger<EmployeeController>>();
            var controller = new EmployeeController(repository.Object, cache, logger.Object);

            //Act
            var result = controller.Add(employee);
            var resultStatusCode = (result as ObjectResult)?.StatusCode;

            //Assert
            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Created, resultStatusCode);
        }

        [Fact]
        public void WhenAddNewEmp_ThenInValidCache()
        {
            var employee = GetEmployees().First();
            var repository = new Mock<IDataRepository<Employee>>();
            repository.Setup(r => r.Add(It.IsAny<Employee>())).Verifiable();

            var cachedEmployees = GetCacheEmployees();
            var cache = new MemoryCache(new MemoryCacheOptions());
            cache.Set(empListCacheKey, cachedEmployees);

            var logger = new Mock<ILogger<EmployeeController>>();
            var controller = new EmployeeController(repository.Object, cache, logger.Object);

            //Act 
            var result = controller.Add(employee);

            //Assert
            Assert.Null(cache.Get(empListCacheKey));

        }

        [Fact]
        public void WhenEmpInCache_ReturnSuccess()
        {
            //Arrange 
            var employees = GetEmployees();
            var repository = new Mock<IDataRepository<Employee>>();
            repository.Setup(r => r.GetAll()).Returns(employees);

            var cachedEmployees = GetCacheEmployees();
            var cache = new MemoryCache(new MemoryCacheOptions());
            cache.Set(empListCacheKey, cachedEmployees);

            var logger = new Mock<ILogger<EmployeeController>>();
            var controller = new EmployeeController(repository.Object, cache, logger.Object);

            //Act
            var result = controller.GetAsync();
            var resultCount = ((result.Result as ObjectResult)?.Value as List<Employee>)?.Count;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(1, resultCount);
        }

        [Fact]
        public void WhenEmplNotInCache_ReturnsSuccess()
        {
            // Arrange            
            var employees = GetEmployees();
            var repository = new Mock<IDataRepository<Employee>>();
            repository.Setup(r => r.GetAll()).Returns(employees);

            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = new Mock<ILogger<EmployeeController>>();
            var controller = new EmployeeController(repository.Object, cache, logger.Object);

            // Act            
            var result = controller.GetAsync();
            var resultCount = ((result.Result as ObjectResult)?.Value as List<Employee>)?.Count;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, resultCount);
        }

        private static List<Employee> GetEmployees()
        {
            return new List<Employee>()
            {
                new Employee()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    EmployeeId = 1
                },
                new Employee()
                {
                    FirstName = "Chris",
                    LastName = "Evans",
                    EmployeeId = 2
                }
            };
        }

        public static List<Employee> GetCacheEmployees()
        {
            return new List<Employee>()
            {
                new Employee()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    EmployeeId = 1
                }
            };
        }
    }
}

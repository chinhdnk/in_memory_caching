namespace InMemoryCacheSRC.Models.Repository
{
    public class EmployeeManager : IDataRepository<Employee>
    {
        public readonly EmployeeContext _empContext;
        public EmployeeManager(EmployeeContext empContext)
        {
            _empContext = empContext;
        }
        public void Add(Employee entity)
        {
            _empContext.Employees.Add(entity);
            _empContext.SaveChanges();
        }

        public void Delete(Employee entity)
        {
            _empContext?.Employees.Remove(entity);
            _empContext.SaveChanges();
        }

        public Employee Get(long id)
        {
            return _empContext.Employees.FirstOrDefault(m => m.EmployeeId == id);
        }

        public IEnumerable<Employee> GetAll()
        {
            return _empContext.Employees.ToList();
        }

        public void Update(Employee dbEntity, Employee entity)
        {
            dbEntity.EmployeeId = entity.EmployeeId;
            dbEntity.FirstName = entity.FirstName;
            dbEntity.LastName = entity.LastName;
            dbEntity.Email = entity.Email;
            dbEntity.DateOfBirth = entity.DateOfBirth;
            dbEntity.PhoneNumber=entity.PhoneNumber;
            _empContext.SaveChanges();
        }
    }
}

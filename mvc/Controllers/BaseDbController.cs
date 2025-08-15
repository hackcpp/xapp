using Microsoft.AspNetCore.Mvc;

namespace test.Controllers
{
    public abstract class BaseDbController : Controller
    {
        protected readonly string _connectionString;
        protected readonly ILogger _logger;

        protected BaseDbController(IConfiguration config, ILogger logger)
        {
            _connectionString = GetConnectionString(config);
            _logger = logger;
        }

        protected abstract string GetConnectionString(IConfiguration config);

    }
}
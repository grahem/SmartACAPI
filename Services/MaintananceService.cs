using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using SmartACDeviceAPI.Models;

namespace SmartACDeviceAPI.Services
{
    ///Checks to see weather or not the systems InMaintenance flag is set to true.
    ///Used to short circuit service actions
    public class MaintenanceService
    {
        private readonly IDynamoDBContext _dbContext;

        public MaintenanceService(IDynamoDBContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<bool> IsInMaintananceMode()
        {
            var sys = await _dbContext.LoadAsync<SystemConfig>("InMaintenance");
            return sys == null ? false : sys.ConfigValue.Equals("true");
            
        }
    }
}
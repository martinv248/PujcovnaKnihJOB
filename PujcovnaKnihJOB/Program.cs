using Hangfire;
using Hangfire.Storage;
using PujcovnaKnihJOB.Models;
using PujcovnaKnihJOB.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PujcovnaKnihJOB
{
    class Program
    {
        private ModelServices svc = new ModelServices();

        static void Main()
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=master;Integrated Security=True");
            using (var server = new BackgroundJobServer())
            {
                var p = new Program();
                p.deleteJobs();
                Console.WriteLine("Hangfire Server started. Press any key to close the application...");
                RecurringJob.AddOrUpdate(() => p.svc.checkBooks(), Cron.Minutely);
                Console.ReadKey();

            }
        }

        private void deleteJobs()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach(var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }
        }
        
    }
}

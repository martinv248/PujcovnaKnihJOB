using Hangfire;
using Hangfire.Storage;
using PujcovnaKnihJOB.Services;
using System;

namespace PujcovnaKnihJOB
{
    class Program
    {
        private ModelServices svc = new ModelServices();

        static void Main()
        {
            var p = new Program();

            GlobalConfiguration.Configuration.UseSqlServerStorage(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=master;Integrated Security=True");
            using (var server = new BackgroundJobServer())
            {                    
                p.deleteJobs();
                Console.WriteLine("----- PujcovnaKnihJOB -----");
                Console.WriteLine("Hangfire Server started.");
                RecurringJob.AddOrUpdate(() => p.svc.checkBooks(), Cron.Minutely);
                bool showMenu = true;
                while (showMenu)
                {
                    showMenu = Menu(p);
                }
            }
        }

        private static bool Menu(Program p)
        {
            Console.WriteLine("Vyberte možnost:");
            Console.WriteLine("1) Poslat fakturu zákazníkům, kteří vrátili knihu");
            Console.WriteLine("2) Ukončit aplikaci");

            switch (Console.ReadLine())
            {
                case "1":
                    BackgroundJob.Enqueue(() => p.svc.invoiceRents().Wait());
                    return true;
                case "2":
                    return false;
                default:
                    return true;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;
using ContosoBotPrototype.DataModels;
using System.Threading.Tasks;

namespace ContosoBotPrototype
{
    public class AzureManager
    {
        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<myTable> myTableRecords;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://msadatabasetest.azurewebsites.net");
            this.myTableRecords = this.client.GetTable<myTable>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<myTable>> getRecords()
        {
            return await this.myTableRecords.ToListAsync();
        }

        public async Task<List<string>> getPassWord(string user)
        {
            IMobileServiceTableQuery<string> query = myTableRecords.Where(record => record.userName == user).Select(record => record.passWord);
            return await query.ToListAsync();
        }

        public async Task<List<double>> getBalance(string user)
        {
            IMobileServiceTableQuery<double> query = myTableRecords.Where(record => record.userName == user).Select(record => record.Balance);
            return await query.ToListAsync();
        }

        public async Task addRecord(myTable record)
        {
            await this.myTableRecords.InsertAsync(record);
        }

        public async Task deleteRecord(myTable record)
        {
            await this.myTableRecords.DeleteAsync(record);
        }

        public async Task updateRecord(myTable record)
        {
            await this.myTableRecords.UpdateAsync(record);
        }
    }
}
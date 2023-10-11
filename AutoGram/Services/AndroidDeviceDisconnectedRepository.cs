using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public class AndroidDeviceDisconnectedRepository : IAndroidDeviceRepository
    {
        public void Create(AndroidDevice androidDevice)
        {
            using (var context = new AndroidDeviceContext())
            {
                androidDevice.ScoreRisk = androidDevice.Status.Accounts + androidDevice.Status.Failure;

                context.AndroidDevices.Add(androidDevice);
                context.SaveChanges();
            }
        }

        public AndroidDevice Get()
        {
            using (var context = new AndroidDeviceContext())
            {
                return context.AndroidDevices
                    .AsNoTracking()
                    .Include(x => x.Status)
                    .Include(x => x.App)
                    .Where(x => x.ScoreRisk == 4)
                    .Where(x => x.DateModified > new DateTime(2018, 12, 1))
                    .OrderByDescending(x => x.Status.Success)
                    .ThenBy(x => x.DateModified)
                    .FirstOrDefault();
            }
        }

        public AndroidDevice FindById(long id)
        {
            using (var context = new AndroidDeviceContext())
            {
                return context.AndroidDevices
                    .AsNoTracking()
                    .Include(x => x.Status)
                    .Include(x => x.App)
                    .FirstOrDefault(x => x.Id == id);
            }
        }

        public AndroidDevice FindByUuid(string uuid)
        {
            using (var context = new AndroidDeviceContext())
            {
                return context.AndroidDevices
                    .AsNoTracking()
                    .Include(x => x.Status)
                    .Include(x => x.App)
                    .FirstOrDefault(x => x.Uuid == uuid);
            }
        }

        public IEnumerable<AndroidDevice> GetAll()
        {
            using (var context = new AndroidDeviceContext())
            {
                return context.AndroidDevices
                    .AsNoTracking()
                    .ToList();
            }
        }

        public IEnumerable<AndroidDevice> GetAllWithInclude()
        {
            using (var context = new AndroidDeviceContext())
            {
                return context.AndroidDevices
                    .AsNoTracking()
                    .Include(x => x.Status)
                    .Include(x => x.App)
                    .OrderBy(x => x.ScoreRisk)
                    .ToList();
            }
        }

        public void Update(AndroidDevice androidDevice)
        {
            androidDevice.ScoreRisk = androidDevice.Status.Accounts + androidDevice.Status.Failure;

            using (var context = new AndroidDeviceContext())
            {
                context.Entry(androidDevice.Status).State = EntityState.Modified;
                context.Entry(androidDevice).State = EntityState.Modified;

                context.SaveChanges();
            }
        }

        public void Remove(AndroidDevice androidDevice)
        {
            using (var context = new AndroidDeviceContext())
            {
                context.Entry(androidDevice).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        public void RemoveById(long id)
        {
            var androidDevice = this.FindById(id);

            using (var context = new AndroidDeviceContext())
            {
                if (androidDevice != null)
                {
                    context.Entry(androidDevice).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}

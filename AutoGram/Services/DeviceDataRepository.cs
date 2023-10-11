using System;
using System.Linq;
using Database;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public static class DeviceDataRepository
    {
        private static readonly object Lock = new object();

        public static DeviceData GetDevicaData()
        {
            lock (Lock)
                using (var db = new DeviceDataContext())
                {
                    return db.DeviceDatas
                        .AsNoTracking()
                        .Where(d => d.Accounts < 10)
                        .OrderBy(d => d.DateModified)
                        .FirstOrDefault();
                }
        }

        public static bool Any()
        {
            lock (Lock)
                using (var db = new DeviceDataContext())
                {
                    return db.DeviceDatas
                        .AsNoTracking()
                        .Any(d => d.Accounts < 10);
                }
        }

        public static DeviceData GetPostById(int id)
        {
            lock (Lock)
                using (var db = new DeviceDataContext())
                {
                    return db.DeviceDatas
                        .AsNoTracking()
                        .FirstOrDefault(x => x.Id == id);
                }
        }

        public static void Update(DeviceData deviceData)
        {
            deviceData.DateModified = DateTime.Now;

            lock (Lock)
                using (var db = new DeviceDataContext())
                {
                    db.Entry(deviceData).State = EntityState.Modified;
                    db.SaveChanges();
                }
        }
    }
}

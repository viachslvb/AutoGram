using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public class AndroidDeviceRepository : IAndroidDeviceRepository
    {
        readonly AndroidDeviceContext _context;

        public AndroidDeviceRepository()
        {
            _context = new AndroidDeviceContext();
        }

        public void Create(AndroidDevice androidDevice)
        {
            androidDevice.DateModified = DateTime.Now;
            _context.AndroidDevices.Add(androidDevice);
            _context.SaveChanges();
        }

        public AndroidDevice Get()
        {
            return _context.AndroidDevices
                .Include(x => x.Status)
                .Include(x => x.App)
                .Where(x => x.ScoreRisk == 2)
                .Where(x => x.DateModified > new DateTime(2018, 12, 1))
                .OrderByDescending(x => x.Status.Success)
                .ThenBy(x => x.DateModified)
                .FirstOrDefault();
        }

        public AndroidDevice FindById(long id)
        {
            return _context.AndroidDevices
                .Include(x => x.Status)
                .Include(x => x.App)
                .FirstOrDefault(x => x.Id == id);
        }

        public AndroidDevice FindByUuid(string uuid)
        {
            return _context.AndroidDevices
                .Include(x => x.Status)
                .Include(x => x.App)
                .FirstOrDefault(x => x.Uuid == uuid);
        }

        public IEnumerable<AndroidDevice> GetAll()
        {
            return _context.AndroidDevices.ToList();
        }

        public IEnumerable<AndroidDevice> GetAllWithInclude()
        {
            return _context.AndroidDevices
                .Include(x => x.Status)
                .Include(x => x.App)
                .OrderBy(x => x.ScoreRisk)
                .ToList();
        }

        public void Update(AndroidDevice androidDevice)
        {
            androidDevice.ScoreRisk = androidDevice.Status.Accounts + androidDevice.Status.Failure;
            androidDevice.DateModified = DateTime.Now;

            _context.Entry(androidDevice).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Remove(AndroidDevice androidDevice)
        {
            _context.Remove(androidDevice);
            _context.SaveChanges();
        }

        public void RemoveById(long id)
        {
            var androidDevice = this.FindById(id);

            if (androidDevice != null)
            {
                _context.Remove(androidDevice);
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

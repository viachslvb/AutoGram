using System;
using System.Collections.Generic;
using Database;

namespace AutoGram.Services
{
    public interface IAndroidDeviceRepository : IDisposable
    {
        void Create(AndroidDevice androidDevice);
        AndroidDevice Get();
        AndroidDevice FindById(long id);
        AndroidDevice FindByUuid(string uuid);
        IEnumerable<AndroidDevice> GetAll();
        IEnumerable<AndroidDevice> GetAllWithInclude();
        void Update(AndroidDevice androidDevice);
        void Remove(AndroidDevice androidDevice);
        void RemoveById(long id);
    }
}
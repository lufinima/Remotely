using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DeviceId;
using DeviceId.Encoders;
using DeviceId.Formatters;
using Standart.Hash.xxHash;

namespace Remotely.Shared.Utilities
{
    public static class UniqueIdGenerator
    {
        public static string GenerateID()
        {
            var deviceId = new DeviceIdBuilder()
                    .AddSystemDriveSerialNumber()
                    .AddMotherboardSerialNumber()
                    .AddMachineName()
                    .AddSystemUUID()
                    .UseFormatter(new HashDeviceIdFormatter(() => MD5.Create(), new Base64UrlByteArrayEncoder()))
                    .ToString();
            byte[] data = Encoding.UTF8.GetBytes(deviceId);

            return xxHash32.ComputeHash(data, data.Length).ToString();
        }
    }
}

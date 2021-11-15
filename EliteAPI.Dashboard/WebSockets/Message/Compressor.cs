﻿using System.IO;
using Newtonsoft.Json;

namespace EliteAPI.Dashboard.WebSockets.Message
{
    public static class Compressor
    {
        public static string Compress(WebSocketMessage message)
        {
            return JsonConvert.SerializeObject(message);
            
            // var json = JsonConvert.SerializeObject(message);
            // var bytes = Encoding.UTF8.GetBytes(json);
            //
            // using var msi = new MemoryStream(bytes);
            // using var mso = new MemoryStream();
            // using (var gs = new DeflaterOutputStream(mso)) {
            //     CopyTo(msi, gs);
            // }
            // var compressedJson = mso.ToArray();
            //
            // var base64 = Convert.ToBase64String(compressedJson);
            // return base64;
        }

        public static WebSocketMessage Decompress(string base64)
        {
            return JsonConvert.DeserializeObject<WebSocketMessage>(base64);
            
            // var compressedBytes = Convert.FromBase64String(base64);
            //
            // using var msi = new MemoryStream(compressedBytes);
            // using var mso = new MemoryStream();
            // var gs = new InflaterInputStream(msi);
            // CopyTo(gs, mso);
            //
            // var bytes = mso.ToArray();
            //
            // var json = Encoding.UTF8.GetString(bytes);
            //
            // return JsonConvert.DeserializeObject<WebSocketMessage>(json);
        }
        
        private static void CopyTo(Stream src, Stream dest) {
            var bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}
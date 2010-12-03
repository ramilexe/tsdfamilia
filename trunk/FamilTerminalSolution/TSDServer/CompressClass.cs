using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.IO.Compression;

namespace TSDServer
{
    public static class Compressor
    {

        public static byte[] Compress(System.Runtime.Serialization.ISerializable obj)
        {

            //    infile.Length];
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, obj);
            byte[] buffer = ms.ToArray();
            //Stream stream = new MemoryStream();

            //    FileStream("MyFile.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            //MyObject obj = (MyObject)formatter.Deserialize(stream);
            //stream.Close();



            // Use the newly created memory stream for the compressed data.
            MemoryStream msOutput = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(msOutput, CompressionMode.Compress, true);

            compressedzipStream.Write(buffer, 0, buffer.Length);
            // Close the stream.
            compressedzipStream.Close();
            return msOutput.ToArray();

        }

        public static byte[] Compress(byte[] obj)
        {

            //    infile.Length];
            //IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            //formatter.Serialize(ms, obj);
            byte[] buffer = obj;
            //Stream stream = new MemoryStream();

            //    FileStream("MyFile.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            //MyObject obj = (MyObject)formatter.Deserialize(stream);
            //stream.Close();



            // Use the newly created memory stream for the compressed data.
            MemoryStream msOutput = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(msOutput, CompressionMode.Compress, true);

            compressedzipStream.Write(buffer, 0, buffer.Length);
            // Close the stream.
            compressedzipStream.Close();
            return msOutput.ToArray();

        }

        public static byte[] DeCompressBytes(byte[] Data)
        {
            object o = null;
            byte[] buffer1 = new byte[UInt16.MaxValue];
            byte[] buffer2 = new byte[UInt16.MaxValue];

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(Data, 0, Data.Length);
                ms.Seek(0, SeekOrigin.Begin);


                using (MemoryStream ms1 = new MemoryStream())
                {
                    using (GZipStream compressedzipStream1 = new GZipStream(ms, CompressionMode.Decompress))
                    {

                        int offset = 0;
                        int totalCount = 0;
                        while (true)
                        {

                            int bytesRead = compressedzipStream1.Read(buffer1, 0, buffer1.Length);

                            ms1.Write(buffer1, 0, bytesRead);
                            if (bytesRead == 0)
                            {
                                break;
                            }
                            offset += bytesRead;
                            totalCount += bytesRead;
                        }

                        compressedzipStream1.Close();
                        ms.Close();


                        //byte[] outBuffer1 = new byte[ms1.Length];
                        ms1.Seek(0, SeekOrigin.Begin);
                        buffer2 = ms1.ToArray();
                        //ms1.Read(outBuffer1, 0, (int)ms1.Length);

                        //IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                        //o = formatter.Deserialize(ms1);


                    }
                    ms1.Close();
                }
            }
            return buffer2;
        }
        public static object DeCompress(byte[] Data)
        {
            object o = null;
            byte[] buffer1 = new byte[UInt16.MaxValue];

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(Data, 0, Data.Length);
                ms.Seek(0, SeekOrigin.Begin);


                using (MemoryStream ms1 = new MemoryStream())
                {
                    using (GZipStream compressedzipStream1 = new GZipStream(ms, CompressionMode.Decompress))
                    {

                        int offset = 0;
                        int totalCount = 0;
                        while (true)
                        {

                            int bytesRead = compressedzipStream1.Read(buffer1, 0, buffer1.Length);

                            ms1.Write(buffer1, 0, bytesRead);
                            if (bytesRead == 0)
                            {
                                break;
                            }
                            offset += bytesRead;
                            totalCount += bytesRead;
                        }

                        compressedzipStream1.Close();
                        ms.Close();


                        //byte[] outBuffer1 = new byte[ms1.Length];
                        ms1.Seek(0, SeekOrigin.Begin);
                        //ms1.Read(outBuffer1, 0, (int)ms1.Length);

                        IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                        o = formatter.Deserialize(ms1);


                    }
                    ms1.Close();
                }
            }
            return o;
        }
    }

}

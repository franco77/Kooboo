//Copyright (c) 2018 Yardi Technology Limited. Http://www.kooboo.com 
//All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kooboo.Lib.Utilities
{
    public class CalculateUtility
    {
        /// <summary>
        /// Convert bytes to KB/MB/GB
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetSizeString(long bytes)
        {
            Decimal filesize = new Decimal(bytes);
            Decimal gigabytes = new Decimal(1024 * 1024 * 1024);
            var returnValue = filesize / gigabytes;
            if (returnValue > 1)
            {
                return (returnValue.ToString("#0.00") + "GB");
            }
            Decimal megabyte = new Decimal(1024 * 1024);
            returnValue = filesize / megabyte;
            if (returnValue > 1)
            {
                return (returnValue.ToString("#0.00") + "MB");
            }
            Decimal kilobyte = new Decimal(1024);
            returnValue = filesize / kilobyte;
            return (returnValue.ToString("#0.00") + "KB");
        }
          
        public static SizeMeansurement GetImageSize(byte[] imagebytes)
        { 
            SizeMeansurement measure = new SizeMeansurement(); 
             
            try
            {
                System.IO.MemoryStream stream = new System.IO.MemoryStream(imagebytes); 
                System.Drawing.Image image = null;
                image = System.Drawing.Image.FromStream(stream);
                measure.Height = image.Height;
                measure.Width = image.Width;
            }
            catch (Exception ex)
            { 

            } 
            return measure;  
        }
    }

    public class SizeMeansurement
    {
        public int Height { get; set; }

        public int Width { get; set; }
    }

       

}

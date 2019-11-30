using System;
using System.Collections.Generic;
using System.Diagnostics;
using AForge.Imaging.Filters;
using Emgu.CV;
using System.Drawing;
using System.IO;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using OfficeOpenXml;
using Tesseract;

namespace CABR_ID_SCANNER
{
    
    public static class Utils
    {
        private static double DETECT_THRESHOLD = 0.8;
        
        public static Boolean hasMiddleName(Image<Gray, Byte> Input_Image) {
            Image<Gray, Byte> mark_line = new Image<Gray, Byte>(Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\linie.jpg") as Bitmap);
            return detectObject(Input_Image, mark_line);
        }

        public static string extractBirthDateFromCnp(string cnp)
        {
            string birthDate = "";
            
            if (!cnp.Length.Equals(13)) return birthDate;
            
            string sex = cnp.Substring(0, 1);
            string ani = cnp.Substring(1, 2);
            string luna = cnp.Substring(3, 2);
            string zi = cnp.Substring(5, 2);

            birthDate += zi;
            birthDate += ".";
            birthDate += luna;
            birthDate += ".";

            if (sex.Equals("1") || sex.Equals("2"))
            {
                birthDate += "19" + ani;
            }
            else if (sex.Equals("3") || sex.Equals("4"))
            {
                birthDate += "18" + ani;
            }
            else if (sex.Equals("5") || sex.Equals("6"))
            {
                birthDate += "20" + ani;
            }

            return birthDate;
        }

        public static bool detectObject(Image<Gray, Byte> Input_Image, Image<Gray, Byte> object_Image)
        {
            using (Image<Gray, float> result =
                Input_Image.MatchTemplate(object_Image, TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > DETECT_THRESHOLD)
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    return true;
                }
            }

            return false;
        }

        public static Point getObjectPositionFromImage(Image<Gray, Byte> Input_Image, Image<Gray, Byte> object_Image)
        {
            using (Image<Gray, float> result =
                Input_Image.MatchTemplate(object_Image, TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > DETECT_THRESHOLD)
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    return maxLocations[0];
                }
            }

            return Point.Empty;
        }

        public static string createCropFromFile(string fileName, Rectangle cropRect, string outputName)
        {
            Bitmap src = Image.FromFile(fileName) as Bitmap;
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
                string output = @"C:\Users\AndreiStoica\CABR_ID_SCANNER\crops\" + outputName;

                target.Save(output);

                return output;
            }
        }

        /*public static string applyFilterOnImageFile(string sourceFilePath, string outputFileName)
        {
            string outputFilePath = new DirectoryInfo(sourceFilePath).Parent.FullName + "\\" + outputFileName;
            Bitmap image = Image.FromFile(sourceFilePath) as Bitmap;

            Grayscale grayFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            GaussianSharpen gausFilter = new GaussianSharpen(4, 11);
            Invert invertFilter = new Invert();

            image = grayFilter.Apply(image);
            image = gausFilter.Apply(image);
            invertFilter.ApplyInPlace(image);

            image.Save(outputFilePath);
            image.Dispose();

            return outputFilePath;
        }*/

        public static string applyFilterOnImageFile(string sourceFilePath, string outputFileName,
            double binaryFilterThreshold = 150D)
        {
            string outputFilePath = new DirectoryInfo(sourceFilePath).Parent.FullName + "\\" + outputFileName;
            
            Bitmap image = Image.FromFile(sourceFilePath) as Bitmap;
            Image<Gray, byte> greyImage = new Image<Gray, byte>(image);
            Image<Gray, byte> binaryImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));
            Image<Gray, byte> cubicResizeImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));
            Image<Gray, byte> bilateralImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));
            
            CvInvoke.Threshold(greyImage, binaryImage, binaryFilterThreshold, 255, ThresholdType.Binary);
            CvInvoke.Resize(binaryImage, cubicResizeImage, Size.Empty, 2D, 2D, Inter.Cubic);
            CvInvoke.BilateralFilter(cubicResizeImage, bilateralImage, 9, 75, 75);
            
            bilateralImage.Save(outputFilePath);
            
            image.Dispose();
            greyImage.Dispose();
            binaryImage.Dispose();
            cubicResizeImage.Dispose();
            bilateralImage.Dispose();

            return outputFilePath;
        }

        public static void applyCompleteFilterOnImageFile(string sourceFilePath)
        {
            Bitmap image = Image.FromFile(sourceFilePath) as Bitmap;

            Image<Gray, byte> grayImage = new Image<Gray, byte>(image);

            Image<Gray, byte> binarizeImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));

            Image<Gray, byte> adaptiveBinarizeImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));
            
            Image<Gray, byte> linearResizeImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));
            
            Image<Gray, byte> cubicResizeImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));

            Image<Gray, byte> medianImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));

            Image<Gray, byte> bilateralImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));

            Image<Gray, byte> gausianImage = new Image<Gray, byte>(image.Width, image.Height, new Gray(0));

            CvInvoke.Threshold(grayImage, binarizeImage, 125, 255, ThresholdType.Binary);

            CvInvoke.AdaptiveThreshold(grayImage, adaptiveBinarizeImage, 255, AdaptiveThresholdType.GaussianC,
                ThresholdType.Binary, 31, 2);
            
            CvInvoke.Resize(binarizeImage, linearResizeImage, Size.Empty, 2D, 2D, Inter.Linear);
            
            CvInvoke.Resize(binarizeImage, cubicResizeImage, Size.Empty, 2D, 2D, Inter.Cubic);

            CvInvoke.MedianBlur(cubicResizeImage, medianImage, 3);

            CvInvoke.BilateralFilter(cubicResizeImage, bilateralImage, 9, 75, 75);

            CvInvoke.GaussianBlur(cubicResizeImage, gausianImage, new Size(5, 5), 0);

            grayImage.Save(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\crops\grey_" +
                           Path.GetFileNameWithoutExtension(sourceFilePath) + ".png");

            binarizeImage.Save(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\crops\binar_" +
                               Path.GetFileNameWithoutExtension(sourceFilePath) + ".png");

            adaptiveBinarizeImage.Save(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\crops\adaptive_binar_" +
                                       Path.GetFileNameWithoutExtension(sourceFilePath) + ".png");

            linearResizeImage.Save(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\crops\linear_resize_" +
                                       Path.GetFileNameWithoutExtension(sourceFilePath) + ".png");

            cubicResizeImage.Save(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\crops\cubic_resize_" +
                                       Path.GetFileNameWithoutExtension(sourceFilePath) + ".png");

            medianImage.Save(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\crops\median_" +
                             Path.GetFileNameWithoutExtension(sourceFilePath) + ".png");

            bilateralImage.Save(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\crops\bilateral_" +
                                Path.GetFileNameWithoutExtension(sourceFilePath) + ".png");

            gausianImage.Save(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\crops\gausian_" +
                              Path.GetFileNameWithoutExtension(sourceFilePath) + ".png");

            image.Dispose();
            grayImage.Dispose();
            binarizeImage.Dispose();
            adaptiveBinarizeImage.Dispose();
            linearResizeImage.Dispose();
            cubicResizeImage.Dispose();
            medianImage.Dispose();
            bilateralImage.Dispose();
            gausianImage.Dispose();
        }
        
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        
        public static bool cnpValid(string cnp)
        {
            Console.WriteLine("Se verifica validitatea CNP-ului {0}.", cnp);

            // cifra = 1/2
            DateTime unuDoiIanuarie1900 = new DateTime(1900, 1, 1);
            DateTime treiUnuDecembrie1999 = new DateTime(1999, 12, 31);

            // cifra = 3/4
            DateTime unuDoiIanuarie1800 = new DateTime(1800, 1, 1);
            DateTime treiUnuDecembrie1899 = new DateTime(1899, 12, 31);

            // cifra = 5/6
            DateTime unuDoiIanuarie2000 = new DateTime(2000, 1, 1);
            DateTime treiUnuDecembrie2099 = new DateTime(2099, 12, 31);

            // cifra = 7/8
            // straini

            try
            {
                if (!cnp.Length.Equals(13)) return false;

                string sex = cnp.Substring(0, 1);
                string ani = cnp.Substring(1, 2);
                string luna = cnp.Substring(3, 2);
                string zi = cnp.Substring(5, 2);
                string judet = cnp.Substring(7, 2);
                string nnn = cnp.Substring(9, 3);
                string c = cnp.Substring(12);


                // partea de sex si data nastere
                if (sex.Equals("1") || sex.Equals("2"))
                {
                    DateTime dtBuletin = new DateTime(int.Parse("19" + ani), int.Parse(luna), int.Parse(zi));
                    if (!IsBewteenTwoDates(dtBuletin, unuDoiIanuarie1900, treiUnuDecembrie1999))
                    {
                        Console.WriteLine("CNP-ul {0} este invalid deoarece nu s-a nascut intre 1900-1999.", cnp);
                        return false;
                    }
                }
                else if (sex.Equals("3") || sex.Equals("4"))
                {
                    DateTime dtBuletin = new DateTime(int.Parse("18" + ani), int.Parse(luna), int.Parse(zi));
                    if (!IsBewteenTwoDates(dtBuletin, unuDoiIanuarie1800, treiUnuDecembrie1899))
                    {
                        Console.WriteLine("CNP-ul {0} este invalid deoarece nu s-a nascut intre 1800-1899.", cnp);
                        return false;
                    }
                }
                else if (sex.Equals("5") || sex.Equals("6"))
                {
                    DateTime dtBuletin = new DateTime(int.Parse("20" + ani), int.Parse(luna), int.Parse(zi));
                    if (!IsBewteenTwoDates(dtBuletin, unuDoiIanuarie2000, treiUnuDecembrie2099))
                    {
                        Console.WriteLine("CNP-ul {0} este invalid deoarece nu s-a nascut intre 2000-2099.", cnp);
                        return false;
                    }
                }
                else if (sex.Equals("9") || sex.Equals("0"))
                {
                    Console.WriteLine("CNP-ul {0} este invalid deoarece contine la cifra sexului 9 sau 0.", cnp);
                    return false;
                }


                // calcul judet
                /*if ((int.Parse(judet) > 47 && int.Parse(judet) < 52) || int.Parse(judet) > 52 || int.Parse(judet) == 0)
                {
                    Console.WriteLine("CNP-ul {0} este invalid datorita judetului.", cnp);
                    return false;
                }*/

                // partea de C
                string pattern = "279146358279";

                int total = 0;
                for (int i = 0; i < pattern.Length; i++)
                {
                    int cifraCurenta = int.Parse(Char.ToString(cnp[i])) * int.Parse(Char.ToString(pattern[i]));
                    total += cifraCurenta;
                }

                if (total % 11 == 10)
                {
                    total = 1;
                }
                else
                {
                    total = total % 11;
                }

                if (total.ToString() != c)
                {
                    Console.WriteLine("CNP-ul {0} este invalid datorita cifrei de control.", cnp);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CNP-ul {0} este invalid din cauza unei exceptii: {1}.", cnp, ex.Message);
                return false;
            }

            Console.WriteLine("CNP-ul {0} este valid.", cnp);
            return true;
        }


        public static bool IsBewteenTwoDates(this DateTime dt, DateTime start, DateTime end)
        {
            return dt >= start && dt <= end;
        }

        public static string getTextFromImageWithTesseract(string imgPath, string lang, string camp,
            string filteredFileName, double binaryFilterThreshold = 150D)
        {
            string textFromImage = "";

            string filteredImage = applyFilterOnImageFile(imgPath, filteredFileName, binaryFilterThreshold);

            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", lang, EngineMode.Default))
                {
                    switch (camp)
                    {
                        case "cnp":
                            engine.SetVariable("tessedit_char_whitelist", "1234567890");
                            break;
                        case "nume":
                            engine.SetVariable("tessedit_char_whitelist", "AĂÂBCDEFGHIÎJKLMNOPQRSȘTȚUVWXYZ");
                            engine.SetVariable("tessedit_enable_dict_correction", 1);
                            break;
                        case "expire":
                            engine.SetVariable("tessedit_char_whitelist", "1234567890.");
                            engine.SetVariable("tessedit_enable_dict_correction", 1);
                            break;
                        default:
                            engine.SetVariable("tessedit_char_whitelist",
                                "AaĂăÂâBbCcDdEeFfGgHhIiÎîJjKkLlMmNnOoPpQqRrSsȘșTtȚțUuVvWwXxYyZz1234567890-");
                            engine.SetVariable("tessedit_enable_dict_correction", 1);
                            break;
                    }

                    using (Pix img = Pix.LoadFromFile(filteredImage))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
//                            Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                            //Console.Write(" {0}", text.Trim());
                            textFromImage = text.Trim();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Console.WriteLine("Unexpected Error: " + e.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(e.ToString());
            }

            return textFromImage;
        }

        public static void createExcelFile(List<PersonalIdModel> personalIdModelList)
        {
            FileInfo excelFile = new FileInfo(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\personalId.xlsx");
            if (excelFile.Exists)
            {
                excelFile.Delete();
            }
            
            using (ExcelPackage excel = new ExcelPackage(excelFile))
            {
                excel.Workbook.Worksheets.Add("Worksheet 1");
                ExcelWorksheet worksheet = excel.Workbook.Worksheets["Worksheet 1"];
                
                List<string[]> headerRow = new List<string[]>()
                {
                    new[]
                    {
                        "First name", "Middle name", "Last name", "CNP", "Gender", "Birth place", "Birth country",
                        "Date of birth", "Nationality", "Id Type", "Id Number", "Issue Date", "Expiry Date",
                        "Issuing Country", "Issuing Entity", "Street no", "Street name",
                        "Other address (ex: ap, bl, floor...)", "Region", "City", "Country"
                    }
                };
                string lastColumnCharacter = Char.ConvertFromUtf32(headerRow[0].Length + 64);
                string headerRange = "A1:" + lastColumnCharacter + "1";
                worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                int currentRowNumber = 2;

                foreach (PersonalIdModel personalIdModel in personalIdModelList)
                {
                    List<string[]> row = personalIdModel.CreateExcelRow();
                    string rowRange = "A" + currentRowNumber + ":" + lastColumnCharacter + currentRowNumber;
                    
                    worksheet.Cells[rowRange].LoadFromArrays(row);
                    currentRowNumber++;
                }
                
                excel.Save();
            }
        }
    }
}

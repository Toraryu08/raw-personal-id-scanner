using System;
using System.Drawing;
using Tesseract;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Emgu.CV;
using Emgu.CV.Structure;


namespace CABR_ID_SCANNER
{

    public static class Program
    {
        static void Main(string[] args)
        {
            FileStream filestream = new FileStream(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\out.txt", FileMode.Create);
            StreamWriter streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);

            string[] filePaths = Directory.GetFiles(@"C:\Users\AndreiStoica\Test_scan\Crop\", "*.jpg");
            ArrayList CropOutput = new ArrayList();

            foreach (String sourceFilePath in filePaths)
            {
                PersonalIdCropFilePaths personalIdCropFilePaths = new PersonalIdCropFilePaths();
                personalIdCropFilePaths.PersonalIdFilepath = sourceFilePath;
                Image<Gray, Byte> sourceImage = new Image<Gray, Byte>(Image.FromFile(sourceFilePath) as Bitmap);

                personalIdCropFilePaths.CnpCropFilepath = Utils.createCropFromFile(sourceFilePath,
                    new Rectangle(302, 117, 200, 30),
                    "CPN_out_" + Path.GetFileNameWithoutExtension(sourceFilePath) + ".png"
                );

                ExtractUtil.extractNames(personalIdCropFilePaths, sourceFilePath, sourceImage);
                ExtractUtil.extractGender(personalIdCropFilePaths, sourceFilePath, sourceImage);
                ExtractUtil.extractBirthplace(personalIdCropFilePaths, sourceFilePath, sourceImage);
                ExtractUtil.extractIssuedBy(personalIdCropFilePaths, sourceFilePath, sourceImage);
                ExtractUtil.extractValidity(personalIdCropFilePaths, sourceFilePath, sourceImage);
                ExtractUtil.extractNationality(personalIdCropFilePaths, sourceFilePath, sourceImage);
                ExtractUtil.extractIdNumber(personalIdCropFilePaths, sourceFilePath, sourceImage);

                personalIdCropFilePaths.AddressCropFilePaths = ExtractUtil.extractAddress(sourceFilePath, sourceImage);

                CropOutput.Add(personalIdCropFilePaths);
            }
            
            List<PersonalIdModel> personalIdModels = new List<PersonalIdModel>();

            foreach (FileInfo fi in (new DirectoryInfo(@"./tessdata")).GetFiles("*.traineddata"))
            {
                string lang = fi.Name.Substring(0, 3);
                if (!lang.Equals("eng")) continue;
                Console.WriteLine("Now trying for language: " + lang.ToUpper());

                foreach (PersonalIdCropFilePaths idCropFilePaths in CropOutput)
                {
                    string meanCnp = Utils.getTextFromImageWithTesseract(idCropFilePaths.CnpCropFilepath,
                        lang, "cnp", 
                        "CPN_out_filtered_" + Path.GetFileNameWithoutExtension(idCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("MEAN CNP : {0}", meanCnp);
                    string cnp = Regex
                        .Replace(meanCnp.Replace("O", "0").Replace("l", "1")
                            .Replace("o", "0"), "[^0-9]", "").Truncate(13);
                    Console.WriteLine("CNP : {0}", cnp);

                    Utils.cnpValid(cnp);
//                    if (Utils.cnpValid(cnp))
//                    {
                        PersonalIdModel personalIdModel = createPersonelIdModel(idCropFilePaths, lang);
                        personalIdModel.Cnp = cnp;
                        personalIdModel.DateOfBirth = Utils.extractBirthDateFromCnp(cnp);
                        personalIdModels.Add(personalIdModel);
//                    }
//                    else
//                    {
//                        Console.WriteLine(
//                            " CNP-ul nu este valid, asadar citirea a esuat, nu mai fac restul citirilor.");
//                        Console.WriteLine(" \n ___________________________________________");
//                    }
                }
            }
            
            Utils.createExcelFile(personalIdModels);
        }

        private static PersonalIdModel createPersonelIdModel(PersonalIdCropFilePaths personalIdCropFilePaths,
            string lang)
        {
            PersonalIdModel personalIdModel = new PersonalIdModel();

            if (personalIdCropFilePaths.IdNumberCropFilepath != null)
            {
                personalIdModel.IdNumber =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.IdNumberCropFilepath, lang, 
                        "idNumber", "ID_NUMBER_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("NUMAR ID : {0}", personalIdModel.IdNumber);
            }

            if (personalIdCropFilePaths.FirstNameCropFilepath != null)
            {
                personalIdModel.FirstName =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.FirstNameCropFilepath, lang, 
                        "prenume", "FIRST_NAME_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("PRENUME : {0}", personalIdModel.FirstName);
            }

            if (personalIdCropFilePaths.MiddleNameCropFilepath != null)
            {
                personalIdModel.MiddleName = Utils.getTextFromImageWithTesseract(
                    personalIdCropFilePaths.MiddleNameCropFilepath, lang, "middle name",
                    "MIDDLE_NAME_out_filtered_" +
                    Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("NUME MIJLOCIU : {0}", personalIdModel.MiddleName);
            }

            if (personalIdCropFilePaths.LastNameCropFilepath != null)
            {
                personalIdModel.LastName =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.LastNameCropFilepath, lang, "nume",
                        "LAST_NAME_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("NUME : {0}", personalIdModel.LastName);
            }

            if (personalIdCropFilePaths.GenderCropFilepath != null)
            {
                personalIdModel.Gender =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.GenderCropFilepath, lang, "gender",
                        "GENDER_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("GENDER : {0}", personalIdModel.Gender);
            }

            if (personalIdCropFilePaths.BirthplaceCropFilepath != null)
            {
                personalIdModel.BirthPlace =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.BirthplaceCropFilepath, lang, "birthplace",
                        "BIRTHPLACE_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png",
                        100D);
                Console.WriteLine("BIRTH PLACE : {0}", personalIdModel.BirthPlace);
            }

            if (personalIdCropFilePaths.IssuedByCropFilepath != null)
            {
                personalIdModel.IssuingEntity =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.IssuedByCropFilepath, lang, "issuedBy",
                        "ISSUED_BY_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("ELIBERAT DE : {0}", personalIdModel.IssuingEntity);
            }

            if (personalIdCropFilePaths.EmittedDateCropFilepath != null)
            {
                personalIdModel.IssueDate =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.EmittedDateCropFilepath, lang, "issuedDate",
                        "ISSUED_DATE_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("DATA ELIBERARE : {0}", personalIdModel.IssueDate);
            }

            if (personalIdCropFilePaths.ExpireDateCropFilepath != null)
            {
                personalIdModel.ExpiryDate =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.ExpireDateCropFilepath, lang, "expiryDate",
                        "EXPIRY_DATE_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("DATA EXPIRARE : {0}", personalIdModel.ExpiryDate);
            }
            
            if (personalIdCropFilePaths.ValidityCropFilepath != null)
            {
                string valability =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.ValidityCropFilepath, lang, "validity",
                        "VALIDITY_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("DATA VALABILITATE : {0}", valability);
            }

            if (personalIdCropFilePaths.NationalityCropFilepath != null)
            {
                personalIdModel.Nationality =
                    Utils.getTextFromImageWithTesseract(personalIdCropFilePaths.NationalityCropFilepath, lang, 
                        "nationality", "NATIONALITY_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                Console.WriteLine("NATIONALITATE : {0}", personalIdModel.Nationality);
            }

            if (personalIdCropFilePaths.AddressCropFilePaths != null)
            {
                Console.WriteLine("DOMICILIU : ");

                // populate city
                if (personalIdCropFilePaths.AddressCropFilePaths.MunicipalityCropFilepath != null)
                {
                    personalIdModel.City = "Mun. " + Utils.getTextFromImageWithTesseract(
                                               personalIdCropFilePaths.AddressCropFilePaths.MunicipalityCropFilepath,
                                               lang, "city",
                                               "MUNICIPALITY_out_filtered_" +
                                               Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t ORAS : {0}", personalIdModel.City);
                }
                if (personalIdCropFilePaths.AddressCropFilePaths.CityCropFilepath != null)
                {
                    personalIdModel.City = "Ors. " + Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.CityCropFilepath,
                        lang, "city",
                        "CITY_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t ORAS : {0}", personalIdModel.City);
                }
                if (personalIdCropFilePaths.AddressCropFilePaths.VillageCropFilepath != null)
                {
                    personalIdModel.City = "Sat. " + Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.VillageCropFilepath,
                        lang, "village",
                        "VILLAGE_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t ORAS : {0}", personalIdModel.City);
                }

                // populate region
                if (personalIdCropFilePaths.AddressCropFilePaths.DistrictCropFilepath != null)
                {
                    personalIdModel.Region = "Jud. " + Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.DistrictCropFilepath,
                        lang, "district",
                        "DISTRICT_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t REGIUNE : {0}", personalIdModel.Region);
                }
                if (personalIdCropFilePaths.AddressCropFilePaths.SectorCropFilepath != null)
                {
                    personalIdModel.Region = "Sec. " + Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.SectorCropFilepath,
                        lang, "sector",
                        "SECTOR_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t REGIUNE : {0}", personalIdModel.Region);
                }

                // populate street name
                if (personalIdCropFilePaths.AddressCropFilePaths.AlleyCropFilepath != null)
                {
                    personalIdModel.StreetName = "Ale. " + Utils.getTextFromImageWithTesseract(
                                                     personalIdCropFilePaths.AddressCropFilePaths.AlleyCropFilepath, lang, "alley",
                                                     "ALLEY_out_filtered_" +
                                                     Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t STRADA : {0}", personalIdModel.StreetName);
                }
                if (personalIdCropFilePaths.AddressCropFilePaths.DrivewayCropFilepath != null)
                {
                    personalIdModel.StreetName = "Sos. " + Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.DrivewayCropFilepath, lang, "street",
                        "DRIVEWAY_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t STRADA : {0}", personalIdModel.StreetName);
                }
                if (personalIdCropFilePaths.AddressCropFilePaths.StreetCropFilepath != null)
                {
                    personalIdModel.StreetName = "Str. " + Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.StreetCropFilepath, lang, "street",
                        "STREET_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t STRADA : {0}", personalIdModel.StreetName);
                }
                if (personalIdCropFilePaths.AddressCropFilePaths.BoulevardCropFilepath != null)
                {
                    personalIdModel.StreetName = "Bld. " + Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.BoulevardCropFilepath, lang, "boulevard",
                        "BOULEVARD_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t STRADA : {0}", personalIdModel.StreetName);
                }

                // populate street number
                if (personalIdCropFilePaths.AddressCropFilePaths.StreetNumberCropFilepath != null)
                {
                    personalIdModel.StreetNumber = Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.StreetNumberCropFilepath,
                        lang, "streetNumber",
                        "STREET_NUMBER_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t NUMAR STRADA : {0}", personalIdModel.StreetNumber);
                }

                string otherAddress = "";

                if (personalIdCropFilePaths.AddressCropFilePaths.BlockCropFilepath != null)
                {
                    string block = Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.BlockCropFilepath,
                        lang, "block",
                        "BLOCK_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t BLOC : {0}", block);
                    otherAddress += "bl. " + block + " ";
                }

                if (personalIdCropFilePaths.AddressCropFilePaths.BlockEntranceCropFilepath != null)
                {
                    string blockEntrance = Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.BlockEntranceCropFilepath,
                        lang, "blockEntrance",
                        "BLOCK_ENTRANCE_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t SCARA : {0}", blockEntrance);
                    otherAddress += "sc. " + blockEntrance + " ";
                }

                if (personalIdCropFilePaths.AddressCropFilePaths.ApartmentCropFilepath != null)
                {
                    string apartment = Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.ApartmentCropFilepath,
                        lang, "apartment",
                        "APARTMENT_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t APARTAMENT : {0}", apartment);
                    otherAddress += "ap. " + apartment + " ";
                }

                if (personalIdCropFilePaths.AddressCropFilePaths.FloorCropFilepath != null)
                {
                    string floor = Utils.getTextFromImageWithTesseract(
                        personalIdCropFilePaths.AddressCropFilePaths.FloorCropFilepath,
                        lang, "floor",
                        "FLOOR_out_filtered_" +
                        Path.GetFileNameWithoutExtension(personalIdCropFilePaths.PersonalIdFilepath) + ".png");
                    Console.WriteLine("\t FLOOR : {0}", floor);
                    otherAddress += "et. " + floor + " ";
                }

                personalIdModel.OtherAddress = otherAddress;
            }

            Console.WriteLine(" \n ___________________________________________");

            // set fix values
            personalIdModel.BirthCountry = "Romania";
            personalIdModel.IdType = "Buletin";
            personalIdModel.IssuingCountry = "Romania";
            personalIdModel.Country = "Romania";

            return personalIdModel;
        }
    }
}
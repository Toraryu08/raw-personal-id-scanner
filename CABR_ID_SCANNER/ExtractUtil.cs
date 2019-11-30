using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace CABR_ID_SCANNER
{
    public static class ExtractUtil
    {
        public static void extractNames(PersonalIdCropFilePaths personalIdCropFilePaths,
            string sourceFilepath, Image<Gray, Byte> sourceImage)
        {
            Image<Gray, Byte> lastNameMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\last_name.jpg") as Bitmap);
            Image<Gray, Byte> firstNameMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\first_name.jpg") as Bitmap);
            

            if (Utils.detectObject(sourceImage, lastNameMark) == false)
            {
                Console.WriteLine("Pentru fisierul {0} nu s-a gasit numele.", sourceFilepath);
                return;
            }
            if (Utils.detectObject(sourceImage, firstNameMark) == false)
            {
                Console.WriteLine("Pentru fisierul {0} nu s-a gasit prenumele.", sourceFilepath);
                return;
            }
            
            Point lastNamePosition = Utils.getObjectPositionFromImage(sourceImage, lastNameMark);
            Point firstNamePosition = Utils.getObjectPositionFromImage(sourceImage, firstNameMark);
            int nameHeight = 26;
            int nameWidth = 250;
            Rectangle lastNameRectangle = new Rectangle(lastNamePosition.X, lastNamePosition.Y + lastNameMark.Height,
                nameWidth, nameHeight);
            Rectangle firstNameRectangle = new Rectangle(firstNamePosition.X, firstNamePosition.Y + firstNameMark.Height,
                nameWidth, nameHeight);
            
            personalIdCropFilePaths.LastNameCropFilepath = Utils.createCropFromFile(sourceFilepath,
                lastNameRectangle,
                "LAST_NAME_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
            );
            personalIdCropFilePaths.FirstNameCropFilepath = Utils.createCropFromFile(sourceFilepath,
                firstNameRectangle,
                "FIRST_NAME_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
            );
        }

        public static AddressCropFilePaths extractAddress(string sourceFilePath, Image<Gray, Byte> sourceImage)
        {
            Image<Gray, Byte> markAddress =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\address_1.jpg") as Bitmap);

            if (Utils.detectObject(sourceImage, markAddress) == false)
            {
                Console.WriteLine("Pentru fisierul {0} nu s-a gasit adresa.", sourceFilePath);
                return null;
            }

            Point addressPosition = Utils.getObjectPositionFromImage(sourceImage, markAddress);

            int addressHeight = 60;
            int addressWidth = sourceImage.Width - addressPosition.X;

            String addressFirstLineCropFilepath = Utils.createCropFromFile(sourceFilePath,
                new Rectangle(addressPosition.X, addressPosition.Y + markAddress.Height, addressWidth,
                    addressHeight / 2),
                "ADDRESS_LINE1_out_" + Path.GetFileNameWithoutExtension(sourceFilePath) + ".png"
            );
            /*addressFirstLineCropFilepath = Utils.applyFilterOnImageFile(addressFirstLineCropFilepath,
                "ADDRESS_LINE1_out_f_" + Path.GetFileNameWithoutExtension(sourceFilePath) + ".png",
                100D);*/
            Image<Gray, Byte> addressFirstLineCropImage = new Image<Gray, Byte>(
                Image.FromFile(addressFirstLineCropFilepath) as Bitmap);

            String addressSecondLineCropFilepath = Utils.createCropFromFile(sourceFilePath,
                new Rectangle(addressPosition.X, addressPosition.Y + markAddress.Height + addressHeight / 2,
                    addressWidth, addressHeight / 2),
                "ADDRESS_LINE2_out_" + Path.GetFileNameWithoutExtension(sourceFilePath) + ".png"
            );
            /*addressSecondLineCropFilepath = Utils.applyFilterOnImageFile(addressSecondLineCropFilepath,
                "ADDRESS_LINE2_out_f_" + Path.GetFileNameWithoutExtension(sourceFilePath) + ".png",
                100D);*/
            Image<Gray, Byte> addressSecondLineCropImage = new Image<Gray, Byte>(
                Image.FromFile(addressSecondLineCropFilepath) as Bitmap);

            AddressCropFilePaths addressCropFilePaths = new AddressCropFilePaths();

            List<AddressElement> addressFirstLineElements = retrieveSortedElementsForFirstAdressLine(
                addressFirstLineCropImage, sourceFilePath, addressFirstLineCropFilepath);
            addressCropFilePaths = populateAddressCropFilePaths(addressCropFilePaths, addressFirstLineElements,
                addressWidth, addressHeight / 2);

            List<AddressElement> addressSecondLineElements = retrieveSortedElementsForSecondAdressLine(
                addressSecondLineCropImage, sourceFilePath, addressSecondLineCropFilepath);
            addressCropFilePaths = populateAddressCropFilePaths(addressCropFilePaths, addressSecondLineElements,
                addressWidth, addressHeight / 2);

            return addressCropFilePaths;
        }

        private static List<AddressElement> retrieveSortedElementsForFirstAdressLine(Image<Gray, Byte> addressLineImage,
            string sourceImagePath, string addressLineFilepath)
        {
            List<AddressElement> addressLineElements = new List<AddressElement>();
            
            Image<Gray, Byte> sectorMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\sec.jpg") as Bitmap);
            Image<Gray, Byte> villageMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\sat.jpg") as Bitmap);
            Image<Gray, Byte> districtMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\jud.jpg") as Bitmap);
            Image<Gray, Byte> municipalityMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\mun.jpg") as Bitmap);
            Image<Gray, Byte> cityMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\ors.jpg") as Bitmap);
            Image<Gray, Byte> streetMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\str_1.jpg") as Bitmap);

            if (Utils.detectObject(addressLineImage, villageMark))
            {
                Point villagePosition = Utils.getObjectPositionFromImage(addressLineImage, villageMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "VILLAGE_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    villagePosition.X, villageMark.Width, addressLineImage.Height,
                    AddressElementType.Village);
                addressLineElements.Add(addressElement);
            }

            if (Utils.detectObject(addressLineImage, districtMark))
            {
                Point districtPosition = Utils.getObjectPositionFromImage(addressLineImage, districtMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "DISTRICT_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    districtPosition.X, districtMark.Width, addressLineImage.Height,
                    AddressElementType.District);
                addressLineElements.Add(addressElement);
            }
            
            if (Utils.detectObject(addressLineImage, municipalityMark))
            {
                Point municipalityPosition = Utils.getObjectPositionFromImage(addressLineImage, municipalityMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "MUNICIPALITY_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    municipalityPosition.X, municipalityMark.Width, addressLineImage.Height,
                    AddressElementType.Municipality);
                addressLineElements.Add(addressElement);
            }

            if (Utils.detectObject(addressLineImage, sectorMark))
            {
                Point sectorPosition = Utils.getObjectPositionFromImage(addressLineImage, sectorMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "SECTOR_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    sectorPosition.X, sectorMark.Width, addressLineImage.Height,
                    AddressElementType.Sector);
                addressLineElements.Add(addressElement);
            }
            
            if (Utils.detectObject(addressLineImage, cityMark))
            {
                Point cityPosition = Utils.getObjectPositionFromImage(addressLineImage, cityMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "CITY_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    cityPosition.X, cityMark.Width, addressLineImage.Height,
                    AddressElementType.City);
                addressLineElements.Add(addressElement);
            }

            if (Utils.detectObject(addressLineImage, streetMark))
            {
                Point streetPosition = Utils.getObjectPositionFromImage(addressLineImage, streetMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "STREET_out_1_" + Path.GetFileNameWithoutExtension(addressLineFilepath) + ".png",
                    streetPosition.X, streetMark.Width, addressLineImage.Height,
                    AddressElementType.Street);
                addressLineElements.Add(addressElement);
            }
            
            addressLineElements.Sort((addressElementFirst, addressElementSecond) =>
                addressElementFirst.MarkXPosition.CompareTo(addressElementSecond.MarkXPosition)
            );

            return addressLineElements;
        }

        private static List<AddressElement> retrieveSortedElementsForSecondAdressLine(Image<Gray, Byte> addressLineImage,
            string sourceImagePath, string addressLineFilepath)
        {
            List<AddressElement> addressLineElements = new List<AddressElement>();

            Image<Gray, Byte> alleyMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\ale.jpg") as Bitmap);
            Image<Gray, Byte> boulevardMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\bld.jpg") as Bitmap);
            Image<Gray, Byte> apartmentMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\ap.jpg") as Bitmap);
            Image<Gray, Byte> blockMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\bl.jpg") as Bitmap);
            Image<Gray, Byte> streetNumberMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\nr.jpg") as Bitmap);
            Image<Gray, Byte> blockEntranceMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\sc.jpg") as Bitmap);
            Image<Gray, Byte> drivewayMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\sos.jpg") as Bitmap);
            Image<Gray, Byte> floorMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\et.jpg") as Bitmap);
            Image<Gray, Byte> streetMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images2\str_1.jpg") as Bitmap);

            if (Utils.detectObject(addressLineImage, alleyMark))
            {
                Point alleyPosition = Utils.getObjectPositionFromImage(addressLineImage, alleyMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "ALLEY_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    alleyPosition.X, alleyMark.Width, addressLineImage.Height,
                    AddressElementType.Alley);
                addressLineElements.Add(addressElement);
            }
            
            if (Utils.detectObject(addressLineImage, boulevardMark))
            {
                Point boulevardPosition = Utils.getObjectPositionFromImage(addressLineImage, boulevardMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "BOULEVARD_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    boulevardPosition.X, boulevardMark.Width, addressLineImage.Height,
                    AddressElementType.Boulevard);
                addressLineElements.Add(addressElement);
            }
            
            if (Utils.detectObject(addressLineImage, apartmentMark))
            {
                Point apartmentPosition = Utils.getObjectPositionFromImage(addressLineImage, apartmentMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "APARTMENT_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    apartmentPosition.X, apartmentMark.Width, addressLineImage.Height,
                    AddressElementType.Apartment);
                addressLineElements.Add(addressElement);
            }

            if (Utils.detectObject(addressLineImage, blockMark))
            {
                Point blockPosition = Utils.getObjectPositionFromImage(addressLineImage, blockMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "BLOCK_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    blockPosition.X, blockMark.Width, addressLineImage.Height,
                    AddressElementType.Block);
                addressLineElements.Add(addressElement);
            }

            if (Utils.detectObject(addressLineImage, streetNumberMark))
            {
                Point streetNumberPosition = Utils.getObjectPositionFromImage(addressLineImage, streetNumberMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "STREET_NUMBER_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    streetNumberPosition.X, streetNumberMark.Width, addressLineImage.Height,
                    AddressElementType.StreetNumber);
                addressLineElements.Add(addressElement);
            }

            if (Utils.detectObject(addressLineImage, blockEntranceMark))
            {
                Point blockEntrancePosition = Utils.getObjectPositionFromImage(addressLineImage, blockEntranceMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "BLOCK_ENTRANCE_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    blockEntrancePosition.X, blockEntranceMark.Width, addressLineImage.Height,
                    AddressElementType.BlockEntrance);
                addressLineElements.Add(addressElement);
            }
            
            if (Utils.detectObject(addressLineImage, drivewayMark))
            {
                Point drivewayPosition = Utils.getObjectPositionFromImage(addressLineImage, drivewayMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "DRIVEWAY_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    drivewayPosition.X, drivewayMark.Width, addressLineImage.Height,
                    AddressElementType.Driveway);
                addressLineElements.Add(addressElement);
            }

            if (Utils.detectObject(addressLineImage, floorMark))
            {
                Point floorPosition = Utils.getObjectPositionFromImage(addressLineImage, floorMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "FLOOR_out_" + Path.GetFileNameWithoutExtension(sourceImagePath) + ".png",
                    floorPosition.X, floorMark.Width, addressLineImage.Height,
                    AddressElementType.Floor);
                addressLineElements.Add(addressElement);
            }

            if (Utils.detectObject(addressLineImage, streetMark))
            {
                Point streetPosition = Utils.getObjectPositionFromImage(addressLineImage, streetMark);
                AddressElement addressElement = new AddressElement(addressLineFilepath,
                    "STREET_out_2_" + Path.GetFileNameWithoutExtension(addressLineFilepath) + ".png",
                    streetPosition.X, streetMark.Width, addressLineImage.Height,
                    AddressElementType.Street);
                addressLineElements.Add(addressElement);
            }

            addressLineElements.Sort((addressElementFirst, addressElementSecond) =>
                addressElementFirst.MarkXPosition.CompareTo(addressElementSecond.MarkXPosition)
            );

            return addressLineElements;
        }

        private static AddressCropFilePaths populateAddressCropFilePaths(AddressCropFilePaths addressCropFilePaths,
            List<AddressElement> addressElements, int addressLineWidth, int addressLineHeight)
        {
            for (int i = 0; i < addressElements.Count - 1; i++)
            {
                AddressElement currentAddressElement = addressElements[i];
                AddressElement nextAddressElement = addressElements[i + 1];
                int elementX = currentAddressElement.MarkXPosition + currentAddressElement.MarkWidth;
                int elementWidth = nextAddressElement.MarkXPosition - elementX;

                string outputCropFilepath = Utils.createCropFromFile(
                    currentAddressElement.SourceFilepath,
                    new Rectangle(elementX, 0, elementWidth, addressLineHeight),
                    currentAddressElement.OutputCropFilepath);

                switch (currentAddressElement.ElementType)
                {
                    case AddressElementType.Alley:
                        addressCropFilePaths.AlleyCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.Apartment:
                        addressCropFilePaths.ApartmentCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.Block:
                        addressCropFilePaths.BlockCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.City:
                        addressCropFilePaths.CityCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.District:
                        addressCropFilePaths.DistrictCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.Municipality:
                        addressCropFilePaths.MunicipalityCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.Sector:
                        addressCropFilePaths.SectorCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.Street:
                        addressCropFilePaths.StreetCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.BlockEntrance:
                        addressCropFilePaths.BlockEntranceCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.StreetNumber:
                        addressCropFilePaths.StreetNumberCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.Driveway:
                        addressCropFilePaths.DrivewayCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.Floor:
                        addressCropFilePaths.FloorCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.Village:
                        addressCropFilePaths.VillageCropFilepath = outputCropFilepath;
                        break;
                    case AddressElementType.Boulevard:
                        addressCropFilePaths.BoulevardCropFilepath = outputCropFilepath;
                        break;
                }
            }

            if (addressElements.Count > 0)
            {
                AddressElement addressElement = addressElements[addressElements.Count - 1];
                string finalCropFilepath = Utils.createCropFromFile(
                    addressElement.SourceFilepath,
                    new Rectangle(addressElement.MarkXPosition + addressElement.MarkWidth, 0,
                        addressLineWidth - (addressElement.MarkXPosition + addressElement.MarkWidth),
                        addressLineHeight),
                    addressElement.OutputCropFilepath);

                switch (addressElement.ElementType)
                {
                    case AddressElementType.Alley:
                        addressCropFilePaths.AlleyCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.Apartment:
                        addressCropFilePaths.ApartmentCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.Block:
                        addressCropFilePaths.BlockCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.City:
                        addressCropFilePaths.CityCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.District:
                        addressCropFilePaths.DistrictCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.Municipality:
                        addressCropFilePaths.MunicipalityCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.Sector:
                        addressCropFilePaths.SectorCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.Street:
                        addressCropFilePaths.StreetCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.BlockEntrance:
                        addressCropFilePaths.BlockEntranceCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.StreetNumber:
                        addressCropFilePaths.StreetNumberCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.Driveway:
                        addressCropFilePaths.DrivewayCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.Floor:
                        addressCropFilePaths.FloorCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.Village:
                        addressCropFilePaths.VillageCropFilepath = finalCropFilepath;
                        break;
                    case AddressElementType.Boulevard:
                        addressCropFilePaths.BoulevardCropFilepath = finalCropFilepath;
                        break;
                }
            }

            return addressCropFilePaths;
        }

        public static void extractGender(PersonalIdCropFilePaths personalIdCropFilePaths,
            string sourceFilepath, Image<Gray, Byte> sourceImage)
        {
            Image<Gray, Byte> genderMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\gender_1.jpg") as Bitmap);
            if (Utils.detectObject(sourceImage, genderMark))
            {
                Point genderPosition = Utils.getObjectPositionFromImage(sourceImage, genderMark);
                int genderHeight = 25;
                Rectangle genderRectangle = new Rectangle(genderPosition.X, genderPosition.Y + genderMark.Height,
                    genderMark.Width, genderHeight);
                string genderCropFilepath = Utils.createCropFromFile(sourceFilepath,
                    genderRectangle,
                    "GENDER_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
                );

                personalIdCropFilePaths.GenderCropFilepath = genderCropFilepath;
            }
        }

        public static void extractBirthplace(PersonalIdCropFilePaths personalIdCropFilePaths,
            string sourceFilepath, Image<Gray, Byte> sourceImage)
        {
            Image<Gray, Byte> birthplaceMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\birthplace_1.jpg") as Bitmap);
            if (Utils.detectObject(sourceImage, birthplaceMark))
            {
                Point birthplacePosition = Utils.getObjectPositionFromImage(sourceImage, birthplaceMark);
                int birthplaceWidth = 340;
                int birthplaceHeight = 27;
                Rectangle birthplaceRectangle = new Rectangle(birthplacePosition.X,
                    birthplacePosition.Y + birthplaceMark.Height,
                    birthplaceWidth, birthplaceHeight);
                string birthplaceCropFilepath = Utils.createCropFromFile(sourceFilepath,
                    birthplaceRectangle,
                    "BIRTHPLACE_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
                );

                personalIdCropFilePaths.BirthplaceCropFilepath = birthplaceCropFilepath;
            }
        }

        public static void extractIssuedBy(PersonalIdCropFilePaths personalIdCropFilePaths,
            string sourceFilepath, Image<Gray, Byte> sourceImage)
        {
            Image<Gray, Byte> issuedByMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\issued._1.jpg") as Bitmap);
            
            if (Utils.detectObject(sourceImage, issuedByMark))
            {
                Point issuedByPosition = Utils.getObjectPositionFromImage(sourceImage, issuedByMark);
                int issuedByHeight = 25;
                int issuedByWidth = 255;
                Rectangle issuedByRectangle = new Rectangle(issuedByPosition.X,
                    issuedByPosition.Y + issuedByMark.Height,
                    issuedByWidth, issuedByHeight);
                string issuedByCropFilepath = Utils.createCropFromFile(sourceFilepath,
                    issuedByRectangle,
                    "ISSUED_BY_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
                );

                personalIdCropFilePaths.IssuedByCropFilepath = issuedByCropFilepath;
            }
        }

        public static void extractValidity(PersonalIdCropFilePaths personalIdCropFilePaths,
            string sourceFilepath, Image<Gray, Byte> sourceImage)
        {
            Image<Gray, Byte> validityMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\validity_1.jpg") as Bitmap);
            Image<Gray, Byte> dashMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\dash.jpg") as Bitmap);

            if (Utils.detectObject(sourceImage, validityMark))
            {
                Point validityPosition = Utils.getObjectPositionFromImage(sourceImage, validityMark);
                int validityWidth = 250;
                int validityHeight = 27;
                int validityX = validityPosition.X + validityMark.Width - validityWidth;
                Rectangle validityRectangle = new Rectangle(validityX, validityPosition.Y + validityMark.Height,
                    validityWidth, validityHeight);

                personalIdCropFilePaths.ValidityCropFilepath = Utils.createCropFromFile(sourceFilepath,
                    validityRectangle,
                    "VALIDITY_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
                );

                int expireWidth = 135;
                int emittedWidth = 100;
                Rectangle expireRectangle = new Rectangle(validityPosition.X + validityMark.Width - expireWidth,
                    validityPosition.Y + validityMark.Height, expireWidth, validityHeight);
                Rectangle emittedRectangle = new Rectangle(
                    validityPosition.X + validityMark.Width - expireWidth - dashMark.Width - emittedWidth,
                    validityPosition.Y + validityMark.Height, emittedWidth, validityHeight);
                personalIdCropFilePaths.ExpireDateCropFilepath = Utils.createCropFromFile(sourceFilepath,
                    expireRectangle,
                    "EXPIRE_DATE_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
                );
                personalIdCropFilePaths.EmittedDateCropFilepath = Utils.createCropFromFile(sourceFilepath,
                    emittedRectangle,
                    "EMIT_DATE_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
                );
            }
        }

        public static void extractNationality(PersonalIdCropFilePaths personalIdCropFilePaths,
            string sourceFilepath, Image<Gray, Byte> sourceImage)
        {
            Image<Gray, Byte> nationalityMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\nationality_1.jpg") as Bitmap);
            Image<Gray, Byte> slashMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\slash.jpg") as Bitmap);

            if (Utils.detectObject(sourceImage, nationalityMark))
            {
                Point nationalityPosition = Utils.getObjectPositionFromImage(sourceImage, nationalityMark);
                int nationalityWidth = 95;
                int nationalityHeight = 27;
                Rectangle nationalityRectangle = new Rectangle(nationalityPosition.X,
                    nationalityPosition.Y + nationalityMark.Height,
                    nationalityWidth, nationalityHeight);

                personalIdCropFilePaths.NationalityCropFilepath = Utils.createCropFromFile(sourceFilepath,
                    nationalityRectangle,
                    "NATIONALITY_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
                );
                /*nationalityCropFilepath = Utils.applyFilterOnImageFile(nationalityCropFilepath,
                    "NATIONALITY_out_f_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png");
                Image<Gray, byte> nationalityCropImage =
                    new Image<Gray, Byte>(Image.FromFile(nationalityCropFilepath) as Bitmap);

                if (Utils.detectObject(nationalityCropImage, slashMark) == false)
                {
                    Console.WriteLine(
                        "Nu s-a gasit slash-ul in campul nationalitate pentru fisierul {0}",
                        sourceFilepath);
                    return;
                }

                Point slashPosition = Utils.getObjectPositionFromImage(nationalityCropImage, slashMark);
                Rectangle nationalityMaxRectangle = new Rectangle(0, 0, slashPosition.X, nationalityCropImage.Height);
                Rectangle nationalityMinRectangle = new Rectangle(slashPosition.X + slashMark.Width, 0,
                    nationalityCropImage.Width - (slashPosition.X + slashMark.Width), nationalityCropImage.Height);

                personalIdCropFilePaths.NationalityCropFilepath = Utils.createCropFromFile(nationalityCropFilepath,
                    new Rectangle(0,0,nationalityWidth,nationalityHeight), 
                    "NATIONALITY_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
                );*/
            }
        }

        public static void extractIdNumber(PersonalIdCropFilePaths personalIdCropFilePaths,
            string sourceFilepath, Image<Gray, Byte> sourceImage)
        {
            Image<Gray, Byte> idNumberMark =
                new Image<Gray, Byte>(
                    Image.FromFile(@"C:\Users\AndreiStoica\CABR_ID_SCANNER\images\images1\idNR.jpg") as Bitmap);
            if (Utils.detectObject(sourceImage, idNumberMark))
            {
                Point idNumberPosition = Utils.getObjectPositionFromImage(sourceImage, idNumberMark);
                int idNumberHeight = 24;
                int idNumberWidth = 100;
                Rectangle idNumberRectangle = new Rectangle(
                    idNumberPosition.X + idNumberMark.Width,
                    idNumberPosition.Y + idNumberMark.Height - idNumberHeight,
                    idNumberWidth, idNumberHeight);
                string idNumberCropFilepath = Utils.createCropFromFile(sourceFilepath,
                    idNumberRectangle,
                    "ID_NUMBER_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png"
                );

                personalIdCropFilePaths.IdNumberCropFilepath = idNumberCropFilepath;
            }
        }

        private static List<Point> extractArrowPosition(string idRouFilepath, Image<Gray, byte> idRouRowImage,
            Image<Gray, byte> oneArrowMark, string sourceFilepath)
        {
            List<Point> arrowPositions = new List<Point>(4);
            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            double searchThreshold = 0.5;

            Image<Gray, float> firstPositionResult =
                idRouRowImage.MatchTemplate(oneArrowMark, TemplateMatchingType.CcoeffNormed);
            firstPositionResult.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
            if (maxValues[0] > searchThreshold)
            {
                arrowPositions.Add(maxLocations[0]);
            }
            else
            {
                Console.WriteLine("Nu s-a gasit prima sageata pentru {0}", sourceFilepath);
                return arrowPositions;
            }

            Rectangle step1Rectangle = new Rectangle(arrowPositions[0].X + oneArrowMark.Width, 0,
                idRouRowImage.Width - (arrowPositions[0].X + oneArrowMark.Width), idRouRowImage.Height);
            string step1CropFilepath = Utils.createCropFromFile(idRouFilepath, step1Rectangle,
                "IDROU_STEP1_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png");
            Image<Gray, Byte> step1CropImage = new Image<Gray, Byte>(Image.FromFile(step1CropFilepath) as Bitmap);

            Image<Gray, float> secondPositionResult =
                step1CropImage.MatchTemplate(oneArrowMark, TemplateMatchingType.CcoeffNormed);
            secondPositionResult.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
            if (maxValues[0] > searchThreshold)
            {
                arrowPositions.Add(maxLocations[0]);
            }
            else
            {
                Console.WriteLine("Nu s-a gasit a doua sageata pentru {0}", sourceFilepath);
                return arrowPositions;
            }

            Rectangle step2Rectangle = new Rectangle(arrowPositions[1].X + oneArrowMark.Width, 0,
                step1CropImage.Width - (arrowPositions[1].X + oneArrowMark.Width), step1CropImage.Height);
            string step2CropFilepath = Utils.createCropFromFile(step1CropFilepath, step2Rectangle,
                "IDROU_STEP2_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png");
            Image<Gray, Byte> step2CropImage = new Image<Gray, Byte>(Image.FromFile(step2CropFilepath) as Bitmap);

            Image<Gray, float> thirdPositionResult =
                step2CropImage.MatchTemplate(oneArrowMark, TemplateMatchingType.CcoeffNormed);
            thirdPositionResult.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
            if (maxValues[0] > searchThreshold)
            {
                arrowPositions.Add(maxLocations[0]);
            }
            else
            {
                Console.WriteLine("Nu s-a gasit a treia sageata pentru {0}", sourceFilepath);
                return arrowPositions;
            }

            Rectangle step3Rectangle = new Rectangle(arrowPositions[2].X + oneArrowMark.Width, 0,
                step2CropImage.Width - (arrowPositions[2].X + oneArrowMark.Width), step2CropImage.Height);
            string step3CropFilepath = Utils.createCropFromFile(step2CropFilepath, step3Rectangle,
                "IDROU_STEP3_out_" + Path.GetFileNameWithoutExtension(sourceFilepath) + ".png");
            Image<Gray, Byte> step3CropImage = new Image<Gray, Byte>(Image.FromFile(step3CropFilepath) as Bitmap);

            Image<Gray, float> fourthPositionResult =
                step3CropImage.MatchTemplate(oneArrowMark, TemplateMatchingType.CcoeffNormed);
            fourthPositionResult.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
            if (maxValues[0] > searchThreshold)
            {
                arrowPositions.Add(maxLocations[0]);
            }
            else
            {
                Console.WriteLine("Nu s-a gasit a patra sageata pentru {0}", sourceFilepath);
                return arrowPositions;
            }

            firstPositionResult.Dispose();
            secondPositionResult.Dispose();
            thirdPositionResult.Dispose();
            fourthPositionResult.Dispose();
            step1CropImage.Dispose();
            step2CropImage.Dispose();
            step3CropImage.Dispose();

            return arrowPositions;
        }
    }
}
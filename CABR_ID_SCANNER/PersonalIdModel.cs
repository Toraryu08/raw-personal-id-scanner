using System.Collections.Generic;

namespace CABR_ID_SCANNER
{
    public class PersonalIdModel
    {
        private string firstName;
        private string middleName;
        private string lastName;
        private string cnp;
        private string gender;
        private string birthPlace;
        private string birthCountry;
        private string dateOfBirth;
        private string nationality;
        private string idType;
        private string idNumber;
        private string issueDate;
        private string expiryDate;
        private string issuingCountry;
        private string issuingEntity;
        private string streetNumber;
        private string streetName;
        private string otherAddress;
        private string region;
        private string city;
        private string country;

        public string FirstName
        {
            get => firstName;
            set => firstName = value;
        }

        public string MiddleName
        {
            get => middleName;
            set => middleName = value;
        }

        public string LastName
        {
            get => lastName;
            set => lastName = value;
        }

        public string Cnp
        {
            get => cnp;
            set => cnp = value;
        }

        public string Gender
        {
            get => gender;
            set => gender = value;
        }

        public string BirthPlace
        {
            get => birthPlace;
            set => birthPlace = value;
        }

        public string BirthCountry
        {
            get => birthCountry;
            set => birthCountry = value;
        }

        public string DateOfBirth
        {
            get => dateOfBirth;
            set => dateOfBirth = value;
        }

        public string Nationality
        {
            get => nationality;
            set => nationality = value;
        }

        public string IdType
        {
            get => idType;
            set => idType = value;
        }

        public string IdNumber
        {
            get => idNumber;
            set => idNumber = value;
        }

        public string IssueDate
        {
            get => issueDate;
            set => issueDate = value;
        }

        public string ExpiryDate
        {
            get => expiryDate;
            set => expiryDate = value;
        }

        public string IssuingCountry
        {
            get => issuingCountry;
            set => issuingCountry = value;
        }

        public string IssuingEntity
        {
            get => issuingEntity;
            set => issuingEntity = value;
        }

        public string StreetNumber
        {
            get => streetNumber;
            set => streetNumber = value;
        }

        public string StreetName
        {
            get => streetName;
            set => streetName = value;
        }

        public string OtherAddress
        {
            get => otherAddress;
            set => otherAddress = value;
        }

        public string Region
        {
            get => region;
            set => region = value;
        }

        public string City
        {
            get => city;
            set => city = value;
        }

        public string Country
        {
            get => country;
            set => country = value;
        }

        public List<string[]> CreateExcelRow()
        {
            string[] excelRowContent = new string[21];

            excelRowContent[0] = firstName ?? "";
            excelRowContent[1] = middleName ?? "";
            excelRowContent[2] = lastName ?? "";
            excelRowContent[3] = cnp ?? "";
            excelRowContent[4] = gender ?? "";
            excelRowContent[5] = birthPlace ?? "";
            excelRowContent[6] = birthCountry ?? "";
            excelRowContent[7] = dateOfBirth ?? "";
            excelRowContent[8] = nationality ?? "";
            excelRowContent[9] = idType ?? "";
            excelRowContent[10] = idNumber ?? "";
            excelRowContent[11] = issueDate ?? "";
            excelRowContent[12] = expiryDate ?? "";
            excelRowContent[13] = issuingCountry ?? "";
            excelRowContent[14] = issuingEntity ?? "";
            excelRowContent[15] = streetNumber ?? "";
            excelRowContent[16] = streetName ?? "";
            excelRowContent[17] = otherAddress ?? "";
            excelRowContent[18] = region ?? "";
            excelRowContent[19] = city ?? "";
            excelRowContent[20] = country ?? "";
            
            List<string[]> row = new List<string[]>();
            row.Add(excelRowContent);
            return row;
        }
    }
}